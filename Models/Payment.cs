using Midix.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
        public string? StripeSessionId { get; set; }

        // FK to Appointment 
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}