using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorOverviewRepository
    {
        Task<DoctorOverviewViewModel> GetOverviewAsync(string doctorUserId);

    }
}
