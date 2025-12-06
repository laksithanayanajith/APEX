using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexGPTWinUI.Models
{
    public class Ticket
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> TroubleshootingLogs { get; set; } = new List<string>();
        public string User_Id { get; set; } = string.Empty;
        public string User_Name { get; set; } = string.Empty;
        public string Created_At { get; set; } = string.Empty;
        public bool Is_Resolved { get; set; } = default!;
    }
}
