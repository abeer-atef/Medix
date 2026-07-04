using Midix.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public AppointmentState State { get; set; } = AppointmentState.Pending;
        public AppointmentType AppointmentType { get; set; } = AppointmentType.NewVisit;

        // FK to Patient (string UserId)
        [ForeignKey("Patient")]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;

        // FK to Doctor (string UserId)
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; } = string.Empty;
        public Doctor Doctor { get; set; } = null!;

        public MedicalRecord? MedicalRecord { get; set; }
        public Payment? Payment { get; set; }
        public Rating? Rating { get; set; }  
        public ICollection<AppointmentStateChange> StateChanges { get; set; } = new List<AppointmentStateChange>();
    }
}
