using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midix.Enums;
using Midix.IRepository.IRating;
using Midix.Models;
using Midix.ViewModel;

namespace Midix.Controllers
{
    [Authorize(Roles = "Patient")]
    public class RatingController : Controller
    {
        private readonly IRatingRepository _ratingRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingController(
            IRatingRepository ratingRepo,
            UserManager<ApplicationUser> userManager)
        {
            _ratingRepo = ratingRepo;
            _userManager = userManager;
        }

        // GET: /Rating/Index
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveTab"] = "rating";

            var patientId = _userManager.GetUserId(User)!;
            var appointments = await _ratingRepo.GetRateableDoctorsAsync(patientId);

            var vm = appointments.Select(a => new RateableDoctorVM
            {
                AppointmentId = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName,
                Specialization = a.Doctor.Specialization.Name,
                AppointmentDate = a.Date
            }).ToList();

            return View(vm);
        }

        // GET: /Rating/Create?appointmentId=5
        public async Task<IActionResult> Create(int appointmentId)
        {
            ViewData["ActiveTab"] = "rating";

            var patientId = _userManager.GetUserId(User)!;
            var appointment = await _ratingRepo.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null || appointment.PatientId != patientId)
                return NotFound();

            if (appointment.State != AppointmentState.Done)
            {
                TempData["Error"] = "You can only rate a doctor after your appointment is completed.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _ratingRepo.GetByAppointmentIdAsync(appointmentId);
            if (existing != null)
            {
                TempData["Error"] = "You have already submitted a rating for this appointment.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new SubmitRatingVM
            {
                AppointmentId = appointmentId,
                DoctorName = appointment.Doctor.User.FirstName + " " + appointment.Doctor.User.LastName,
                Specialization = appointment.Doctor.Specialization.Name,
                AppointmentDate = appointment.Date,
                Rate = 5
            };

            return View(vm);
        }

        // POST: /Rating/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmitRatingVM model)
        {
            ViewData["ActiveTab"] = "rating";

            if (!ModelState.IsValid)
                return View(model);

            var patientId = _userManager.GetUserId(User)!;
            var appointment = await _ratingRepo.GetAppointmentByIdAsync(model.AppointmentId);

            if (appointment == null || appointment.PatientId != patientId)
                return NotFound();

            if (appointment.State != AppointmentState.Done)
            {
                ModelState.AddModelError("", "You can only rate a doctor after your appointment is completed.");
                return View(model);
            }

            var existing = await _ratingRepo.GetByAppointmentIdAsync(model.AppointmentId);
            if (existing != null)
            {
                ModelState.AddModelError("", "You have already submitted a rating for this appointment.");
                return View(model);
            }

            var rating = new Rating
            {
                Rate = model.Rate,
                Comment = model.Comment,
                PatientId = patientId,
                DoctorId = appointment.DoctorId,
                AppointmentId = model.AppointmentId,
                CreatedAt = DateTime.UtcNow
            };

            await _ratingRepo.AddAsync(rating);

            TempData["Success"] = "Your rating has been submitted successfully. Thank you!";
            return RedirectToAction(nameof(Index));
        }
    }
}