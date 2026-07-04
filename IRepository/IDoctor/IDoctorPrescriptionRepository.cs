using Midix.DTO.DoctorDashboardDTOs;
using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorPrescriptionRepository
    {
        Task<WritePrescriptionViewModel?> GetPrescriptionContextAsync(string doctorUserId, int appointmentId);
        Task<bool> SavePrescriptionAsync(string doctorUserId, SavePrescriptionRequest request);
    }
}
