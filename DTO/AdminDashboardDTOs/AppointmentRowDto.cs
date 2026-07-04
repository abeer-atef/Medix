using Midix.Enums;

namespace Midix.DTO.AdminDashboardDTOs
{
    public class AppointmentRowDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AppointmentState State { get; set; }
        public decimal? PaymentAmount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }

}
