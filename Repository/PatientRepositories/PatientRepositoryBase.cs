using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.DTO.PatientDashboardDTOs;
using Midix.Enums;
using Midix.Helpers;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.PatientRepositories
{
    public abstract class PatientRepositoryBase
    {
        protected readonly ApplicationDbContext _context;

        protected PatientRepositoryBase(ApplicationDbContext context)
        {
            _context = context;
        }

        protected async Task<PatientHeaderInfo> LoadHeaderAsync(string patientId)
        {
            var user = await _context.Users.FindAsync(patientId);
            if (user == null) return new PatientHeaderInfo();

            return new PatientHeaderInfo
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Initials = UserHelper.BuildInitials(user.FirstName, user.LastName),
                PictureUrl = user.Picture
            };
        }

        protected static void CopyHeader(PatientHeaderInfo from, PatientHeaderInfo to)
        {
            to.FirstName = from.FirstName;
            to.LastName = from.LastName;
            to.Email = from.Email;
            to.Initials = from.Initials;
            to.PictureUrl = from.PictureUrl;
        }

        protected static AppointmentRowPatientDto MapAppointmentRow(Appointment a)
        {
            var fn = a.Doctor?.User?.FirstName ?? "";
            var ln = a.Doctor?.User?.LastName ?? "";
            return new AppointmentRowPatientDto
            {
                Id = a.Id,
                DoctorName = $"Dr. {fn} {ln}".Trim(),
                DoctorInitials = UserHelper.BuildInitials(fn, ln),
                Specialization = a.Doctor?.Specialization?.Name ?? "",
                ClinicAddress = a.Doctor?.ClinicAddress ?? "",
                Date = a.Date,
                State = a.State,
                PaymentStatus = a.Payment?.Status ?? PaymentStatus.Pending,
                Fee = a.Payment?.Amount ?? 0
            };
        }

        protected static PrescriptionSummaryDto MapPrescription(Prescription p)
        {
            var doc = p.MedicalRecord?.Appointment?.Doctor;
            var fn = doc?.User?.FirstName ?? "";
            var ln = doc?.User?.LastName ?? "";
            return new PrescriptionSummaryDto
            {
                Id = p.Id,
                DoctorName = doc != null ? $"Dr. {fn} {ln}" : "Unknown Doctor",
                DoctorSpecialization = doc?.Specialization?.Name ?? "",
                DoctorInitials = UserHelper.BuildInitials(fn, ln),
                CreatedAt = p.CreatedAt,
                Diagnosis = p.MedicalRecord?.Diagnosis ?? "",
                Notes = p.Notes,
                MedicineCount = p.Medicines?.Count ?? 0,
                Medicines = (p.Medicines ?? new List<Medicine>()).Select(m => new MedicineSummaryDto
                {
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    Duration = m.Duration
                }).ToList()
            };
        }
    }
}