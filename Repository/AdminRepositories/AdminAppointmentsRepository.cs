using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;

namespace Midix.Repository.AdminRepositories
{
    public class AdminAppointmentsRepository: IAdminAppointmentsRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminAppointmentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<AppointmentsViewModel> GetAppointmentsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Appointments
                .Include(a => a.Patient.User)
                .Include(a => a.Doctor.User)
                .Include(a => a.Doctor.Specialization)
                .Include(a => a.Payment)
                .OrderByDescending(a => a.Date);

            var totalItems = await query.CountAsync();

            var appointments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AppointmentRowDto
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FirstName + " " + a.Patient.User.LastName,
                    DoctorName = "Dr. " + a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                    Date = a.Date,
                    State = a.State,
                    PaymentAmount = a.Payment.Amount,
                    PaymentMethod = a.Payment.Method,
                    PaymentStatus = a.Payment.Status
                })
                .ToListAsync();

            return new AppointmentsViewModel
            {
                Appointments = appointments,
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
