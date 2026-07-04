using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midix.Enums;
using Midix.Helpers;
using Midix.IRepository;
using Midix.Models;
using Midix.Services;
using Midix.ViewModel;

namespace Midix.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthRepository _authRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;


        public AccountController(IAuthRepository authRepo, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _authRepo = authRepo;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // LOGIN

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var result = await _authRepo.LoginAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Welcome back!";


                var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                var roles = await _userManager.GetRolesAsync(user);


                if (roles.Contains(UserRole.Admin.ToString()))
                    return RedirectToAction("Index", "AdminDashboard");

                if (roles.Contains(UserRole.Doctor.ToString()))
                    return RedirectToAction("Index", "DoctorDashboard");

                return RedirectToAction("Index", "PatientDashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Your account has been locked out due to too many failed attempts. Please try again later.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty,
                    "Your account is inactive. Please contact support.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty,
                "Invalid email or password. Please try again.");
            return View(model);
        }

        // REGISTER

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new Registerviewmodel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Registerviewmodel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await UserHelper.EmailExistsAsync(_userManager, model.Email))
            {
                ModelState.AddModelError("Email",
                    "This email address is already registered. Please log in or use a different email.");
                return View(model);
            }

            var result = await _authRepo.RegisterAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Account created successfully! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // LOGOUT

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authRepo.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, token) = await _authRepo.GeneratePasswordResetTokenAsync(model.Email);

            System.Diagnostics.Debug.WriteLine($"[FORGOT-PW-DEBUG] Email entered: '{model.Email}' | success={success} | token={(token == null ? "NULL" : token.Substring(0, 10) + "...")}");

            if (success && token != null)
            {
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, token = token }, Request.Scheme);

                var htmlMessage = $@"
        <p>You requested to reset your password.</p>
        <p><a href='{resetLink}'>Click here to reset your password</a></p>
        <p>If you didn't request this, please ignore this email.</p>";

                try
                {
                    await _emailSender.SendEmailAsync(model.Email, "Reset Your Medix Password", htmlMessage);
                    System.Diagnostics.Debug.WriteLine("[FORGOT-PW-DEBUG] Email sent SUCCESSFULLY");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FORGOT-PW-DEBUG] Email FAILED: {ex.GetType().Name}: {ex.Message}");
                    if (ex.InnerException != null)
                        System.Diagnostics.Debug.WriteLine($"[FORGOT-PW-DEBUG] Inner: {ex.InnerException.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[FORGOT-PW-DEBUG] Skipped sending — success was false or token was null (email likely not found in DB)");
            }

            TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        // RESET PASSWORD 

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authRepo.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully. Please log in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


    }
}