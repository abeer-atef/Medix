using Midix.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class AppointmentStateChange
    {
        public int Id { get; set; }
        public AppointmentState OldState { get; set; }
        public AppointmentState NewState { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // FK to Appointment
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}