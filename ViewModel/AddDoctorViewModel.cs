using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Midix.ViewModel
{
    public class AddDoctorViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);

        [Required(ErrorMessage = "Please select a specialization.")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Consultation fee is required.")]
        [Range(0, 99999, ErrorMessage = "Fee must be a positive number.")]
        [Display(Name = "Consultation Fee ($)")]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = "Follow-up fee is required.")]
        [Range(0, 99999, ErrorMessage = "Fee must be a positive number.")]
        [Display(Name = "Follow-up Fee ($)")]
        public decimal FollowUpFee { get; set; }

        [Display(Name = "Bio")]
        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;

        [Display(Name = "Clinic Address")]
        [MaxLength(500)]
        public string ClinicAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<SelectListItem> Specializations { get; set; } = new();
    }

}