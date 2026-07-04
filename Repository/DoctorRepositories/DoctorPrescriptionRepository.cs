using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.Enums;
using Midix.Helpers;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorPrescriptionRepository : DoctorRepositoryBase, IDoctorPrescriptionRepository
    {
        public DoctorPrescriptionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<WritePrescriptionViewModel?> GetPrescriptionContextAsync(string doctorUserId, int appointmentId)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);

            var apt = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Patient)
                .Include(a => a.MedicalRecord)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorUserId);

            if (apt == null) return null;

            var u = apt.Patient?.User;
            var fn = u?.FirstName ?? "";
            var ln = u?.LastName ?? "";
            var dob = u?.DateOfBirth ?? DateTime.MinValue;
            var age = UserHelper.CalcAge(dob);

            var prevVisits = await _context.Appointments
                .Include(a => a.MedicalRecord).ThenInclude(mr => mr.Prescriptions)
                .Where(a => a.PatientId == apt.PatientId
                          && a.DoctorId == doctorUserId
                          && a.State == AppointmentState.Done
                          && a.Id != appointmentId)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .Select(a => new PreviousVisitDto
                {
                    Date = a.Date,
                    Diagnosis = a.MedicalRecord != null ? a.MedicalRecord.Diagnosis : "—",
                    RxCount = a.MedicalRecord != null ? a.MedicalRecord.Prescriptions.Count : 0
                })
                .ToListAsync();

            var bloodType = await _context.Patients
                .Where(p => p.UserId == apt.PatientId)
                .Select(p => p.BloodType)
                .FirstOrDefaultAsync() ?? "—";

            return new WritePrescriptionViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                AppointmentId = appointmentId,
                PatientName = $"{fn} {ln}".Trim(),
                PatientInitials = UserHelper.BuildInitials(fn, ln),
                PatientAge = age,
                BloodType = bloodType,
                ExistingDiagnosis = apt.MedicalRecord?.Diagnosis ?? "",
                PreviousVisits = prevVisits
            };
        }

        public async Task<bool> SavePrescriptionAsync(string doctorUserId, SavePrescriptionRequest request)
        {
            try
            {
                var apt = await _context.Appointments
                    .Include(a => a.MedicalRecord).ThenInclude(mr => mr.Prescriptions)
                    .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.DoctorId == doctorUserId);

                if (apt == null) return false;

                if (apt.MedicalRecord == null)
                {
                    apt.MedicalRecord = new MedicalRecord
                    {
                        AppointmentId = apt.Id,
                        Diagnosis = request.Diagnosis,
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    apt.MedicalRecord.Diagnosis = request.Diagnosis;
                }

                var prescription = new Prescription
                {
                    PatientId = apt.PatientId,
                    MedicalRecord = apt.MedicalRecord,
                    Notes = request.Notes ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Medicines = request.Medicines.Select(m => new Medicine
                    {
                        Name = m.Name,
                        Dosage = m.Dosage,
                        Frequency = m.Frequency,
                        Duration = m.Duration
                    }).ToList()
                };

                _context.Prescriptions.Add(prescription);

                LogStateChange(apt, AppointmentState.Done);
                apt.State = AppointmentState.Done;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE-RX-DEBUG] FAILED: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"[SAVE-RX-DEBUG] Inner: {ex.InnerException.Message}");
                return false;
            }
        }
    }
}