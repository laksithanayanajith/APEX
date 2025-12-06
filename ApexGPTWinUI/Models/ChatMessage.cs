using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexGPTWinUI.Models
{
    public class ChatMessage
    {
        public string Timestamp { get; set; } = string.Empty;
        public string Sender_Type { get; set; } = string.Empty;
        public string Sender_Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
