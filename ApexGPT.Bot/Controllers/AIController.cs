using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Google.GenAI;
using Google.GenAI.Types;
using ApexGPT.Bot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ApexGPT.Bot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly IConfiguration configuration;
        private readonly TicketService _ticketService;

        public AIController(IConfiguration configuration, TicketService ticketService)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            this.configuration = configuration;
            _ticketService = ticketService;
        }

        public class UserRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public string UserId { get; set; } = "user_0001";
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskGemini([FromBody] UserRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey)) return BadRequest("API Key missing");

            // 1. TICKET MANAGEMENT
            var ticket = _ticketService.GetActiveTicket(request.UserId);
            if (ticket == null)
            {
                ticket = _ticketService.CreateTicket(request.UserId, request.Prompt, "General");
            }

            // 2. KNOWLEDGE BASE CONTEXT
            var articles = _ticketService.GetAllArticles();
            string kbContext = "Here are some relevant Knowledge Base articles:\n";
            foreach (var art in articles)
            {
                if (request.Prompt.ToLower().Contains(art.Category.ToLower()) ||
                    request.Prompt.ToLower().Contains(art.Title.Split(' ')[0].ToLower()))
                {
                    kbContext += $"- KB ID: {art.Id}, Issue: {art.Title}, Fix: {string.Join(", ", art.Resolution_Steps)}\n";
                }
            }

            try
            {
                var client = new Client(apiKey: _apiKey);

                // 3. SMART PROMPT (Updated with ESCALATE instructions)
                string systemPrompt = $"SYSTEM: You are ApexGPT, an expert IT Support Agent.\n" +
                                      $"TICKET ID: {ticket.Id} | STATUS: {ticket.Status}\n" +
                                      $"{kbContext}\n" +
                                      $"INSTRUCTIONS:\n" +
                                      $"1. If you can fix it safely, reply: 'COMMAND:CHECK_DISK', 'COMMAND:CHECK_PING', or 'COMMAND:FLUSH_DNS'.\n" +
                                      $"2. If the issue is solved, reply: 'COMMAND:RESOLVE'.\n" +
                                      $"3. CRITICAL: If the issue is dangerous (smoke, fire, hardware failure) OR if you cannot solve it, reply: 'COMMAND:ESCALATE'.\n" +
                                      $"4. Otherwise, provide helpful advice.\n" +
                                      $"USER: {request.Prompt}";

                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-pro", // Using the smart model
                    contents: new List<Content>
                    {
                        new Content
                        {
                            Role = "user",
                            Parts = new List<Part> { new Part { Text = systemPrompt } }
                        }
                    }
                );

                string answer = response?.Candidates?[0].Content?.Parts?[0].Text ?? "No response";

                // 4. ACTION HANDLING (Now includes Escalation)
                if (answer.Contains("COMMAND:ESCALATE"))
                {
                    // Call the new service method
                    _ticketService.EscalateTicket(ticket.Id, "AI could not resolve the issue or detected danger.");

                    // Override answer for the user
                    answer = $"I have detected a critical issue. I have ESCALATED Ticket {ticket.Id} to a Senior Human Technician immediately.";
                }
                else if (answer.Contains("COMMAND:RESOLVE"))
                {
                    _ticketService.ResolveTicket(ticket.Id, "Resolved via Chatbot");
                    answer = "I have marked this ticket as resolved. Is there anything else I can help with?";
                }
                else if (answer.Contains("COMMAND:"))
                {
                    _ticketService.LogAction(ticket.Id, $"AI executed automated command: {answer}");
                }
                else
                {
                    _ticketService.LogAction(ticket.Id, "AI provided consultation.");
                }

                return Ok(new { response = answer, ticketId = ticket.Id });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"AI Error: {ex.Message}");
            }
        }
    }
}