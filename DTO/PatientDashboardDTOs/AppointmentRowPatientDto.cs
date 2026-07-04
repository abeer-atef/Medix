using Midix.Enums;

namespace Midix.DTO.PatientDashboardDTOs
{
    public class AppointmentRowPatientDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorInitials { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AppointmentType AppointmentType { get; set; }
        public AppointmentState State { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Fee { get; set; }
    }
}