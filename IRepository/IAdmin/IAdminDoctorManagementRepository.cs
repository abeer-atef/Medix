using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminDoctorManagementRepository
    {
        Task<EditDoctorViewModel?> GetDoctorForEditAsync(string userId);
        Task<bool> UpdateDoctorAsync(EditDoctorViewModel model);
        Task<bool> ToggleDoctorActiveAsync(string userId);
        Task<int> GetUpcomingAppointmentsCountAsync(string doctorUserId);
        Task CancelDoctorAppointmentsAsync(string doctorUserId);
    }
}
