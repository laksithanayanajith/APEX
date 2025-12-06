using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace ApexGPT.Bot.Models
{
    public class AppOneData
    {
        public List<User> Users { get; set; } = new List<User>();

        [JsonPropertyName("user_history")]
        public List<UserHistory> User_History { get; set; } = new List<UserHistory>();

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        [JsonPropertyName("ticket_conversations")]
        public List<object> Ticket_Conversations { get; set; } = new List<object>();

        [JsonPropertyName("knowledge_base")]
        public List<KnowledgeBaseArticle> Knowledge_Base { get; set; } = new List<KnowledgeBaseArticle>();
    }
}