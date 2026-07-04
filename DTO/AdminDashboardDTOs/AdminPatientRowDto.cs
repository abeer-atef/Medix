namespace Midix.DTO.AdminDashboardDTOs
{
    public class AdminPatientRowDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Picture { get; set; }

    }

}
