using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.ViewModel;
using Midix.Enums;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorScheduleRepository : DoctorRepositoryBase, IDoctorScheduleRepository
    {

        public DoctorScheduleRepository(ApplicationDbContext context): base(context) { }

        public async Task<DoctorScheduleViewModel> GetScheduleAsync(string doctorUserId)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayApts = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.MedicalRecord)
                .Include(a => a.Payment)
                .Where(a => a.DoctorId == doctorUserId
                         && a.Date >= today && a.Date < tomorrow
                         && a.State != AppointmentState.Cancelled)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var queue = todayApts.Select((a, i) =>
            {
                var fn = a.Patient?.User?.FirstName ?? "";
                var ln = a.Patient?.User?.LastName ?? "";
                return new AppointmentQueueDto
                {
                    Id = a.Id,
                    QueueNumber = i + 1,
                    PatientName = $"{fn} {ln}".Trim(),
                    Initials = fn.Length > 0 && ln.Length > 0 ? $"{fn[0]}{ln[0]}".ToUpper() : "??",
                    TimeLabel = a.Date.ToString("h:mm tt"),
                    Reason = a.MedicalRecord?.Diagnosis ?? "Consultation",
                    State = a.State,
                    PaymentMethod = a.Payment?.Method ?? PaymentMethod.Cash
                };
            }).ToList();

            return new DoctorScheduleViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                TodayTotal = todayApts.Count,
                TodayCompleted = todayApts.Count(a => a.State == AppointmentState.Done),
                TodayRemaining = todayApts.Count(a => a.State == AppointmentState.Pending || a.State == AppointmentState.Confirmed || a.State == AppointmentState.InProgress),
                TodayQueue = queue
            };
        }
        public async Task<bool> StartAppointmentAsync(string doctorUserId, int appointmentId)
        {
            var apt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId
                                       && a.DoctorId == doctorUserId
                                      );
            if (apt == null) return false;

            LogStateChange(apt, AppointmentState.InProgress); 
            apt.State = AppointmentState.InProgress;         
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(string doctorUserId, int appointmentId)
        {
            var apt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorUserId);
            if (apt == null) return false;


            LogStateChange(apt, AppointmentState.Done);
            apt.State = AppointmentState.Done;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAppointmentAsync(string doctorUserId, int appointmentId)
        {
            var apt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorUserId
                                       && a.State != AppointmentState.Done);
            if (apt == null) return false;


            LogStateChange(apt, AppointmentState.Cancelled);
            apt.State = AppointmentState.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
