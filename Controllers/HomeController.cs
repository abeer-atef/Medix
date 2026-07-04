using Microsoft.AspNetCore.Mvc;
using Midix.IRepository.IDoctor;
using Midix.Models;
using Midix.ViewModel;
using System.Diagnostics;

namespace Midix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDoctorRepository _doctorRepo;

        public HomeController(ILogger<HomeController> logger, IDoctorRepository doctorRepo)
        {
            _logger = logger;
            _doctorRepo = doctorRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Doctors(
            string? search,
            int? specializationId,
            int page = 1)
        {
            var vm = await _doctorRepo.GetAllDoctorsAsync(
             search,
             specializationId,
             page);

            return View(vm);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult BadUrl()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}