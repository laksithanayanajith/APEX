using ApexGPTWinUI.Models; // Required for TicketHistoryItem
using ApexGPTWinUI.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI;

namespace ApexGPTWinUI
{
    public class UIMessage
    {
        public string Text { get; set; } = string.Empty;
        public SolidColorBrush Color { get; set; } = default!;
        public SolidColorBrush ForeColor { get; set; } = default!;
        public HorizontalAlignment Alignment { get; set; } = default!;
    }

    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<UIMessage> Messages { get; set; } = new();
        private SafeTroubleshooter _troubleshooter = new SafeTroubleshooter();
        private static readonly HttpClient _httpClient = new HttpClient();
        private bool _isLiveEnvironment = true; // Toggle between live and local API

        public MainWindow()
        {
            this.InitializeComponent();
            ChatHistoryList.ItemsSource = Messages;
            AddBotMessage("System Online. Connected to ApexGPT Cloud Core.");
        }

        // --- 1. SIDEBAR & NAVIGATION ---

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            Messages.Clear();
            AddBotMessage("System Online. Connected to ApexGPT Cloud Core. New session started.");
        }

        private async void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            const string userId = "user_0001";

            if (sender is Button btn) btn.IsEnabled = false;
            LoadingSpinner.Visibility = Visibility.Visible;

            Messages.Clear();
            AddBotMessage($"Fetching ticket history for User {userId}...");

            await FetchTicketHistory(userId);

            LoadingSpinner.Visibility = Visibility.Collapsed;
            if (sender is Button finalBtn) finalBtn.IsEnabled = true;
        }


        // --- 2. CORE CHAT LOGIC ---

        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessUserMessage();

        private void InputBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) ProcessUserMessage();
        }

        private async void ProcessUserMessage()
        {
            string text = InputBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            // FIX: Restored user message color to RoyalBlue for blue theme
            Messages.Add(new UIMessage
            {
                Text = text,
                Color = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue),
                ForeColor = new SolidColorBrush(Microsoft.UI.Colors.White),
                Alignment = HorizontalAlignment.Right
            });
            InputBox.Text = "";

            LoadingSpinner.Visibility = Visibility.Visible;
            SendButton.Visibility = Visibility.Collapsed;
            MicButton.Visibility = Visibility.Collapsed;

            await CallBotApi(text);

            LoadingSpinner.Visibility = Visibility.Collapsed;
            SendButton.Visibility = Visibility.Visible;
            MicButton.Visibility = Visibility.Visible;
        }

        private async Task CallBotApi(string userText)
        {
            try
            {
                var payload = new { Prompt = userText, UserId = "user_0001" };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string botUrl = _isLiveEnvironment ? "https://apexgpt-hackathon-bot-bxehhrc3ajcuanf5.westcentralus-01.azurewebsites.net/api/ai/ask" : "http://localhost:5142/api/ai/ask";

                HttpResponseMessage response = await _httpClient.PostAsync(botUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(responseString);
                    string botReply = doc.RootElement.GetProperty("response").GetString() ?? "No Response";

                    if (botReply.Contains("COMMAND:"))
                        HandleCommand(botReply);
                    else
                        AddBotMessage(botReply);
                }
                else
                {
                    AddBotMessage($"Error: Server returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddBotMessage($"Connection Failed: {ex.Message}");
            }
        }

        // --- 3. COMMAND & HISTORY HANDLERS ---

        private void HandleCommand(string commandText)
        {
            string actionResult = "";
            string userMessage = "";
            string commandLower = commandText.ToLower(); // Use lowercased command for reliable matching

            if (commandLower.Contains("check_disk"))
            {
                userMessage = "Running disk hardware and health check...";
            }
            else if (commandLower.Contains("check_ping"))
            {
                userMessage = "Testing connectivity to external DNS (8.8.8.8)...";
            }
            else if (commandLower.Contains("flush_dns"))
            {
                userMessage = "Flushing DNS cache...";
            }
            else if (commandLower.Contains("reduce_ram"))
            {
                userMessage = "Analyzing top 5 memory consuming processes...";
            }
            else if (commandLower.Contains("clean_disk"))
            {
                userMessage = "Calculating temporary file size for cleaning recommendation...";
            }
            else if (commandLower.Contains("escalated"))
            {
                userMessage = "ESCALATION ALERT:";
            }
            // Execute the action (SafeTroubleshooter handles all known commands)
            actionResult = _troubleshooter.RunAction(commandLower.Replace("command:", "").Trim());

            AddBotMessage(userMessage);
            AddBotMessage($"RESULT:\n{actionResult}");
        }

        private async Task FetchTicketHistory(string userId)
        {
            try
            {
                string historyUrl = _isLiveEnvironment ? $"https://apexgpt-hackathon-bot-bxehhrc3ajcuanf5.westcentralus-01.azurewebsites.net/api/ai/history/{userId}" : $"http://localhost:5142/api/ai/history/{userId}";

                HttpResponseMessage response = await _httpClient.GetAsync(historyUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();

                    var history = System.Text.Json.JsonSerializer.Deserialize<List<TicketHistoryItem>>(
                        responseString,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (history != null && history.Any())
                    {
                        Messages.Add(new UIMessage
                        {
                            Text = $"🎫 Found {history.Count} tickets (Highest Priority First):",
                            Alignment = HorizontalAlignment.Left,
                            Color = new SolidColorBrush(Microsoft.UI.Colors.LightGray)
                        });

                        foreach (var ticket in history)
                        {
                            string logSummary = string.Join("\n * ", ticket.TroubleshootingLogs.TakeLast(3));

                            AddBotMessage($"--- TICKET {ticket.Id} ---");
                            AddBotMessage($"Status: **{ticket.Status}** (Priority {ticket.Priority})");
                            AddBotMessage($"Issue: {ticket.Title}");
                            AddBotMessage($"Last Actions:\n * {logSummary}");
                        }
                    }
                    else
                    {
                        AddBotMessage("No active or historical tickets found for this user.");
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    AddBotMessage($"Error fetching history: {response.StatusCode}. Details: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                AddBotMessage($"FATAL CONNECTION ERROR: {ex.Message}");
            }
        }

        // --- 4. HELPERS ---

        private void AddBotMessage(string text)
        {
            Messages.Add(new UIMessage
            {
                Text = text,
                Color = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)),
                ForeColor = new SolidColorBrush(Microsoft.UI.Colors.Black),
                Alignment = HorizontalAlignment.Left
            });
        }

        private async void MicButton_Click(object sender, RoutedEventArgs e)
        {
            MicButton.IsEnabled = false;
            InputBox.PlaceholderText = "Listening...";
            try
            {
                var speechRecognizer = new SpeechRecognizer();
                await speechRecognizer.CompileConstraintsAsync();
                SpeechRecognitionResult result = await speechRecognizer.RecognizeAsync();
                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    InputBox.Text = result.Text;
                }
            }
            catch
            {
                InputBox.PlaceholderText = "Mic Error";
            }
            finally
            {
                MicButton.IsEnabled = true;
                if (string.IsNullOrEmpty(InputBox.Text))
                    InputBox.PlaceholderText = "Ask ApexGPT...";
            }
        }
    }
}