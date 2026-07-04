namespace Midix.Models.ViewModels
{
    public class PharmacyIndexViewModel
    {
        public IEnumerable<Pharmacy> Pharmacies { get; set; } = new List<Pharmacy>();

        public string? SearchTerm { get; set; }
    }
}
