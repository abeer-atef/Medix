using Microsoft.AspNetCore.Identity;
using Midix.Models;

namespace Midix.Helpers
{
    public static class UserHelper
    {
        public static string BuildInitials(string firstName, string lastName)
        {
            var f = firstName?.Length > 0 ? firstName[0].ToString() : "";
            var l = lastName?.Length > 0 ? lastName[0].ToString() : "";
            return (f + l).ToUpper().Length > 0 ? (f + l).ToUpper() : "??";
        }

        public static int CalcAge(DateTime dateOfBirth)
        {
            if (dateOfBirth == DateTime.MinValue) return 0;
            return (int)((DateTime.UtcNow - dateOfBirth).TotalDays / 365.25);
        }

        public static async Task<bool> EmailExistsAsync(UserManager<ApplicationUser> userManager, string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var user = await userManager.FindByEmailAsync(email.Trim().ToLower());
            return user != null;
        }
    }
}
