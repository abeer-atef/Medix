using Midix.DTO.DoctorDashboardDTOs;
using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorAvailabilityRepository
    {
        Task<DoctorAvailabilityViewModel> GetAvailabilityAsync(string doctorUserId);
        Task<EditAvailabilityDayViewModel> GetAvailabilityDayAsync(string doctorUserId, DateTime date);
        Task<bool> SaveAvailabilityDayAsync(string doctorUserId, string date, List<WorkingHoursDto> slots);
    }
}
