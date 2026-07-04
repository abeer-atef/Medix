using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.IRepository.IRating;
using Midix.Models;

namespace Midix.Repository.RatingRepositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public RatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetRateableDoctorsAsync(string patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Include(a => a.Rating)
                .Where(a =>
                    a.PatientId == patientId &&
                    a.State == AppointmentState.Done &&
                    a.Rating == null)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Rating?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Ratings
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task AddAsync(Rating rating)
        {
            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
    }

}
