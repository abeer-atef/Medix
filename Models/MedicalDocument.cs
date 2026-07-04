using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class MedicalDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // FK to MedicalRecord
        [ForeignKey("MedicalRecord")]
        public int MedicalRecordId { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;
    }
}
