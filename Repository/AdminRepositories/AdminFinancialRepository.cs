using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;
using Midix.Enums;

namespace Midix.Repository.AdminRepositories
{
    public class AdminFinancialRepository: IAdminFinancialRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminFinancialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialViewModel> GetFinancialAsync(int page = 1, int pageSize = 10)
        {
            var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            // Summary numbers (no pagination needed)
            var payments = await _context.Payments
                .Where(p => p.Appointment.Date >= firstOfMonth || p.Status == PaymentStatus.Pending)
                .Select(p => new { p.Status, p.Method, p.Amount })
                .ToListAsync();

            // Paginated transactions
            var transQuery = _context.Payments
                .Include(p => p.Appointment.Patient.User)
                .Include(p => p.Appointment.Doctor.User)
                .OrderByDescending(p => p.Appointment.Date);

            var totalItems = await transQuery.CountAsync();

            var transactions = await transQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new TransactionRowDto
                {
                    Id = p.Id,
                    PatientName = p.Appointment.Patient.User.FirstName + " " + p.Appointment.Patient.User.LastName,
                    Amount = p.Amount,
                    Method = p.Method,
                    Date = p.Appointment.Date,
                    Status = p.Status
                })
                .ToListAsync();

            return new FinancialViewModel
            {
                OnlinePayments = payments.Where(p => p.Status == PaymentStatus.Paid && p.Method != PaymentMethod.Cash).Sum(p => p.Amount),
                CashPayments = payments.Where(p => p.Status == PaymentStatus.Paid && p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
                PendingPayments = payments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                Transactions = transactions,
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
