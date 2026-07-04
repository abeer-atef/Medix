using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.Models;

namespace Midix.Repository.DoctorRepositories
{
    public abstract class DoctorRepositoryBase
    {
        protected readonly ApplicationDbContext _context;

        protected DoctorRepositoryBase(ApplicationDbContext context)
        {
            _context = context;
        }

        protected async Task<(Doctor? doctor, string name, string initials, string spec)> LoadHeaderAsync(string doctorUserId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            if (doctor == null) return (null, "", "", "");

            var name = $"Dr. {doctor.User.FirstName} {doctor.User.LastName}";
            var initials = $"{doctor.User.FirstName[0]}{doctor.User.LastName[0]}".ToUpper();
            var spec = doctor.Specialization?.Name ?? "";
            return (doctor, name, initials, spec);
        }

        protected void LogStateChange(Appointment apt, AppointmentState newState)
        {
            _context.AppointmentStateChanges.Add(new AppointmentStateChange
            {
                AppointmentId = apt.Id,
                OldState = apt.State,   
                NewState = newState,
                Date = DateTime.UtcNow
            });
        }
    }
}
