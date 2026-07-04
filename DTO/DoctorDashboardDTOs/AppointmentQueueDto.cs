using Midix.Enums;

namespace Midix.DTO.DoctorDashboardDTOs
{
    public class AppointmentQueueDto
    {
        public int Id { get; set; }
        public int QueueNumber { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string TimeLabel { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public AppointmentState State { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

}
