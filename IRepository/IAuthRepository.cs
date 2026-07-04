using Microsoft.AspNetCore.Identity;
using Midix.ViewModel;

namespace Midix.IRepository
{
    public interface IAuthRepository
    {
     Task<IdentityResult> RegisterAsync(Registerviewmodel model);
     Task<SignInResult> LoginAsync(LoginViewModel model);
     Task LogoutAsync();

     Task<(bool Success, string? Token)> GeneratePasswordResetTokenAsync(string email);
     Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);

    }
}

