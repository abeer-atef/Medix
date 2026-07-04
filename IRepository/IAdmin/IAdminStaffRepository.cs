using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminStaffRepository
    {
        Task<StaffViewModel> GetStaffAsync(int page = 1, int pageSize = 10);
    }
}
