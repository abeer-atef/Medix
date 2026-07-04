using Microsoft.AspNetCore.Identity;
using Midix.Enums;

namespace Midix.Models
{
    public class ApplicationUser : IdentityUser
    {
        // IdentityUser already provides: Id, Email, PhoneNumber, UserName, PasswordHash

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public UserGender Gender { get; set; } = UserGender.Male;
        public UserRole Role { get; set; } = UserRole.Patient;
        public bool IsActive { get; set; } = true;
        public string? Picture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
