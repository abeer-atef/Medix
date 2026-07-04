using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Enums;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.AdminRepositories
{
    public class AdminDoctorManagementRepository : IAdminDoctorManagementRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminDoctorManagementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EditDoctorViewModel?> GetDoctorForEditAsync(string userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null) return null;

            var specializations = await _context.Specializations
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == doctor.SpecializationId
                }).ToListAsync();

            return new EditDoctorViewModel
            {
                UserId = doctor.UserId,
                FirstName = doctor.User.FirstName,
                LastName = doctor.User.LastName,
                PhoneNumber = doctor.User.PhoneNumber ?? string.Empty,
                SpecializationId = doctor.SpecializationId,
                ConsultationFee = doctor.ConsultationFee,
                FollowUpFee = doctor.FollowUpFee,
                Bio = doctor.Bio ?? string.Empty,
                ClinicAddress = doctor.ClinicAddress ?? string.Empty,
                IsActive = doctor.User.IsActive,
                Specializations = specializations
            };
        }

        public async Task<bool> UpdateDoctorAsync(EditDoctorViewModel model)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == model.UserId);

            if (doctor == null) return false;

            doctor.User.FirstName = model.FirstName;
            doctor.User.LastName = model.LastName;
            doctor.User.PhoneNumber = model.PhoneNumber;
            doctor.User.IsActive = model.IsActive;
            doctor.SpecializationId = model.SpecializationId;
            doctor.ConsultationFee = model.ConsultationFee;
            doctor.FollowUpFee = model.FollowUpFee;
            doctor.Bio = model.Bio;
            doctor.ClinicAddress = model.ClinicAddress;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleDoctorActiveAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<int> GetUpcomingAppointmentsCountAsync(string doctorUserId)
        {
            var now = DateTime.UtcNow;
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorUserId
                         && a.Date > now
                         && a.State != AppointmentState.Cancelled
                         && a.State != AppointmentState.Done)
                .CountAsync();
        }

        public async Task CancelDoctorAppointmentsAsync(string doctorUserId)
        {
            var now = DateTime.UtcNow;
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorUserId
                         && a.Date > now
                         && a.State != AppointmentState.Cancelled
                         && a.State != AppointmentState.Done)
                .ToListAsync();

            foreach (var appt in appointments)
                appt.State = AppointmentState.Cancelled;

            await _context.SaveChangesAsync();
        }
    }
}