using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorScheduleRepository
    {
        Task<DoctorScheduleViewModel> GetScheduleAsync(string doctorUserId);
        Task<bool> StartAppointmentAsync(string doctorUserId, int appointmentId);
        Task<bool> CompleteAppointmentAsync(string doctorUserId, int appointmentId);
        Task<bool> CancelAppointmentAsync(string doctorUserId, int appointmentId);
    }
}
