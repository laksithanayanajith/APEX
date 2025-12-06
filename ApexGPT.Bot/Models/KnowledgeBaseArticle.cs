using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApexGPT.Bot.Models
{
    public class KnowledgeBaseArticle
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("issue_pattern")]
        public string Issue_Pattern { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("resolution_steps")]
        public List<string> Resolution_Steps { get; set; } = new List<string>();
    }
}