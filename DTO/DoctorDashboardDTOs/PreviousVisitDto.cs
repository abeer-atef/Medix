namespace Midix.DTO.DoctorDashboardDTOs
{
    public class PreviousVisitDto
    {
        public DateTime Date { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public int RxCount { get; set; }
    }

}
