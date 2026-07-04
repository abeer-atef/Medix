using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.IRepository.IPatient;
using Midix.ViewModel;

namespace Midix.Repository.PatientRepositories
{
    public class PatientOverviewRepository : PatientRepositoryBase, IPatientOverviewRepository
    {
        public PatientOverviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PatientOverviewViewModel> GetOverviewAsync(string patientId)
        {
            var header = await LoadHeaderAsync(patientId);
            var now = DateTime.UtcNow;

            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Payment)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // تصحيح التكرار: دمج الشروط في سطر واحد
            var upcomingTop = appointments
                .Where(a => a.Date >= now && a.State != AppointmentState.Cancelled)
                .Where(a => a.Date >= now && a.State != AppointmentState.Cancelled && a.State != AppointmentState.Done)
                .OrderBy(a => a.Date)
                .Take(3)
                .Select(MapAppointmentRow)
                .ToList();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.MedicalRecord).ThenInclude(mr => mr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(p => p.MedicalRecord).ThenInclude(mr => mr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(p => p.Medicines)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var recentRx = prescriptions.Take(3).Select(MapPrescription).ToList();

            var vm = new PatientOverviewViewModel
            {
                UpcomingAppointmentsCount = appointments.Count(a => a.Date >= now && a.State != AppointmentState.Cancelled && a.State != AppointmentState.Done),
                PastVisitsCount = appointments.Count(a => a.Date < now || a.State == AppointmentState.Done),
                TotalPrescriptionsCount = prescriptions.Count,
                MedicationsDueCount = prescriptions.Sum(p => p.Medicines?.Count ?? 0),
                UpcomingAppointments = upcomingTop,
                RecentPrescriptions = recentRx
            };

            CopyHeader(header, vm);
            return vm;
        }
    }
}