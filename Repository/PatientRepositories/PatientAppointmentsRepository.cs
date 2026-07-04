using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IPatient;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;
using Midix.Enums;

namespace Midix.Repository.PatientRepositories
{
    public class PatientAppointmentsRepository : PatientRepositoryBase, IPatientAppointmentsRepository
    {
        public PatientAppointmentsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PatientAppointmentsViewModel> GetAppointmentsAsync(
            string patientId, int upcomingPage = 1, int pastPage = 1, int pageSize = 10)
        {
            var header = await LoadHeaderAsync(patientId);
            var now = DateTime.UtcNow;

            var baseQuery = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Payment)
                .Where(a => a.PatientId == patientId);

            var upcomingQuery = baseQuery
                .Where(a => a.Date >= now && a.State != AppointmentState.Cancelled)
                .OrderBy(a => a.Date);
            var pastQuery = baseQuery
                .Where(a => a.Date < now || a.State == AppointmentState.Done || a.State == AppointmentState.Cancelled)
                .OrderByDescending(a => a.Date);

            var totalUpcoming = await upcomingQuery.CountAsync();
            var totalPast = await pastQuery.CountAsync();

            var upcomingList = await upcomingQuery
                .Skip((upcomingPage - 1) * pageSize).Take(pageSize).ToListAsync();
            var pastList = await pastQuery
                .Skip((pastPage - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new PatientAppointmentsViewModel
            {
                Upcoming = upcomingList.Select(MapAppointmentRow).ToList(),
                Past = pastList.Select(MapAppointmentRow).ToList(),
                UpcomingPagination = new PaginationInfo { CurrentPage = upcomingPage, PageSize = pageSize, TotalItems = totalUpcoming },
                PastPagination = new PaginationInfo { CurrentPage = pastPage, PageSize = pageSize, TotalItems = totalPast }
            };
            CopyHeader(header, vm);
            return vm;
        }
    }
}