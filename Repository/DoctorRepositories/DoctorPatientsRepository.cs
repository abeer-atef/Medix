using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.Helpers;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.DTO.AdminDashboardDTOs;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.ViewModel;
using Midix.Enums;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorPatientsRepository : DoctorRepositoryBase, IDoctorPatientsRepository
    {

        public DoctorPatientsRepository(ApplicationDbContext context): base(context) { }

        public async Task<DoctorPatientsViewModel> GetPatientsAsync(string doctorUserId, int page = 1, int pageSize = 10)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);

            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.MedicalRecord).ThenInclude(mr => mr.Prescriptions)
                .Where(a => a.DoctorId == doctorUserId && a.State == AppointmentState.Done)
                .OrderByDescending(a => a.Date);

            var allDone = await query.ToListAsync();

            var grouped = allDone
                .GroupBy(a => a.PatientId)
                .ToList();

            var totalPatients = grouped.Count;

            var patients = grouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g =>
                {
                    var last = g.First();
                    var u = last.Patient?.User;
                    var fn = u?.FirstName ?? "";
                    var ln = u?.LastName ?? "";
                    var dob = u?.DateOfBirth ?? DateTime.MinValue;
                    var rxCount = g.Sum(a => a.MedicalRecord?.Prescriptions?.Count ?? 0);
                    return new DoctorPatientRowDto
                    {
                        PatientId = g.Key,
                        FullName = $"{fn} {ln}".Trim(),
                        Initials = UserHelper.BuildInitials(fn, ln),
                        Age = UserHelper.CalcAge(dob),
                        LastVisit = last.Date.ToString("MMM d, yyyy"),
                        LastDiagnosis = last.MedicalRecord?.Diagnosis ?? "—",
                        ActivePrescriptions = rxCount,
                        LastAppointmentId = last.Id
                    };
                }).ToList();

            return new DoctorPatientsViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                Patients = patients,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalPatients
                }
            };
        }

    }
}
