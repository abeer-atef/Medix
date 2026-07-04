using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Midix.Models
{
    public class Prescription
    {
        // ── Primary Key ──────────────────────────────────────────────────────────
        [Key]
        public int Id { get; set; }

        // ── Clinical fields ──────────────────────────────────────────────────────
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── AI Prescription-scan fields ──────────────────────────────────────────
        /// <summary>
        /// URL of the uploaded prescription image (e.g. stored in cloud / wwwroot).
        /// </summary>
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Raw JSON returned by the AI analysis service. Nullable until analysis completes.
        /// </summary>
        public string? AiAnalysisResult { get; set; }

        /// <summary>
        /// UTC timestamp recorded when the prescription image was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // ── Relationships ────────────────────────────────────────────────────────

        /// <summary>
        /// FK to the patient who owns this prescription.
        /// </summary>
        [Required]
        [ForeignKey("Patient")]
        public string PatientId { get; set; } = string.Empty;
        public Patient Patient { get; set; } = null!;


        [ForeignKey("MedicalRecord")]
        public int? MedicalRecordId { get; set; }          // nullable — AI-only prescriptions have no MedicalRecord
        public MedicalRecord? MedicalRecord { get; set; }

        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
