using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Helpers;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;

namespace Midix.Repository.AdminRepositories
{
    public class AdminStaffRepository : IAdminStaffRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminStaffRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<StaffViewModel> GetStaffAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.Appointments)
                .OrderBy(d => d.User.LastName);

            var totalItems = await query.CountAsync();

            var doctors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DoctorRowDto
                {
                    UserId = d.UserId,
                    FullName = "Dr. " + d.User.FirstName + " " + d.User.LastName,
                    Initials = UserHelper.BuildInitials(d.User.FirstName, d.User.LastName),
                    Specialization = d.Specialization.Name,
                    PatientCount = d.Appointments.Select(a => a.PatientId).Distinct().Count(),
                    IsActive = d.User.IsActive,
                    Picture = d.User.Picture
                })
                .ToListAsync();

            return new StaffViewModel
            {
                Doctors = doctors,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };
        }
    }
}