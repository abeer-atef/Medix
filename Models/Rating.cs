using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rate { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK to Patient
        [ForeignKey("Patient")]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        // FK to Doctor
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; } = string.Empty;
        public Doctor Doctor { get; set; } = null!;

        // FK to Appointment (لضمان إن التقييم مرتبط بحجز Done)
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}