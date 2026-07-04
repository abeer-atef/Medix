using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Midix.Data;
using Midix.IRepository.IPatient;
using Midix.Models;
using Midix.DTO;
using Midix.ViewModel;
using Stripe.Checkout;
using Microsoft.AspNetCore.Hosting;
using Midix.DTO.PatientDashboardDTOs;
using Midix.Enums;

namespace Midix.Controllers
{
    [Authorize(Roles = nameof(UserRole.Patient))]
    public class PatientDashboardController : Controller
    {
        private readonly IPatientOverviewRepository _overviewRepo;
        private readonly IPatientBookingRepository _bookingRepo;
        private readonly IPatientAppointmentsRepository _appointmentsRepo;
        private readonly IPatientPrescriptionsRepository _prescriptionsRepo;
        private readonly IPatientProfileRepository _profileRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public PatientDashboardController(
      IPatientOverviewRepository overviewRepo,
      IPatientBookingRepository bookingRepo,
      IPatientAppointmentsRepository appointmentsRepo,
      IPatientPrescriptionsRepository prescriptionsRepo,
      IPatientProfileRepository profileRepo,
      UserManager<ApplicationUser> userManager,
      ApplicationDbContext context,
      IWebHostEnvironment environment)
        {
            _overviewRepo = overviewRepo;
            _bookingRepo = bookingRepo;
            _appointmentsRepo = appointmentsRepo;
            _prescriptionsRepo = prescriptionsRepo;
            _profileRepo = profileRepo;
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        private async Task<string?> _GetPatientId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        // ── Overview ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            var vm = await _overviewRepo.GetOverviewAsync(pid);
            return View(vm);
        }

        // ── Book Appointment: Step 1 — choose doctor ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            var vm = await _bookingRepo.GetBookingDataAsync(pid);
            return View(vm);
        }

        // ── Book Appointment: Step 2 — choose date & time ────────────────────
        [HttpGet]
        public async Task<IActionResult> SelectTime(string doctorId, string? date)
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("BookAppointment");

            var vm = await _bookingRepo.GetBookingDataAsync(pid);
            var doctor = vm.AvailableDoctors.FirstOrDefault(d => d.UserId == doctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Selected doctor was not found.";
                return RedirectToAction("BookAppointment");
            }

            vm.SelectedDoctorId = doctor.UserId;
            vm.SelectedDoctorName = doctor.FullName;
            vm.SelectedSpec = doctor.Specialization;
            vm.SelectedFee = doctor.ConsultationFee;
            vm.SelectedDate = string.IsNullOrEmpty(date) ? DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd") : date;

            return View(vm);
        }

        // GET /PatientDashboard/GetAvailableSlots?doctorId=xxx&date=2025-06-10
        // (still needed: slot availability changes per date, an AJAX-free page reload
        //  would otherwise require resubmitting the whole step for each date click;
        //  this endpoint only returns JSON data, no markup, so it doesn't add JS-driven UI)
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(string doctorId, string date)
        {
            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(date))
                return BadRequest();

            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest();

            var result = await _bookingRepo.GetAvailableSlotsAsync(doctorId, selectedDate);
            return Json(result);
        }

        // ── Book Appointment: Step 3 — payment & confirm ─────────────────────
        [HttpGet]
        public async Task<IActionResult> ConfirmBooking(string doctorId, string date, string time, string appointmentType = "NewVisit")
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                TempData["ErrorMessage"] = "Please select a date and time first.";
                return RedirectToAction("BookAppointment");
            }

            var vm = await _bookingRepo.GetBookingDataAsync(pid);
            var doctor = vm.AvailableDoctors.FirstOrDefault(d => d.UserId == doctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Selected doctor was not found.";
                return RedirectToAction("BookAppointment");
            }

            vm.SelectedDoctorId = doctor.UserId;
            vm.SelectedDoctorName = doctor.FullName;
            vm.SelectedSpec = doctor.Specialization;
            vm.SelectedAppointmentType = appointmentType;
            vm.SelectedFee = appointmentType == "FollowUp"
                                             ? (doctor.FollowUpFee > 0 ? doctor.FollowUpFee : doctor.ConsultationFee)
                                             : doctor.ConsultationFee;
            vm.SelectedFollowUpFee = doctor.FollowUpFee > 0 ? doctor.FollowUpFee : doctor.ConsultationFee;
            vm.SelectedDate = date;
            vm.SelectedTime = time;

            return View(vm);
        }
        // ── Final submit (stripe) ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointmentConfirm( string doctorId, string appointmentDate, string appointmentTime,
             string paymentMethod, string appointmentType)   
        {

            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(doctorId) ||
                !DateTime.TryParse($"{appointmentDate} {appointmentTime}", out var dateTime))
            {
                TempData["ErrorMessage"] = "Please select a valid doctor, date, and time.";
                return RedirectToAction("BookAppointment");
            }

            if (!Enum.TryParse<PaymentMethod>(paymentMethod, true, out var method))
                method = PaymentMethod.Cash;

            if (!Enum.TryParse<AppointmentType>(appointmentType, true, out var apptType))
                apptType = AppointmentType.NewVisit;

            var (result, appointment) = await _bookingRepo.BookAppointmentAsync(
                pid, doctorId, dateTime, method, apptType);


            if (result != BookingResult.Success || appointment == null)
            {
                TempData["ErrorMessage"] = result switch
                {
                    BookingResult.InvalidDate => "Appointments can only be booked within the next 7 days.",
                    BookingResult.DoctorNotFound => "The selected doctor was not found.",
                    BookingResult.UnExpectedEror => "Something went wrong. Please try again.",
                    _ => "An unknown error occurred."
                };
                return RedirectToAction("Index");
            }

            if (method == PaymentMethod.Online)
            {
                var checkoutUrl = await CreateStripeCheckoutSessionAsync(appointment.Payment);
                return Redirect(checkoutUrl);
            }

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string session_id)
        {
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            var payment = await _context.Payments
                .Include(p => p.Appointment)
                .FirstOrDefaultAsync(p => p.StripeSessionId == session_id);

            if (session.PaymentStatus == "paid" && payment != null)
            {
                payment.Status = PaymentStatus.Paid;

                if (payment.Appointment != null && payment.Appointment.State == AppointmentState.Pending)
                {
                    payment.Appointment.State = AppointmentState.Confirmed;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment completed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Payment could not be confirmed.";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCancel(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment != null && payment.Status == PaymentStatus.Pending)
            {
                _context.Payments.Remove(payment);
                payment.Appointment.State = AppointmentState.Cancelled;
                await _context.SaveChangesAsync();
            }

            TempData["ErrorMessage"] = "Payemt has been Canceled";
            return RedirectToAction("Index");
        }

        private async Task<string> CreateStripeCheckoutSessionAsync(Payment payment)
        {
            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Quantity = 1,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(payment.Amount * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Medix Appointment Fee"
                    }
                }
            }
        },
                Mode = "payment",
                SuccessUrl = $"{domain}/PatientDashboard/PaymentSuccess?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/PatientDashboard/PaymentCancel?paymentId={payment.Id}"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            payment.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return session.Url;
        }

        // ── My Appointments (paginated) ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Appointments(int upcomingPage = 1, int pastPage = 1, int pageSize = 10)
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            var vm = await _appointmentsRepo.GetAppointmentsAsync(pid, upcomingPage, pastPage, pageSize);
            return View(vm);
        }

        // ── Prescriptions (paginated) ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Prescriptions(int page = 1, int pageSize = 10)
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            var vm = await _prescriptionsRepo.GetPrescriptionsAsync(pid, page, pageSize);
            return View(vm);
        }

        // ── Profile ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            var vm = await _profileRepo.GetProfileAsync(pid);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
    PatientProfileDto model,
    IFormFile? ProfileImage)
        {
            var pid = await _GetPatientId();
            if (pid == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your inputs.";
                return RedirectToAction("Profile");
            }
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(model.ProfileImage.FileName);

                var uploadPath = Path.Combine(
                    _environment.WebRootPath,
                    "uploads");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    user.Picture = "/uploads/" + fileName;
                    await _userManager.UpdateAsync(user);
                }
            }


            var success = await _profileRepo.UpdateProfileAsync(pid, model);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Profile updated successfully!"
                : "Failed to update profile.";

            return RedirectToAction("Profile");
        }
    }

}