using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorOverviewRepository : DoctorRepositoryBase, IDoctorOverviewRepository
    {
        public DoctorOverviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DoctorOverviewViewModel> GetOverviewAsync(string doctorUserId)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayApts = await _context.Appointments
                .Where(a => a.DoctorId == doctorUserId
                         && a.Date >= today && a.Date < tomorrow
                         && a.State != AppointmentState.Cancelled)
                .Select(a => a.State)
                .ToListAsync();

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            return new DoctorOverviewViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                TodayTotal = todayApts.Count,
                TodayCompleted = todayApts.Count(s => s == AppointmentState.Done),
                TodayRemaining = todayApts.Count(s => s == AppointmentState.Pending || s == AppointmentState.Confirmed),
                AvgVisitMinutes = 30,
                Picture = doctor?.User?.Picture
            };
        }
    }
}