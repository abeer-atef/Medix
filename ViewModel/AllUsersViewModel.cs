using Midix.DTO.AdminDashboardDTOs;

namespace Midix.ViewModel
{
    public class AllUsersViewModel
    {
        public List<AdminRowDto> Admins { get; set; } = new();
        public List<AdminPatientRowDto> Patients { get; set; } = new();

        // Pagination for doctors tab (in AllUsers page)
        public PaginationInfo DoctorsPagination { get; set; } = new();
        public List<DoctorRowDto> Doctors { get; set; } = new();

        // Pagination for patients tab
        public PaginationInfo PatientsPagination { get; set; } = new();

        // Pagination for admins tab
        public PaginationInfo AdminsPagination { get; set; } = new();
    }


}