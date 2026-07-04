namespace Midix.DTO.DoctorDashboardDTOs
{
    public class SavePrescriptionRequest
    {
        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<MedicineDto> Medicines { get; set; } = new();
    }

}
