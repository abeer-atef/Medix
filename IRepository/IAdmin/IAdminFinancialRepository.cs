using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminFinancialRepository
    {
        Task<FinancialViewModel> GetFinancialAsync(int page = 1, int pageSize = 10);
    }
}
