using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Midix.Data;
using Midix.IRepository.IPatient;
using Midix.Models;
using Midix.ViewModel;
using Microsoft.AspNetCore.SignalR;
using Midix.Hubs;
using Midix.Helpers;
using Midix.DTO.PatientDashboardDTOs;
using Midix.DTO;
using Midix.Enums;

namespace Midix.Repository.PatientRepositories
{


    public class PatientBookingRepository : PatientRepositoryBase, IPatientBookingRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public PatientBookingRepository(ApplicationDbContext context, IConfiguration configuration, IHubContext<AppointmentHub> hubContext)
            : base(context)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }


        public async Task<BookAppointmentViewModel> GetBookingDataAsync(string patientId)
        {
            var header = await LoadHeaderAsync(patientId);

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.Ratings)
                .Where(d => d.User.IsActive)
                .OrderBy(d => d.User.LastName)
                .ToListAsync();

            var doctorCards = doctors.Select(d => new DoctorCardDto
            {
                UserId = d.UserId,
                FullName = $"Dr. {d.User.FirstName} {d.User.LastName}",
                Initials = Midix.Helpers.UserHelper.BuildInitials(d.User.FirstName, d.User.LastName),
                Specialization = d.Specialization?.Name ?? "",
                ConsultationFee = d.ConsultationFee,
                FollowUpFee = d.FollowUpFee,
                AverageRating = d.Ratings.Any() ? Math.Round(d.Ratings.Average(r => r.Rate), 1) : 0,
                RatingCount = d.Ratings.Count,
                Bio = d.Bio,
                ClinicAddress = d.ClinicAddress
            }).ToList();

            var vm = new BookAppointmentViewModel { AvailableDoctors = doctorCards };
            CopyHeader(header, vm);
            return vm;
        }

        // ── Booking actions ────────────────────────────────────────────────────
        public async Task<(BookingResult Result, Appointment? Appointment)> BookAppointmentAsync(
     string patientId, string doctorId, DateTime date, PaymentMethod paymentMethod, AppointmentType appointmentType)
        {
            try
            {
                var maxDays = _configuration.GetValue<int>("BookingSettings:MaxDays");

                if (date.Date > DateTime.UtcNow.Date.AddDays(maxDays))
                    return (BookingResult.InvalidDate, null);

                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorId);
                if (doctor == null) return (BookingResult.DoctorNotFound, null);

                var fee = appointmentType == AppointmentType.FollowUp
                    ? (doctor.FollowUpFee > 0 ? doctor.FollowUpFee : doctor.ConsultationFee)
                    : doctor.ConsultationFee;

                var appointment = new Appointment
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    Date = date,
                    AppointmentType = appointmentType,   
                    State = AppointmentState.Pending,
                    Payment = new Payment
                    {
                        Amount = fee,                   
                        Tax = 0,
                        Method = paymentMethod,
                        Status = paymentMethod == PaymentMethod.Card
                                    ? PaymentStatus.Paid
                                    : PaymentStatus.Pending
                    }
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                var patient = await _context.Users.FindAsync(patientId);

                await _hubContext.Clients.Group(doctorId).SendAsync("NewAppointmentBooked", new
                {
                    AppointmentId = appointment.Id,
                    PatientName = $"{patient?.FirstName} {patient?.LastName}",
                    Initials = UserHelper.BuildInitials(patient?.FirstName ?? "", patient?.LastName ?? ""),
                    TimeLabel = appointment.Date.ToString("h:mm tt"),
                    Reason = appointmentType == AppointmentType.FollowUp ? "Follow-Up" : "New Visit",
                    State = appointment.State.ToString()
                }); 
                
                return (BookingResult.Success, appointment);
            }
            catch
            {
                return (BookingResult.UnExpectedEror, null);
            }
        }

        public async Task<SlotResultDto> GetAvailableSlotsAsync(string doctorId, DateTime selectedDate)
        {
            var result = new SlotResultDto();

            var maxDays = _configuration.GetValue<int>("BookingSettings:MaxDays");
            var now = DateTime.UtcNow;
            var today = now.Date;

            if (selectedDate.Date < today || selectedDate.Date > today.AddDays(maxDays))
                return result;

            var isToday = selectedDate.Date == now.Date;
            var cutoffTime = isToday
                ? TimeOnly.FromDateTime(now.AddMinutes(30))
                : new TimeOnly(0, 0);

            var workingHours = await _context.WorkingHours
                .Where(w => w.DoctorId == doctorId && w.Day.Date == selectedDate.Date)
                .ToListAsync();

            if (!workingHours.Any()) return result;

            foreach (var wh in workingHours)
            {
                var cur = wh.StartTime;
                while (cur < wh.EndTime)
                {
                    if (!isToday || cur > cutoffTime)
                        result.AvailableSlots.Add(cur.ToString("HH:mm"));
                    cur = cur.AddMinutes(30);
                }
            }

            var dayStart = selectedDate.Date;
            var dayEnd = dayStart.AddDays(1);

            var booked = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.Date >= dayStart
                         && a.Date < dayEnd
                         && a.State != AppointmentState.Cancelled)
                .Select(a => a.Date)
                .ToListAsync();

            result.BookedSlots = booked.Select(d => d.ToString("HH:mm")).ToList();
            return result;
        }
    }
}