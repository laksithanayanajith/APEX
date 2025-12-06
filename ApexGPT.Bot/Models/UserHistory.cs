using System.Text.Json.Serialization;

namespace ApexGPT.Bot.Models
{
    public class UserHistory
    {
        [JsonPropertyName("user_id")]
        public string User_Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}