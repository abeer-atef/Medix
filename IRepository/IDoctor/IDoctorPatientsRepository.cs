using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorPatientsRepository
    {
        Task<DoctorPatientsViewModel> GetPatientsAsync(string doctorUserId, int page = 1, int pageSize = 10);
    }

}
