using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using ApexGPTWinUI.Services;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

        public MainWindow()
        {
            this.InitializeComponent();
            ChatHistoryList.ItemsSource = Messages;
            AddBotMessage("System Online. Connected to ApexGPT Cloud Core.");
        }

        // --- 1. SEND BUTTON ---
        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessUserMessage();

        private void InputBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) ProcessUserMessage();
        }

        // --- 2. CORE PROCESS ---
        private async void ProcessUserMessage()
        {
            string text = InputBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            // Show User Message
            Messages.Add(new UIMessage
            {
                Text = text,
                Color = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue),
                ForeColor = new SolidColorBrush(Microsoft.UI.Colors.White),
                Alignment = HorizontalAlignment.Right
            });
            InputBox.Text = "";

            // UI Loading
            LoadingSpinner.Visibility = Visibility.Visible;
            SendButton.Visibility = Visibility.Collapsed;
            MicButton.Visibility = Visibility.Collapsed;

            // API Call
            await CallBotApi(text);

            // UI Restore
            LoadingSpinner.Visibility = Visibility.Collapsed;
            SendButton.Visibility = Visibility.Visible;
            MicButton.Visibility = Visibility.Visible;
        }

        // --- 3. API CALL ---
        private async Task CallBotApi(string userText)
        {
            try
            {
                var payload = new { Prompt = userText, UserId = "user_0001" };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Ensure port matches your Bot project (e.g. 5142)
                string botUrl = "http://localhost:5142/api/ai/ask";

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

        // --- 4. HANDLER ---
        private void HandleCommand(string commandText)
        {
            string actionResult = "";
            string userMessage = "";

            if (commandText.Contains("CHECK_DISK"))
            {
                userMessage = "Running disk check...";
                actionResult = _troubleshooter.RunAction("check_disk");
            }
            else if (commandText.Contains("CHECK_PING"))
            {
                userMessage = "Testing connectivity...";
                actionResult = _troubleshooter.RunAction("check_ping");
            }
            else if (commandText.Contains("FLUSH_DNS"))
            {
                userMessage = "Flushing DNS...";
                actionResult = _troubleshooter.RunAction("flush_dns");
            }
            else if (commandText.Contains("ESCALATED"))
            {
                userMessage = "ESCALATION ALERT:";
                actionResult = commandText;
            }

            AddBotMessage(userMessage);
            AddBotMessage($"RESULT:\n{actionResult}");
        }

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

        // --- 5. MIC LOGIC (Completely separate method) ---
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