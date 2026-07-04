namespace Midix.ViewModel
{
    public class SavePrescriptionFormModel
    {
        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<string> MedicineNames { get; set; } = new();
        public List<string> MedicineDosages { get; set; } = new();
        public List<string> MedicineFrequencies { get; set; } = new();
        public List<string> MedicineDurations { get; set; } = new();
    }

}
