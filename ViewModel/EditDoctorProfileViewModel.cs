using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace Midix.ViewModel
{
    public class EditDoctorProfileViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";
        [Required]
        public string LastName { get; set; } = "";
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Bio { get; set; } = "";
        public string ClinicAddress { get; set; } = "";

        [Required(ErrorMessage = "Consultation fee is required.")]
        [Range(0, 99999, ErrorMessage = "Fee must be a positive number.")]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = "Follow-up fee is required.")]
        [Range(0, 99999, ErrorMessage = "Fee must be a positive number.")]
        public decimal FollowUpFee { get; set; }

        public int SpecializationId { get; set; }
        public IFormFile? PictureFile { get; set; }
        public string? CurrentPicture { get; set; }
        public string? Picture { get; set; }
    }
}