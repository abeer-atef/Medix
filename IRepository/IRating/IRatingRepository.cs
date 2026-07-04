using Midix.Models;

namespace Midix.IRepository.IRating
{
    public interface IRatingRepository
    {
        Task<IEnumerable<Appointment>> GetRateableDoctorsAsync(string patientId);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task<Rating?> GetByAppointmentIdAsync(int appointmentId);
        Task AddAsync(Rating rating);
    }
}