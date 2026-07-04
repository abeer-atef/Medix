using Midix.DTO.AdminDashboardDTOs;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.Models;
using Midix.Enums;

namespace Midix.ViewModel
{
    // ── Shared doctor header info (reused across all sections) ───────────────
    public class DoctorHeaderInfo
    {
        public string DoctorName   { get; set; } = string.Empty;
        public string Initials     { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Picture { get; set; }
    }

    // ── Overview: stats only (fast) ──────────────────────────────────────────
    public class DoctorOverviewViewModel : DoctorHeaderInfo
    {
        public int TodayTotal     { get; set; }
        public int TodayCompleted { get; set; }
        public int TodayRemaining { get; set; }
        public double AvgVisitMinutes { get; set; } = 30;
    }

    // ── Today's Schedule (queue) ─────────────────────────────────────────────
    public class DoctorScheduleViewModel : DoctorHeaderInfo
    {
        public int TodayTotal     { get; set; }
        public int TodayCompleted { get; set; }
        public int TodayRemaining { get; set; }
        public List<AppointmentQueueDto> TodayQueue { get; set; } = new();
    }

    // ── Patient list (paginated) ─────────────────────────────────────────────
    public class DoctorPatientsViewModel : DoctorHeaderInfo
    {
        public List<DoctorPatientRowDto> Patients { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    // ── Availability manager: list of next 14 days with slot counts ──────────
    public class DoctorAvailabilityViewModel : DoctorHeaderInfo
    {
        public List<AvailabilityDayDto> Days { get; set; } = new();
    }

    public class AvailabilityDayDto
    {
        public string   DateKey   { get; set; } = string.Empty; // yyyy-MM-dd
        public DateTime Date      { get; set; }
        public int      SlotCount { get; set; }
    }

    // ── Edit a single day's working hours (own page) ──────────────────────────
    public class EditAvailabilityDayViewModel : DoctorHeaderInfo
    {
        public string   Date         { get; set; } = string.Empty; // yyyy-MM-dd
        public DateTime DisplayDate  { get; set; }
        public List<string> StartTimes { get; set; } = new();
        public List<string> EndTimes   { get; set; } = new();
    }

    // ── Write Prescription (loaded when doctor starts an appointment) ─────────
    public class WritePrescriptionViewModel : DoctorHeaderInfo
    {
        public int    AppointmentId  { get; set; }
        public string PatientName    { get; set; } = string.Empty;
        public string PatientInitials { get; set; } = string.Empty;
        public int    PatientAge     { get; set; }
        public string BloodType      { get; set; } = string.Empty;
        public string ExistingDiagnosis { get; set; } = string.Empty;

        // Previous visits for this patient (with this doctor)
        public List<PreviousVisitDto> PreviousVisits { get; set; } = new();
    }

    





}

