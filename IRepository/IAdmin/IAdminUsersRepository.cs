using Midix.ViewModel;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminUsersRepository
    {
        Task<AllUsersViewModel> GetAdminsTabAsync(int page = 1, int pageSize = 10);
        Task<AllUsersViewModel> GetPatientsTabAsync(int page = 1, int pageSize = 10);
        Task<AllUsersViewModel> GetDoctorsTabAsync(int page = 1, int pageSize = 10);
        Task<DoctorReviewsViewModel>GetDoctorReviewsAsync(string doctorId);

        Task DeleteReviewAsync(int ratingId);
    }

}
