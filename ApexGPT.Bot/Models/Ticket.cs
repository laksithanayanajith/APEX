using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApexGPT.Bot.Models
{
    public class Ticket
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";

        public int Priority { get; set; } // Must be int to match your JSON

        public string Urgency { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string User_Id { get; set; } = string.Empty;

        [JsonPropertyName("is_resolved")]
        public bool Is_Resolved { get; set; } = default!;

        [JsonPropertyName("created_at")]
        public string Created_At { get; set; } = string.Empty;

        [JsonPropertyName("resolved_at")]
        public string Resolved_At { get; set; } = string.Empty;

        // New field for your Hackathon requirement
        public List<string> TroubleshootingLogs { get; set; } = new List<string>();
    }
}