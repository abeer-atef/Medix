using Midix.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Midix.IRepository.IAdmin
{
    public interface IAdminRepository
    {
 
        Task<IdentityResult> AddAdminAsync(AddAdminViewModel model);


    }
}
