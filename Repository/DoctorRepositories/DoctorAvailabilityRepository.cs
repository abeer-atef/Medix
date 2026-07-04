using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Repository.DoctorRepositories
{
    public class DoctorAvailabilityRepository : DoctorRepositoryBase, IDoctorAvailabilityRepository
    {

        public DoctorAvailabilityRepository(ApplicationDbContext context):base(context) { }

        public async Task<DoctorAvailabilityViewModel> GetAvailabilityAsync(string doctorUserId)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);
            var today = DateTime.UtcNow.Date;
            var rangeEnd = today.AddDays(14);

            var workingHours = await _context.WorkingHours
                .Where(w => w.DoctorId == doctorUserId && w.Day >= today && w.Day < rangeEnd)
                .ToListAsync();

            var countsByDate = workingHours
                .GroupBy(w => w.Day.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var days = new List<AvailabilityDayDto>();
            for (int i = 0; i < 14; i++)
            {
                var date = today.AddDays(i);
                days.Add(new AvailabilityDayDto
                {
                    DateKey = date.ToString("yyyy-MM-dd"),
                    Date = date,
                    SlotCount = countsByDate.TryGetValue(date, out var c) ? c : 0
                });
            }

            return new DoctorAvailabilityViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                Days = days
            };
        }

        public async Task<EditAvailabilityDayViewModel> GetAvailabilityDayAsync(string doctorUserId, DateTime date)
        {
            var (_, name, initials, spec) = await LoadHeaderAsync(doctorUserId);
            var day = date.Date;

            var slots = await _context.WorkingHours
                .Where(w => w.DoctorId == doctorUserId && w.Day.Date == day)
                .OrderBy(w => w.StartTime)
                .ToListAsync();

            var vm = new EditAvailabilityDayViewModel
            {
                DoctorName = name,
                Initials = initials,
                Specialization = spec,
                Date = day.ToString("yyyy-MM-dd"),
                DisplayDate = day,
                StartTimes = slots.Select(s => s.StartTime.ToString("HH:mm")).ToList(),
                EndTimes = slots.Select(s => s.EndTime.ToString("HH:mm")).ToList()
            };

            // Always have at least one empty row to edit in the form
            if (!vm.StartTimes.Any())
            {
                vm.StartTimes.Add("09:00");
                vm.EndTimes.Add("17:00");
            }

            return vm;
        }

        public async Task<bool> SaveAvailabilityDayAsync(string doctorUserId, string date, List<WorkingHoursDto> slots)
        {
            try
            {
                var day = DateTime.ParseExact(date, "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture).Date;

                var existing = _context.WorkingHours
                    .Where(w => w.DoctorId == doctorUserId && w.Day.Date == day);
                _context.WorkingHours.RemoveRange(existing);

                foreach (var slot in slots)
                {
                    var start = TimeOnly.Parse(slot.StartTime);
                    var end = TimeOnly.Parse(slot.EndTime);
                    if (end <= start) return false;

                    _context.WorkingHours.Add(new WorkingHours
                    {
                        DoctorId = doctorUserId,
                        Day = day,
                        StartTime = start,
                        EndTime = end
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

    }
}
