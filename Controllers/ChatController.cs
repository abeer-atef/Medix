using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.AspNetCore.Authorization;
using Midix.DTO.ChatDTOs;

namespace Midix.Controllers
{
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IConfiguration configuration, ILogger<ChatController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetSystemPrompt()
        {
            string userRole = User.IsInRole("Admin") ? "Admin" :
                             (User.IsInRole("Doctor") ? "Doctor" : "Patient");

            if (userRole == "Patient")
            {
                return @"You are MedixBot, a friendly, warm, and highly helpful AI assistant for the Medix Healthcare web platform.

[RESPONSE STYLE & TONE]
- Mimic a natural, conversational, and very polite human assistant.
- Always end your initial instructions by offering further assistance.
- If the user says 'ok' or acknowledges you, respond warmly and check on their progress.
- If the user says 'thanks', say 'You're welcome', suggest other features, and wish them a great day.

[STRICT BOUNDARIES - CRITICAL]
1. You only guide users on HOW to use the website. NEVER attempt to book the appointment within the chat itself.
2. NEVER ask the user what type of doctor they need or what their symptoms are.
3. NEVER mention a physical reception desk, phone numbers, or calling the clinic.

[NAVIGATION INSTRUCTIONS]
- Booking: 'To book an appointment, please go to the 'Book Appointment' tab in the left sidebar.'
- Managing/History: Direct them to the 'My Appointments' tab.
- Prescriptions: Direct them to the 'AI Analysis' tab.";
            }
            else
            {
                // Prompt for Admin and Doctor
                return $@"You are 'Midix Assistant'. The current user is a {userRole}. You are talking to an authorized clinic staff member.
            ## Core Rules
            - Respond in the same language as the user (Arabic/English).
            - Be professional, warm, and concise (3-5 sentences).
            - Do NOT provide medical diagnoses.
            - Only assist with clinic navigation, staff tools, and general inquiries.";
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetBotResponse([FromBody] ChatRequest request)
        {
            if (request?.Messages == null || request.Messages.Count == 0)
                return BadRequest(new ChatResponse { Reply = "No messages provided." });

            try
            {
                var apiKey = _configuration["OpenAiSettings:ApiKey"];
                var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") });
                var chatClient = client.GetChatClient("llama-3.3-70b-versatile");

                var chatMessages = new List<ChatMessage> { new SystemChatMessage(GetSystemPrompt()) };

                foreach (var msg in request.Messages)
                {
                    if (msg.Role?.ToLower() == "user") chatMessages.Add(new UserChatMessage(msg.Content));
                    else if (msg.Role?.ToLower() == "assistant") chatMessages.Add(new AssistantChatMessage(msg.Content));
                }

                var completion = await chatClient.CompleteChatAsync(chatMessages);
                return Ok(new ChatResponse { Reply = completion.Value.Content[0].Text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot error.");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }
    }
}