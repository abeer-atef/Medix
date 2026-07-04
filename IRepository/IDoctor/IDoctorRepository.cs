using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.IRepository.IDoctor
{
    public interface IDoctorRepository
    {
        Task<List<SelectListItem>> GetSpecializationsAsync();
        Task<IdentityResult> AddDoctorAsync(AddDoctorViewModel model);
        Task<bool> EmailExistsAsync(string email);
        Task<DoctorsListViewModel> GetAllDoctorsAsync(
            string? search = null,
            int? specializationId = null,
            int page = 1,
            int pageSize =8 ); Task<EditDoctorProfileViewModel?> GetDoctorForEditAsync(string userId);

        Task<bool> UpdateDoctorAsync(string userId, EditDoctorProfileViewModel model);
        Task<DoctorReviewsViewModel> GetMyReviewsAsync(string doctorId);

        //// ── Dashboard sections (split) ─────────────────────────────
        //Task<DoctorOverviewViewModel> GetOverviewAsync(string doctorUserId);//done
        //Task<DoctorScheduleViewModel> GetScheduleAsync(string doctorUserId);//done
        //Task<DoctorPatientsViewModel> GetPatientsAsync(string doctorUserId, int page = 1, int pageSize = 10);//done
        //Task<DoctorAvailabilityViewModel> GetAvailabilityAsync(string doctorUserId);//done
        //Task<EditAvailabilityDayViewModel> GetAvailabilityDayAsync(string doctorUserId, DateTime date);//done
        //Task<bool> SaveAvailabilityDayAsync(string doctorUserId, string date, List<WorkingHoursDto> slots);//done

        //// ── Appointment actions ────────────────────────────────────
        //Task<bool> StartAppointmentAsync(string doctorUserId, int appointmentId);//done
        //Task<bool> CompleteAppointmentAsync(string doctorUserId, int appointmentId);//done
        //Task<bool> CancelAppointmentAsync(string doctorUserId, int appointmentId);//done

        //// ── Prescription ───────────────────────────────────────────
        //Task<WritePrescriptionViewModel?> GetPrescriptionContextAsync(string doctorUserId, int appointmentId);//done
        //Task<bool> SavePrescriptionAsync(string doctorUserId, SavePrescriptionRequest request);//done
    }
}