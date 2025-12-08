using Microsoft.EntityFrameworkCore;
using ApexGPT.Bot.Models;

namespace ApexGPT.Bot.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<KnowledgeBaseArticle> KnowledgeBase { get; set; }
    }
}