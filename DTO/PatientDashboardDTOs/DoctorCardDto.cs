namespace Midix.DTO.PatientDashboardDTOs
{
    public class DoctorCardDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public decimal FollowUpFee { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
    }

}
