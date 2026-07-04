namespace Midix.DTO.ChatDTOs
{

    public class ChatMessageDto { public string Role { get; set; } = string.Empty; public string Content { get; set; } = string.Empty; }
    public class ChatRequest { public List<ChatMessageDto> Messages { get; set; } = new(); }
    public class ChatResponse { public string Reply { get; set; } = string.Empty; }
}
