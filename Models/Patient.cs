using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Patient
    {
        // PK = FK to ApplicationUser (UserId is the primary key)
        [Key]
        public string UserId { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
