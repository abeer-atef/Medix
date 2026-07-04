using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IPatient;
using Midix.DTO.AdminDashboardDTOs;
using Midix.ViewModel;

namespace Midix.Repository.PatientRepositories
{
    public class PatientPrescriptionsRepository : PatientRepositoryBase, IPatientPrescriptionsRepository
    {
        public PatientPrescriptionsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PatientPrescriptionsViewModel> GetPrescriptionsAsync(string patientId, int page = 1, int pageSize = 10)
        {
            var header = await LoadHeaderAsync(patientId);

            var query = _context.Prescriptions
                .Include(p => p.MedicalRecord).ThenInclude(mr => mr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .Include(p => p.MedicalRecord).ThenInclude(mr => mr.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(p => p.Medicines)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new PatientPrescriptionsViewModel
            {
                Prescriptions = items.Select(MapPrescription).ToList(),
                Pagination = new PaginationInfo { CurrentPage = page, PageSize = pageSize, TotalItems = totalItems }
            };
            CopyHeader(header, vm);
            return vm;
        }
    }
}