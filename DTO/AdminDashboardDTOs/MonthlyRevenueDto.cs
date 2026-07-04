namespace Midix.DTO.AdminDashboardDTOs
{
    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int HeightPercent { get; set; }
        public bool IsCurrentMonth { get; set; }
    }

}
