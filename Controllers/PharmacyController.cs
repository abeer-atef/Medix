using Microsoft.AspNetCore.Mvc;

namespace Midix.Controllers
{
    public class PharmacyController : Controller
    {
        // GET: /Pharmacy
        // The list is populated entirely client-side via the OpenStreetMap
        // Overpass API — see wwwroot/js/pharmacy.js (fetchNearbyPharmacies).
        public IActionResult Index()
        {
            return View();
        }
    }
}
