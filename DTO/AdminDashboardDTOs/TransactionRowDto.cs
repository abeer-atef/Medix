using Midix.Enums;

namespace Midix.DTO.AdminDashboardDTOs
{
    public class TransactionRowDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime Date { get; set; }
        public PaymentStatus Status { get; set; }
    }

}
