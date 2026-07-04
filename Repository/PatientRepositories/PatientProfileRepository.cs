using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.DTO.PatientDashboardDTOs;
using Midix.Enums;
using Midix.IRepository.IPatient;
using Midix.ViewModel;

namespace Midix.Repository.PatientRepositories
{
    public class PatientProfileRepository : PatientRepositoryBase, IPatientProfileRepository
    {
        public PatientProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PatientProfileViewModel> GetProfileAsync(string patientId)
        {
            var header = await LoadHeaderAsync(patientId);
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientId);

            var vm = new PatientProfileViewModel();
            CopyHeader(header, vm);

            if (patient?.User != null)
            {
                vm.PhoneNumber = patient.User.PhoneNumber ?? "";
                vm.DateOfBirth = patient.User.DateOfBirth;
                vm.Gender = patient.User.Gender.ToString();
                vm.BloodType = patient.BloodType;
                vm.Picture = patient.User.Picture;
                vm.PictureUrl = patient.User.Picture;

            }
            return vm;
        }

        public async Task<bool> UpdateProfileAsync(string patientId, PatientProfileDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(patientId);
                if (user == null) return false;

                user.FirstName = dto.FirstName.Trim();
                user.LastName = dto.LastName.Trim();
                user.PhoneNumber = dto.PhoneNumber.Trim();
                user.DateOfBirth = dto.DateOfBirth;

                if (Enum.TryParse<UserGender>(dto.Gender, out var gender))
                    user.Gender = gender;

                var patient = await _context.Patients.FindAsync(patientId);
                if (patient != null)
                    patient.BloodType = dto.BloodType.Trim();

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}