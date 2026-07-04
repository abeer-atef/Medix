using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Midix.ViewModel
{
    // ── 1. Upload Request ────────────────────────────────────────────────────────

    public class UploadPrescriptionRequest
    {
        [Required(ErrorMessage = "A prescription image is required.")]
        public IFormFile? PrescriptionImage { get; set; }
    }

    // ── 2. Single Medication ─────────────────────────────────────────────────────
    public class MedicationDto
    {
        /// <summary>Generic or brand name of the medication.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Form of the medication (e.g. tablet, syrup, injection).</summary>
        public string DosageForm { get; set; } = string.Empty;

        /// <summary>Strength per dose (e.g. "500 mg").</summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>How often to take it (e.g. "twice daily").</summary>
        public string Frequency { get; set; } = string.Empty;

        /// <summary>Total treatment period (e.g. "7 days").</summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>Patient-facing instructions in Arabic.</summary>
        public string InstructionsAr { get; set; } = string.Empty;
    }


    public class StructuredPrescriptionAnalysis
    {
        public string DoctorSpecialty { get; set; } = string.Empty;

        public string SuspectedDiagnosis { get; set; } = string.Empty;

        public string GeneralPrecautions { get; set; } = string.Empty;

        public string RawMedicalSummaryEn { get; set; } = string.Empty;

        public List<MedicationDto> Medications { get; set; } = new List<MedicationDto>();
    }
}
