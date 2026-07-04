using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midix.DTO.DoctorDashboardDTOs;
using Midix.Enums;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;
using System.Security.Claims;

namespace Midix.Controllers
{
    [Authorize(Roles = nameof(UserRole.Doctor))]
    public class DoctorDashboardController : Controller
    {
        private readonly IDoctorOverviewRepository _overviewRepo;
        private readonly IDoctorScheduleRepository _scheduleRepo;
        private readonly IDoctorPatientsRepository _patientsRepo;
        private readonly IDoctorAvailabilityRepository _availabilityRepo;
        private readonly IDoctorPrescriptionRepository _prescriptionRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDoctorRepository _doctorRepo;
        public DoctorDashboardController(
            IDoctorOverviewRepository overviewRepo,
            IDoctorScheduleRepository scheduleRepo,
            IDoctorPatientsRepository patientsRepo,
            IDoctorAvailabilityRepository availabilityRepo,
            IDoctorPrescriptionRepository prescriptionRepo,
            UserManager<ApplicationUser> userManager,
            IDoctorRepository doctorRepo)
        {
            _overviewRepo = overviewRepo;
            _scheduleRepo = scheduleRepo;
            _patientsRepo = patientsRepo;
            _availabilityRepo = availabilityRepo;
            _prescriptionRepo = prescriptionRepo;
            _userManager = userManager;
            _doctorRepo = doctorRepo;
        }

        private async Task<string?> _GetDoctorId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        // ── Overview (Index) ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var vm = await _overviewRepo.GetOverviewAsync(uid);
            return View(vm);
        }

        // ── Today's Schedule ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var vm = await _scheduleRepo.GetScheduleAsync(uid);
            return View(vm);
        }

        // ── Patient List ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Patients(int page = 1, int pageSize = 10)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var vm = await _patientsRepo.GetPatientsAsync(uid, page, pageSize);
            return View(vm);
        }

        // ── Availability Manager ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Availability()
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var vm = await _availabilityRepo.GetAvailabilityAsync(uid);
            return View(vm);
        }

        // ── Edit a single day's working hours ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditAvailabilityDay(string date)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            if (!DateTime.TryParse(date, out var day))
            {
                TempData["ErrorMessage"] = "Invalid date.";
                return RedirectToAction("Availability");
            }

            var vm = await _availabilityRepo.GetAvailabilityDayAsync(uid, day);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAvailabilityDay(EditAvailabilityDayViewModel model)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var slots = new List<WorkingHoursDto>();
            for (int i = 0; i < model.StartTimes.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(model.StartTimes[i]) || string.IsNullOrWhiteSpace(model.EndTimes[i]))
                    continue;
                slots.Add(new WorkingHoursDto
                {
                    Date      = model.Date,
                    StartTime = model.StartTimes[i],
                    EndTime   = model.EndTimes[i]
                });
            }

            var ok = await _availabilityRepo.SaveAvailabilityDayAsync(uid, model.Date, slots);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Availability saved successfully!"
                : "Failed to save availability. Please check your time slots.";

            return RedirectToAction("Availability");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAvailabilityDay(string date)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            await _availabilityRepo.SaveAvailabilityDayAsync(uid, date, new List<WorkingHoursDto>());
            TempData["SuccessMessage"] = "Day cleared.";
            return RedirectToAction("Availability");
        }

        // ── Start Appointment → redirect to prescription page ────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAppointment(int appointmentId)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var ok = await _scheduleRepo.StartAppointmentAsync(uid, appointmentId);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Cannot start this appointment. It may not be confirmed yet.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("WritePrescription", new { appointmentId });
        }

        // ── Write Prescription page ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> WritePrescription(int appointmentId)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            var vm = await _prescriptionRepo.GetPrescriptionContextAsync(uid, appointmentId);
            if (vm == null)
            {
                TempData["ErrorMessage"] = "Appointment not found or access denied.";
                return RedirectToAction("Index");
            }

            return View(vm);
        }

        // ── Save Prescription (traditional form POST) ─────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePrescription(SavePrescriptionFormModel model)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.Diagnosis))
            {
                TempData["ErrorMessage"] = "Diagnosis is required.";
                return RedirectToAction("WritePrescription", new { appointmentId = model.AppointmentId });
            }

            var request = new SavePrescriptionRequest
            {
                AppointmentId = model.AppointmentId,
                Diagnosis     = model.Diagnosis,
                Notes         = model.Notes,
                Medicines     = new List<MedicineDto>()
            };

            for (int i = 0; i < model.MedicineNames.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(model.MedicineNames[i])) continue;
                request.Medicines.Add(new MedicineDto
                {
                    Name      = model.MedicineNames[i],
                    Dosage    = i < model.MedicineDosages.Count   ? model.MedicineDosages[i]   : "",
                    Frequency = i < model.MedicineFrequencies.Count ? model.MedicineFrequencies[i] : "",
                    Duration  = i < model.MedicineDurations.Count ? model.MedicineDurations[i] : ""
                });
            }

            var ok = await _prescriptionRepo.SavePrescriptionAsync(uid, request);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Prescription issued! Appointment marked as Done."
                : "Failed to save prescription. Please try again.";

            return ok ? RedirectToAction("Index") : RedirectToAction("WritePrescription", new { appointmentId = model.AppointmentId });
        }

        // ── Cancel Appointment ───────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var uid = await _GetDoctorId();
            if (uid == null) return RedirectToAction("Login", "Account");

            await _scheduleRepo.CancelAppointmentAsync(uid, appointmentId);
            TempData["SuccessMessage"] = "Appointment cancelled.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDoctor()
        {
            var uid = await _GetDoctorId();

            if (uid == null)
                return RedirectToAction("Login", "Account");

            var model = await _doctorRepo.GetDoctorForEditAsync(uid);

            if (model == null)
                return NotFound();

            ViewBag.Specializations =
                await _doctorRepo.GetSpecializationsAsync();

            return View(model);
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDoctor(
    EditDoctorProfileViewModel model)
        {
            var uid = await _GetDoctorId();

            if (uid == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Specializations =
                    await _doctorRepo.GetSpecializationsAsync();

                return View(model);
            }

            var result =
                await _doctorRepo.UpdateDoctorAsync(uid, model);

            TempData["SuccessMessage"] =
                result
                ? "Profile updated successfully."
                : "Failed to update profile.";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vm = await _doctorRepo.GetMyReviewsAsync(userId);

            return View(vm);
        }

    }
}
