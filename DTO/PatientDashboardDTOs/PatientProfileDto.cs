namespace Midix.DTO.PatientDashboardDTOs
{
    public class PatientProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public IFormFile? Picture { get; set; }
        public IFormFile? ProfileImage { get; set; }


    }

}
