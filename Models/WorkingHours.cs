using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class WorkingHours
    {
        public int Id { get; set; }

        public DateTime Day { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // FK to Doctor (string UserId)
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; } = string.Empty;
        public Doctor Doctor { get; set; } = null!;
    }
}
