namespace Midix.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
