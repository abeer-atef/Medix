using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminAppointmentsRepository
    {
        Task<AppointmentsViewModel> GetAppointmentsAsync(int page = 1, int pageSize = 10);
    }
}
