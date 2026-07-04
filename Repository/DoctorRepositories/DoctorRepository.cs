using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.DTO.AdminDashboardDTOs;
using Midix.Helpers;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<SelectListItem>> GetSpecializationsAsync() =>
            await _context.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

        public async Task<bool> EmailExistsAsync(string email) =>
            await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null;

        public async Task<DoctorsListViewModel> GetAllDoctorsAsync(
    string? search = null,
    int? specializationId = null,
    int page = 1,
    int pageSize = 8)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.Ratings)
                .Where(d => d.User.IsActive);

            if (specializationId.HasValue && specializationId.Value > 0)
            {
                query = query.Where(d => d.SpecializationId == specializationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                query = query.Where(d =>
                    d.User.FirstName.ToLower().Contains(term) ||
                    d.User.LastName.ToLower().Contains(term) ||
                    d.Specialization.Name.ToLower().Contains(term));
            }

            var totalDoctors = await query.CountAsync();

            var doctors = await query
                .OrderBy(d => d.User.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new DoctorsListViewModel
            {
                Doctors = doctors.Select(d =>
                {
                    var fn = d.User.FirstName ?? "";
                    var ln = d.User.LastName ?? "";

                    return new DoctorCardViewModel
                    {
                        DoctorUserId = d.UserId,
                        FullName = $"Dr. {fn} {ln}",
                        Initials = UserHelper.BuildInitials(fn, ln),
                        SpecializationId = d.SpecializationId,
                        SpecializationName = d.Specialization?.Name ?? "General",
                        ConsultationFee = d.ConsultationFee,
                        Bio = d.Bio,
                        ClinicAddress = d.ClinicAddress,
                        Picture = d.User.Picture,
                        AverageRating = d.Ratings.Any()
                            ? Math.Round(d.Ratings.Average(r => r.Rate), 1)
                            : 0,
                        RatingCount = d.Ratings.Count
                    };
                }).ToList(),

                Specializations = await GetSpecializationsAsync(),
                SearchTerm = search,
                SelectedSpecializationId = specializationId,

                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalDoctors
                }
            };
        }

        public async Task<IdentityResult> AddDoctorAsync(AddDoctorViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email.Trim().ToLower(),
                Email = model.Email.Trim().ToLower(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                DateOfBirth = model.DateOfBirth,
                Role = UserRole.Doctor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Doctor = new Doctor
                {
                    SpecializationId = model.SpecializationId,
                    ConsultationFee = model.ConsultationFee,
                    FollowUpFee = model.FollowUpFee,
                    Bio = model.Bio.Trim(),
                    ClinicAddress = model.ClinicAddress.Trim()
                }
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, UserRole.Doctor.ToString());

            return result;
        }

        public async Task<EditDoctorProfileViewModel?> GetDoctorForEditAsync(string userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null) return null;

            return new EditDoctorProfileViewModel
            {
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                Email = doctor.User.Email ?? "",
                PhoneNumber = doctor.User.PhoneNumber ?? "",
                Bio = doctor.Bio,
                ClinicAddress = doctor.ClinicAddress,
                ConsultationFee = doctor.ConsultationFee,
                FollowUpFee = doctor.FollowUpFee,
                SpecializationId = doctor.SpecializationId,
                CurrentPicture = doctor.User.Picture
            };
        }

        public async Task<bool> UpdateDoctorAsync(string userId, EditDoctorProfileViewModel model)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null) return false;

            doctor.User.FirstName = model.FirstName;
            doctor.User.LastName = model.LastName;
            doctor.User.Email = model.Email;
            doctor.User.UserName = model.Email;
            doctor.User.PhoneNumber = model.PhoneNumber;

            doctor.Bio = model.Bio;
            doctor.ClinicAddress = model.ClinicAddress;
            doctor.ConsultationFee = model.ConsultationFee;
            doctor.FollowUpFee = model.FollowUpFee;
            doctor.SpecializationId = model.SpecializationId;

            if (model.PictureFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PictureFile.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PictureFile.CopyToAsync(stream);
                }
                doctor.User.Picture = fileName;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<DoctorReviewsViewModel> GetMyReviewsAsync(string doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Ratings)
                    .ThenInclude(r => r.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorId);

            if (doctor == null)
                return null!;

            return new DoctorReviewsViewModel
            {
                DoctorId = doctor.UserId,

                DoctorName = "Dr. "
                    + doctor.User.FirstName
                    + " "
                    + doctor.User.LastName,

                AverageRate = doctor.Ratings.Any()
                    ? doctor.Ratings.Average(r => r.Rate)
                    : 0,

                TotalReviews = doctor.Ratings.Count,

                Reviews = doctor.Ratings
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewItemVM
                    {
                        RatingId = r.Id,
                        PatientName = r.Patient.User.FirstName + " " + r.Patient.User.LastName,
                        Rate = r.Rate,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    }).ToList()
            };
        }
    }
}