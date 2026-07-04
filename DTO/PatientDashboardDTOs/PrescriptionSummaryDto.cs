using Midix.ViewModel;

namespace Midix.DTO.PatientDashboardDTOs
{
    public class PrescriptionSummaryDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public string DoctorInitials { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int MedicineCount { get; set; }
        public List<MedicineSummaryDto> Medicines { get; set; } = new();
    }

}
