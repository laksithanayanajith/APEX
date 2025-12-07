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
        private readonly TicketService _ticketService;

        public AIController(IConfiguration configuration, TicketService ticketService)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            _ticketService = ticketService;
        }

        public class UserRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public string UserId { get; set; } = "user_0001";
        }

        [HttpGet("history/{userId}")]
        public IActionResult GetTicketHistory(string userId)
        {
            try
            {
                var history = _ticketService.GetTicketsByUserId(userId);

                if (history.Count > 0)
                {
                    return Ok(history);
                }
                else
                {
                    return NotFound(new { message = $"No ticket history found for user ID: {userId}" });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error fetching history: {ex.Message}");
            }
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
            string kbContext = "";
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

                // 3. SMART PROMPT (Includes all new commands and enhanced structure)
                string systemPrompt = $"SYSTEM: You are ApexGPT, an expert IT Support Agent. Your primary goal is to diagnose and resolve the USER's reported issue based on the provided CONTEXT.\n\n" +

                                      $"CONTEXT & DATA:\n" +
                                      $"- TICKET ID: {ticket.Id} | STATUS: {ticket.Status}\n" +
                                      $"- KNOWLEDGE BASE:\n{kbContext}\n\n" +

                                      $"PRIORITY INSTRUCTIONS (Follow these steps strictly):\n" +

                                      $"1. **GUARDRAIL REFUSAL:** If the user's message is not related to IT, computers, or technical support, you MUST reply ONLY with the refusal phrase: 'I am only designed to assist with IT and PC troubleshooting. How can I help you with your computer issues?'\n\n" +

                                      $"2. **COMMAND DECISION:** Based ONLY on the ticket status and user input, reply ONLY with a COMMAND if an action is needed. Choose from the following options:\n" +
                                      $"\t- Low Memory/High CPU: 'COMMAND:REDUCE_RAM'\n" +
                                      $"\t- Network Issue: 'COMMAND:CHECK_PING' OR 'COMMAND:FLUSH_DNS'\n" +
                                      $"\t- Disk Space Low: 'COMMAND:CLEAN_DISK'\n" +
                                      $"\t- System Health Diagnostic: 'COMMAND:CHECK_DISK'\n" +
                                      $"\t- Issue Resolved: 'COMMAND:RESOLVE'\n" +
                                      $"\t- Critical Failure/Cannot Resolve: 'COMMAND:ESCALATE'\n\n" +

                                      $"3. **STANDARD RESPONSE:** If you cannot issue a command or the command is not appropriate, provide concise, professional, and helpful advice.\n\n" +

                                      $"USER: {request.Prompt}";

                var response = await client.Models.GenerateContentAsync(
                    //model: "gemini-1.5-flash", // Corrected model for stable quotas
                    model: "gemini-2.5-flash",
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

                // 4. ACTION HANDLING
                if (answer.Contains("COMMAND:ESCALATE"))
                {
                    _ticketService.EscalateTicket(ticket.Id, "AI could not resolve the issue or detected danger.");
                    answer = $"I have detected a critical issue. I have ESCALATED Ticket {ticket.Id} to a Senior Human Technician immediately.";
                }
                else if (answer.Contains("COMMAND:RESOLVE"))
                {
                    _ticketService.ResolveTicket(ticket.Id, "Resolved via Chatbot");
                    answer = "I have marked this ticket as resolved. Is there anything else I can help with?";
                }
                else if (answer.Contains("COMMAND:"))
                {
                    // Logs action for new commands: CHECK_DISK, REDUCE_RAM, CLEAN_DISK, etc.
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