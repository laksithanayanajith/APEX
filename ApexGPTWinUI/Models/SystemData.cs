using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexGPTWinUI.Models
{
    public class SystemData
    {
        public List<User> Users { get; set; } = default!;
        public List<UserHistory> User_History { get; set; } = default!;
        public List<Ticket> Tickets { get; set; } = default!;
        public List<TicketConversation> Ticket_Conversations { get; set; } = default!;
        public List<KnowledgeBaseArticle> Knowledge_Base { get; set; } = default!;
    }
}
