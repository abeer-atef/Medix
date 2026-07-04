using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.DTO.AdminDashboardDTOs;
using Midix.Enums;
using Midix.IRepository.IAdmin;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.AdminRepositories
{
    public class AdminOverviewRepository: IAdminOverviewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOverviewRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardViewModel> GetOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var firstOfMonth = new DateTime(now.Year, now.Month, 1);
            var today = now.Date;

            var totalDoctors = await _context.Doctors.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var todayAppts = await _context.Appointments.CountAsync(a => a.Date.Date == today);
            var monthlyRevenue = await _GetMonthlyRevenueAsync(firstOfMonth);
            var revenueChart = await _GetRevenueChartDataAsync(firstOfMonth);
            var specStats = await _GetSpecializationStatsAsync();

            return new AdminDashboardViewModel
            {
                TotalDoctors = totalDoctors,
                TotalPatients = totalPatients,
                TodayAppointments = todayAppts,
                MonthlyRevenue = monthlyRevenue,
                RevenueChart = revenueChart,
                AppointmentsBySpec = specStats
            };
        }

        private async Task<decimal> _GetMonthlyRevenueAsync(DateTime firstOfMonth) =>
    await _context.Payments
        .Where(p => p.Status == PaymentStatus.Paid && p.Appointment.Date >= firstOfMonth)
        .SumAsync(p => (decimal?)p.Amount) ?? 0;

        private async Task<List<MonthlyRevenueDto>> _GetRevenueChartDataAsync(DateTime firstOfMonth)
        {
            var sixMonthsAgo = firstOfMonth.AddMonths(-5);
            var dbData = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.Appointment.Date >= sixMonthsAgo)
                .GroupBy(p => new { p.Appointment.Date.Year, p.Appointment.Date.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => (decimal?)x.Amount) ?? 0 })
                .ToListAsync();

            var result = new List<MonthlyRevenueDto>();
            for (int i = 5; i >= 0; i--)
            {
                var target = firstOfMonth.AddMonths(-i);
                var amount = dbData.FirstOrDefault(d => d.Year == target.Year && d.Month == target.Month)?.Total ?? 0;
                result.Add(new MonthlyRevenueDto { Month = target.ToString("MMM"), Amount = amount, IsCurrentMonth = i == 0 });
            }
            var max = result.Max(r => r.Amount);
            result.ForEach(r => r.HeightPercent = max > 0 ? Math.Max(5, (int)(r.Amount / max * 96)) : 5);
            return result;
        }

        private async Task<List<SpecializationAppointmentDto>> _GetSpecializationStatsAsync()
        {
            var specColors = new[] { "var(--blue)", "var(--teal)", "var(--green)", "var(--orange)", "var(--purple)" };
            var stats = await _context.Appointments
                .GroupBy(a => a.Doctor.Specialization.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            int max = stats.Any() ? stats[0].Count : 1;
            return stats.Select((s, i) => new SpecializationAppointmentDto
            {
                Name = s.Name,
                Count = s.Count,
                MaxCount = max,
                Color = specColors[i % specColors.Length]
            }).ToList();
        }

    }
}
