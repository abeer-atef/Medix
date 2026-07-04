namespace Midix.ViewModel 
{
    public class ChatMessageViewModel
    {
        public string Message { get; set; }
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now; 
    }
}
