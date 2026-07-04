using Midix.ViewModel;

namespace Midix.IRepository.IPatient
{
    public interface IPatientOverviewRepository
    {
        Task<PatientOverviewViewModel> GetOverviewAsync(string patientId);
    }
}
