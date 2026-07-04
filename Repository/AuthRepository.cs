using Microsoft.AspNetCore.Identity;
using Midix.Enums;
using Midix.IRepository;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Register 
        public async Task<IdentityResult> RegisterAsync(Registerviewmodel model)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim().ToLower(),
                UserName = model.Email.Trim().ToLower(),
                PhoneNumber = model.PhoneNumber.Trim(),
                DateOfBirth = model.DateOfBirth,
                Gender = Enum.TryParse<UserGender>(model.Gender, out var gender) ? gender : UserGender.Male,
                Role = UserRole.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,


                Patient = new Patient()
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Patient.ToString());
            }

            return result;
        }

        // Login
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());

            if (user == null)
                return SignInResult.Failed;

            if (!user.IsActive)
                return SignInResult.NotAllowed;

            return await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);
        }

        // Logout 
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Reset Password

        public async Task<(bool Success, string? Token)> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user == null) return (false, null);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return (true, token);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

    }

}