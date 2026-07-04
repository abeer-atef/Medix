using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Helpers;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;
using Midix.Enums;

namespace Midix.Repository.AdminRepositories
{
    public class AdminUsersRepository : IAdminUsersRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminUsersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //  Admins tab only 
        public async Task<AllUsersViewModel> GetAdminsTabAsync(int page = 1, int pageSize = 10)
        {
            var adminsQuery = _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .OrderBy(u => u.LastName);

            var totalAdmins = await adminsQuery.CountAsync();

            var admins = await adminsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminRowDto
                {
                    UserId = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Initials = UserHelper.BuildInitials(u.FirstName, u.LastName),
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            return new AllUsersViewModel
            {
                Admins = admins,
                AdminsPagination = new PaginationInfo { CurrentPage = page, PageSize = pageSize, TotalItems = totalAdmins }
            };
        }

        // Patients tab only 
        public async Task<AllUsersViewModel> GetPatientsTabAsync(int page = 1, int pageSize = 10)
        {
            var patientsQuery = _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .OrderBy(p => p.User.LastName);

            var totalPatients = await patientsQuery.CountAsync();

            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AdminPatientRowDto
                {
                    UserId = p.UserId,
                    FullName = p.User.FirstName + " " + p.User.LastName,
                    Initials = UserHelper.BuildInitials(p.User.FirstName, p.User.LastName),
                    Email = p.User.Email ?? string.Empty,
                    PhoneNumber = p.User.PhoneNumber ?? string.Empty,
                    BloodType = p.BloodType,
                    TotalAppointments = p.Appointments.Count,
                    IsActive = p.User.IsActive,
                    CreatedAt = p.User.CreatedAt,
                    Picture = p.User.Picture
                }).ToListAsync();

            return new AllUsersViewModel
            {
                Patients = patients,
                PatientsPagination = new PaginationInfo { CurrentPage = page, PageSize = pageSize, TotalItems = totalPatients }
            };
        }

        // ── Doctors tab only ────────────────────────────────────────────────────
        public async Task<AllUsersViewModel> GetDoctorsTabAsync(int page = 1, int pageSize = 10)
        {
            var doctorsQuery = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .Include(d => d.Appointments)
                .OrderBy(d => d.User.LastName)
                .Include(d => d.Ratings);

            var totalDoctors = await doctorsQuery.CountAsync();

            var doctors = await doctorsQuery
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
                    AverageRate = d.Ratings.Any()
    ? d.Ratings.Average(r => r.Rate)
    : 0,

                    TotalReviews = d.Ratings.Count(),
                    Picture = d.User.Picture
                }).ToListAsync();

            return new AllUsersViewModel
            {
                Doctors = doctors,
                DoctorsPagination = new PaginationInfo { CurrentPage = page, PageSize = pageSize, TotalItems = totalDoctors }
            };
        }
        public async Task<DoctorReviewsViewModel> GetDoctorReviewsAsync(string doctorId)
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
                DoctorName = "Dr. " + doctor.User.FirstName + " " + doctor.User.LastName,

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
                    })
                    .ToList()
            };
        }
        public async Task DeleteReviewAsync(int ratingId)
        {
            var review = await _context.Ratings.FindAsync(ratingId);

            if (review != null)
            {
                _context.Ratings.Remove(review);

                await _context.SaveChangesAsync();
            }
        }
    }


}