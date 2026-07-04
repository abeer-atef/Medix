using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midix.Enums;
using Midix.Helpers;
using Midix.IRepository.IAdmin;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Controllers
{
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminDashboardController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly IAdminOverviewRepository _overviewRepo;
        private readonly IAdminStaffRepository _staffRepo;
        private readonly IAdminAppointmentsRepository _appointmentsRepo;
        private readonly IAdminFinancialRepository _financialRepo;
        private readonly IAdminDoctorManagementRepository _doctorMgmtRepo;
        private readonly IAdminUsersRepository _usersRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDashboardController(
            IAdminRepository adminRepo,
            IAdminOverviewRepository overviewRepo,
            IAdminStaffRepository staffRepo,
            IAdminAppointmentsRepository appointmentsRepo,
            IAdminFinancialRepository financialRepo,
            IAdminDoctorManagementRepository doctorMgmtRepo,
            IAdminUsersRepository usersRepo,
            IDoctorRepository doctorRepo,
            UserManager<ApplicationUser> userManager)
        {
            _adminRepo = adminRepo;
            _overviewRepo = overviewRepo;
            _staffRepo = staffRepo;
            _appointmentsRepo = appointmentsRepo;
            _financialRepo = financialRepo;
            _doctorMgmtRepo = doctorMgmtRepo;
            _usersRepo = usersRepo;
            _doctorRepo = doctorRepo;
            _userManager = userManager;
        }

        // ── Dashboard Overview (stats + charts only — fast) ──────────────────

        public async Task<IActionResult> Index()
        {
            var vm = await _overviewRepo.GetOverviewAsync();
            return View(vm);
        }

        // ── Staff section ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Staff(int page = 1, int pageSize = 10)
        {
            var vm = await _staffRepo.GetStaffAsync(page, pageSize);
            return View(vm);
        }

        // ── Appointments section ─────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Appointments(int page = 1, int pageSize = 10)
        {
            var vm = await _appointmentsRepo.GetAppointmentsAsync(page, pageSize);
            return View(vm);
        }

        // ── Financial section ────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Financial(int page = 1, int pageSize = 10)
        {
            var vm = await _financialRepo.GetFinancialAsync(page, pageSize);
            return View(vm);
        }

        // ── Add Doctor ───────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> AddDoctor()
        {
            var model = new AddDoctorViewModel
            {
                Specializations = await _doctorRepo.GetSpecializationsAsync()
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(AddDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Specializations = await _doctorRepo.GetSpecializationsAsync();
                return View(model);
            }

            if (await _doctorRepo.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                model.Specializations = await _doctorRepo.GetSpecializationsAsync();
                return View(model);
            }

            var result = await _doctorRepo.AddDoctorAsync(model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Dr. {model.FirstName} {model.LastName} added successfully!";
                return RedirectToAction("Index");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            model.Specializations = await _doctorRepo.GetSpecializationsAsync();
            return View(model);
        }

        // ── Edit Doctor ──────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditDoctor(string userId)
        {
            var model = await _doctorMgmtRepo.GetDoctorForEditAsync(userId);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(EditDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Specializations = await _doctorRepo.GetSpecializationsAsync();
                return View(model);
            }

            var ok = await _doctorMgmtRepo.UpdateDoctorAsync(model);
            if (ok)
            {
                TempData["SuccessMessage"] = $"Dr. {model.FirstName} {model.LastName} updated successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Update failed. Please try again.");
            model.Specializations = await _doctorRepo.GetSpecializationsAsync();
            return View(model);
        }

        // ── Deactivate Doctor ────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> DeactivateDoctor(string userId)
        {
            var doctor = await _doctorMgmtRepo.GetDoctorForEditAsync(userId);
            if (doctor == null) return NotFound();

            var upcomingCount = await _doctorMgmtRepo.GetUpcomingAppointmentsCountAsync(userId);

            var vm = new DeactivateDoctorViewModel
            {
                UserId       = userId,
                DoctorName   = $"Dr. {doctor.FirstName} {doctor.LastName}",
                UpcomingCount = upcomingCount
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateDoctor(string userId, bool cancelAppointments)
        {
            if (cancelAppointments)
                await _doctorMgmtRepo.CancelDoctorAppointmentsAsync(userId);

            await _doctorMgmtRepo.ToggleDoctorActiveAsync(userId);

            TempData["SuccessMessage"] = cancelAppointments
                ? "Doctor deactivated and upcoming appointments cancelled."
                : "Doctor deactivated.";

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateDoctor(string userId)
        {
            await _doctorMgmtRepo.ToggleDoctorActiveAsync(userId);
            TempData["SuccessMessage"] = "Doctor activated successfully.";
            return RedirectToAction("Index");
        }

        // ── Add Admin ────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult AddAdmin() => View(new AddAdminViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(AddAdminViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await UserHelper.EmailExistsAsync(_userManager,model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var result = await _adminRepo.AddAdminAsync(model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Admin {model.FirstName} {model.LastName} added successfully!";
                return RedirectToAction("Index");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(model);
        }

        // ── Users: Admins tab (own action/view) ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AllUsers(int page = 1, int pageSize = 10)
        {
            var vm = await _usersRepo.GetAdminsTabAsync(page, pageSize);
            return View(vm);
        }

        // ── Users: Patients tab (own action/view) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> AllPatients(int page = 1, int pageSize = 10)
        {
            var vm = await _usersRepo.GetPatientsTabAsync(page, pageSize);
            return View(vm);
        }

        // ── Users: Doctors tab (own action/view) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AllDoctors(int page = 1, int pageSize = 10)
        {
            var vm = await _usersRepo.GetDoctorsTabAsync(page, pageSize);
            return View(vm);
        }
        public async Task<IActionResult> DoctorReviews(string doctorId)
        {
            var vm = await _usersRepo.GetDoctorReviewsAsync(doctorId);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int ratingId, string doctorId)
        {
            await _usersRepo.DeleteReviewAsync(ratingId);

            TempData["SuccessMessage"] = "Review deleted successfully.";

            return RedirectToAction(nameof(DoctorReviews), new { doctorId });
        }
    }
}
