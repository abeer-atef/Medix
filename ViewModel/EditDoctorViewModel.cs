using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Midix.ViewModel
{
    public class EditDoctorViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

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

        public bool IsActive { get; set; }

        public List<SelectListItem> Specializations { get; set; } = new();
    }
}