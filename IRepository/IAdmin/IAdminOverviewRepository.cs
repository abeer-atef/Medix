using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminOverviewRepository
    {
        Task<AdminDashboardViewModel> GetOverviewAsync();

    }
}
