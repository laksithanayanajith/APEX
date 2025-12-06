using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexGPTWinUI.Models
{
    public class KnowledgeBaseArticle
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Issue_Pattern { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Resolution_Steps { get; set; } = default!;
    }
}
