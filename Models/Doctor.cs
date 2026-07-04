using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Doctor
    {
        // PK = FK to ApplicationUser (UserId is the primary key)
        [Key]
        public string UserId { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public decimal FollowUpFee { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        // FK to Specialization 
        [ForeignKey("Specialization")]
        public int SpecializationId { get; set; }
        public Specialization Specialization { get; set; } = null!;

        public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
