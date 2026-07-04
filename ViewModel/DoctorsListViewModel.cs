using Microsoft.AspNetCore.Mvc.Rendering;
using Midix.DTO.AdminDashboardDTOs;

namespace Midix.ViewModel
{
    // One row/card on the public Doctors page
    public class DoctorCardViewModel
    {
        public string DoctorUserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    // The whole page: list of doctors + filter dropdown + current search state
    public class DoctorsListViewModel
    {
        public List<DoctorCardViewModel> Doctors { get; set; } = new();
        public List<SelectListItem> Specializations { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? SelectedSpecializationId { get; set; }
        public PaginationInfo Pagination { get; set; } = new();

    }

}