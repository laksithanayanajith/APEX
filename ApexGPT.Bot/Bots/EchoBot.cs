using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.GenAI;
using Google.GenAI.Types;

namespace ApexGPT.Bot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly string _apiKey;

        public EchoBot(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // 1. Show "Typing" indicator
            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            var userText = turnContext.Activity.Text;

            // 2. Get Answer from Gemini
            string aiResponse = await GetGeminiResponse(userText);

            // 3. Send Answer
            await turnContext.SendActivityAsync(MessageFactory.Text(aiResponse, aiResponse), cancellationToken);
        }

        private async Task<string> GetGeminiResponse(string userPrompt)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Error: Gemini API Key is missing.";

            try
            {
                var client = new Client(apiKey: _apiKey);

                // Define the System Instruction (The Bot's Persona)
                var systemInstruction = new Content
                {
                    Role = "system",
                    Parts = new List<Part>
                    {
                        new Part { Text = "You are ApexGPT, an expert IT Support Specialist. " +
                                          "You help users diagnose Windows PC issues, network problems, and driver crashes. " +
                                          "Be professional, concise, and safety-conscious." }
                    }
                };

                var userMessage = new Content
                {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = userPrompt } }
                };

                string finalPrompt = $"SYSTEM: You are ApexGPT, an expert IT Support Specialist.\n" +
                                     $"USER: {userPrompt}";

                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-pro",
                    contents: new List<Content>
                    {
                        new Content
                        {
                            Role = "user",
                            Parts = new List<Part> { new Part { Text = finalPrompt } }
                        }
                    }
                 );

                // Return the text
                if (response?.Candidates != null && response.Candidates.Count > 0)
                {
                    var candidate = response.Candidates[0];
                    var content = candidate?.Content;
                    var parts = content?.Parts;
                    var text = (parts != null && parts.Count > 0) ? parts[0].Text : null;

                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }

                return "No response from Gemini.";
            }
            catch (System.Exception ex)
            {
                return $"Gemini Error: {ex}";
            }
        }
    }
}