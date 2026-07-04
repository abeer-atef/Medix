using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK to Appointment 
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;



        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<MedicalDocument> MedicalDocuments { get; set; } = new List<MedicalDocument>();
    }
}
