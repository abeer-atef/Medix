using Midix.DTO.AdminDashboardDTOs;
using Midix.Models;
using Midix.Enums;

namespace Midix.ViewModel
{
    // ── Main dashboard overview (stats + charts only) ────────────────────────
    public class AdminDashboardViewModel
    {
        // Overview Stats
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Revenue Chart (last 6 months)
        public List<MonthlyRevenueDto> RevenueChart { get; set; } = new();

        // Appointments by Specialization
        public List<SpecializationAppointmentDto> AppointmentsBySpec { get; set; } = new();
    }

    // ── Staff (Doctors) with pagination ─────────────────────────────────────
    public class StaffViewModel
    {
        public List<DoctorRowDto> Doctors { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    // ── Appointments list ────────────────────────────────────────────────────
    public class AppointmentsViewModel
    {
        public List<AppointmentRowDto> Appointments { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    // ── Financial summary + transactions ────────────────────────────────────
    public class FinancialViewModel
    {
        public decimal OnlinePayments { get; set; }
        public decimal CashPayments { get; set; }
        public decimal PendingPayments { get; set; }
        public List<TransactionRowDto> Transactions { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

 
}
