using Midix.ViewModel;

namespace Midix.IRepository.IPatient
{
    public interface IPatientPrescriptionsRepository
    {
        Task<PatientPrescriptionsViewModel> GetPrescriptionsAsync(
            string patientId, int page = 1, int pageSize = 10);
    }
}
