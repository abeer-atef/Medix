using Midix.DTO.PatientDashboardDTOs;
using Midix.ViewModel;

namespace Midix.IRepository.IPatient
{
    public interface IPatientProfileRepository
    {
        Task<PatientProfileViewModel> GetProfileAsync(string patientId);
        Task<bool> UpdateProfileAsync(string patientId, PatientProfileDto dto);
    }
}
