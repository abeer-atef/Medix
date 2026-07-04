namespace Midix.DTO.AdminDashboardDTOs
{
    public class DoctorRowDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int PatientCount { get; set; }
        public bool IsActive { get; set; }
        public string? Picture { get; set; }
        public double AverageRate { get; set; }

        public int TotalReviews { get; set; }
    }

}
