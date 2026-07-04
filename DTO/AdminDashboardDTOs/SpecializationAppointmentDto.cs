namespace Midix.DTO.AdminDashboardDTOs
{
    public class SpecializationAppointmentDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int MaxCount { get; set; }
        public string Color { get; set; } = "var(--blue)";
        public int WidthPercent => MaxCount > 0 ? (int)(Count * 100.0 / MaxCount) : 0;
    }

}
