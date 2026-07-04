using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.Helpers;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.AdminRepositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IdentityResult> AddAdminAsync(AddAdminViewModel model)
        {
            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return result;

            await _userManager.AddToRoleAsync(user, nameof(UserRole.Admin));
            return result;
        }

    }
}
