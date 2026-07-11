using Midix.Models;
using Midix.Enums;
using Microsoft.AspNetCore.Http;
using Midix.DTO.AdminDashboardDTOs;
using Midix.DTO.PatientDashboardDTOs;
namespace Midix.ViewModel
{
    // Shared sidebar info (reused across all patient views) 
    public class PatientHeaderInfo
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }

    }

    // Overview (Index) 
    public class PatientOverviewViewModel : PatientHeaderInfo
    {
        public int UpcomingAppointmentsCount { get; set; }
        public int TotalPrescriptionsCount { get; set; }
        public int PastVisitsCount { get; set; }
        public int MedicationsDueCount { get; set; }

        public List<AppointmentRowPatientDto> UpcomingAppointments { get; set; } = new();
        public List<PrescriptionSummaryDto> RecentPrescriptions { get; set; } = new();
    }

    // Book Appointment 
    public class BookAppointmentViewModel : PatientHeaderInfo
    {
        public List<DoctorCardDto> AvailableDoctors { get; set; } = new();

        // Repopulated on step-2 GET
        public string? SelectedDoctorId { get; set; }
        public string? SelectedDoctorName { get; set; }
        public string? SelectedSpec { get; set; }
        public string? SelectedClinicAddress { get; set; }
        public string SelectedAppointmentType { get; set; } = "NewVisit";
        public decimal SelectedFee { get; set; }
        public decimal SelectedFollowUpFee { get; set; }
        public string? SelectedDate { get; set; }

        // Repopulated on step-3 GET
        public string? SelectedTime { get; set; }
    }

    // My Appointments 
    public class PatientAppointmentsViewModel : PatientHeaderInfo
    {
        public List<AppointmentRowPatientDto> Upcoming { get; set; } = new();
        public List<AppointmentRowPatientDto> Past { get; set; } = new();

        // Pagination (shared pageSize for both tabs)
        public PaginationInfo UpcomingPagination { get; set; } = new();
        public PaginationInfo PastPagination { get; set; } = new();
    }

    // Prescriptions 
    public class PatientPrescriptionsViewModel : PatientHeaderInfo
    {
        public List<PrescriptionSummaryDto> Prescriptions { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    // Profile 
    public class PatientProfileViewModel : PatientHeaderInfo
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;

        public string? Picture { get; set; }
    }

}