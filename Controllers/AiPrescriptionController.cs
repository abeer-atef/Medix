using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midix.Data;
using Midix.Models;
using Midix.Enums;
using Midix.Models.ViewModels;
using Midix.Services;
using Midix.ViewModel;
using Midix.Helpers;
using System.Text.Json;

namespace Midix.Controllers
{

    [Authorize(Roles = nameof(UserRole.Patient))]
    [Route("PatientDashboard/[controller]")]
    public class AiPrescriptionController : Controller
    {
        // ?? Dependencies ?????????????????????????????????????????????????????????

        private readonly IPrescriptionAiService _aiService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AiPrescriptionController> _logger;

        // ?? Constructor ??????????????????????????????????????????????????????????

        public AiPrescriptionController(
            IPrescriptionAiService aiService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<AiPrescriptionController> logger)
        {
            _aiService = aiService;
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // ?? Helpers ??????????????????????????????????????????????????????????????

        /// <summary>Resolves the currently authenticated patient's UserId.</summary>
        private async Task<string?> GetPatientIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        /// <summary>
        /// Builds the shared sidebar header (name, initials, email, picture) so this
        /// controller's views render the exact same sidebar as the rest of the patient area.
        /// </summary>
        private async Task<PatientHeaderInfo> LoadHeaderAsync(string patientId)
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

        // ?? GET /PatientDashboard/Prescriptions ??????????????????????????????????

        /// <summary>
        /// Renders the prescription image upload form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var patientId = await GetPatientIdAsync();
            if (patientId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["PatientHeader"] = await LoadHeaderAsync(patientId);
            return View();
        }

        // ?? POST /PatientDashboard/Prescriptions/Analyze ?????????????????????????

        /// <summary>
        /// Accepts the uploaded prescription image, sends it to the Groq Vision AI,
        /// persists the result, and redirects to the analysis result view.
        /// </summary>
        [HttpPost("Analyze")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(UploadPrescriptionRequest request)
        {
            // 1. Resolve patient identity ?????????????????????????????????????
            var patientId = await GetPatientIdAsync();
            if (patientId == null)
                return RedirectToAction("Login", "Auth");

            ViewData["PatientHeader"] = await LoadHeaderAsync(patientId);

            // 2. Validate the uploaded file ???????????????????????????????????
            if (!ModelState.IsValid || request.PrescriptionImage == null || request.PrescriptionImage.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid prescription image.");
                return View("Index", request);
            }

            // Enforce allowed image MIME types
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(request.PrescriptionImage.ContentType.ToLower()))
            {
                ModelState.AddModelError(string.Empty, "Only JPEG, PNG, WebP, or GIF images are accepted.");
                return View("Index", request);
            }

            // Enforce a 10 MB file size cap
            const long maxBytes = 10 * 1024 * 1024;
            if (request.PrescriptionImage.Length > maxBytes)
            {
                ModelState.AddModelError(string.Empty, "Image must be smaller than 10 MB.");
                return View("Index", request);
            }

            try
            {
                // 3. Call the AI service ??????????????????????????????????????
                _logger.LogInformation(
                    "Patient {PatientId} submitted a prescription image for AI analysis.", patientId);

                string rawJson = await _aiService.AnalyzePrescriptionAsync(request.PrescriptionImage);

                // 4. Deserialise the AI JSON into the strongly-typed DTO ??????
                var analysisResult = JsonSerializer.Deserialize<StructuredPrescriptionAnalysis>(
                    rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("AI service returned an empty or invalid JSON response.");

                // 5. Persist the image to wwwroot/uploads/prescriptions ???????
                string uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads", "prescriptions");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.PrescriptionImage.FileName)}";
                string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(physicalPath, FileMode.Create))
                {
                    await request.PrescriptionImage.CopyToAsync(fileStream);
                }

                string imageUrl = $"/uploads/prescriptions/{uniqueFileName}";

                // 6. Persist the Prescription record to the database ??????????
                var prescription = new Prescription
                {
                    PatientId = patientId,
                    ImageUrl = imageUrl,
                    AiAnalysisResult = rawJson,          // Store raw JSON for auditability
                    UploadedAt = DateTime.UtcNow,
                    // MedicalRecordId is intentionally left as default (0) for
                    // AI-only prescriptions not linked to a formal appointment.
                    // A migration can make this nullable; for now we store as-is.
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Prescription record {PrescriptionId} saved for patient {PatientId}.",
                    prescription.Id, patientId);

                // 7. Pass the structured result to the AnalysisResult view ????
                TempData["SuccessMessage"] = "Prescription analysed successfully!";
                return View("AnalysisResult", analysisResult);
            }
            catch (InvalidOperationException ex)
            {
                // ?? DEBUG ONLY — exposes full exception detail in the browser
                _logger.LogWarning(ex, "AI analysis failed for patient {PatientId}.", patientId);
                return Content($"[InvalidOperationException] Error: {ex.Message} | InnerException: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                // ?? DEBUG ONLY — exposes full exception detail in the browser
                _logger.LogError(ex, "Unexpected error during prescription analysis for patient {PatientId}.", patientId);
                return Content($"[{ex.GetType().Name}] Error: {ex.Message} | InnerException: {ex.InnerException?.Message} | StackTrace: {ex.StackTrace}");
            }
        }
    }
}