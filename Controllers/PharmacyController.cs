using Microsoft.AspNetCore.Mvc;
using Midix.DTO;
using Midix.Helpers;
using Midix.Models.ViewModels;
using Midix.Repositories;

namespace Midix.Controllers
{
    public class PharmacyController : Controller
    {
        private readonly IPharmacyRepository _pharmacyRepository;

        public PharmacyController(IPharmacyRepository pharmacyRepository)
        {
            _pharmacyRepository = pharmacyRepository;
        }

        // GET: /Pharmacy
        // Renders the full "Nearby Pharmacies" page. The list shown here is just the
        // initial server-rendered fallback; pharmacy.js immediately replaces it with
        // a distance-sorted list once it has (or fails to get) the user's location.
        public async Task<IActionResult> Index()
        {
            var pharmacies = await _pharmacyRepository.GetAllAsync();

            var viewModel = new PharmacyIndexViewModel
            {
                Pharmacies = pharmacies
            };

            return View(viewModel);
        }

        // GET: /Pharmacy/GetNearby?lat=..&lng=..&search=..&take=..
        // Called via fetch() from the page and from the homepage widget.
        [HttpGet]
        public async Task<IActionResult> GetNearby(double? lat, double? lng, string? search = null, int take = 0)
        {
            var pharmacies = await _pharmacyRepository.SearchAsync(search);

            var hasLocation = lat.HasValue && lng.HasValue;

            IEnumerable<PharmacyDistanceDto> result = pharmacies.Select(p => new PharmacyDistanceDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                PhoneNumber = p.PhoneNumber,
                DistanceKm = hasLocation
                    ? GeoUtils.CalculateDistanceKm(lat!.Value, lng!.Value, p.Latitude, p.Longitude)
                    : null
            });

            result = hasLocation
                ? result.OrderBy(p => p.DistanceKm)
                : result.OrderBy(p => p.Name);

            if (take > 0)
            {
                result = result.Take(take);
            }

            return Json(result);
        }
    }
}
