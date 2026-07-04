namespace Midix.DTO
{
    public class SlotResultDto
    {
        public List<string> AvailableSlots { get; set; } = new List<string>();
        public List<string> BookedSlots { get; set; } = new List<string>();
    }
}
