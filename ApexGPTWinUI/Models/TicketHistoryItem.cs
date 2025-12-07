using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApexGPTWinUI.Models
{
    // This class mirrors the data types returned by the Bot's TicketService
    public class TicketHistoryItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        public string Urgency { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string User_Id { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string Created_At { get; set; } = string.Empty;

        [JsonPropertyName("resolved_at")]
        public string Resolved_At { get; set; } = string.Empty;

        [JsonPropertyName("is_resolved")]
        public bool Is_Resolved { get; set; }

        public List<string> TroubleshootingLogs { get; set; } = new List<string>();
    }
}