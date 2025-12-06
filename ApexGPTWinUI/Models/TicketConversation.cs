using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;

namespace ApexGPTWinUI.Models
{
    public class TicketConversation
    {
        public string Ticket_Id { get; set; } = string.Empty;
        public List<ChatMessage> Conversation { get; set; } = default!;
    }
}
