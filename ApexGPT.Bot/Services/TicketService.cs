using ApexGPT.Bot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApexGPT.Bot.Services
{
    public class TicketService
    {
        // Replaced the JSON object with the Entity Framework Database Context
        private readonly AppDbContext _context;

        // Constructor now asks for the Database instead of loading a file
        public TicketService(AppDbContext context)
        {
            _context = context;
            //SeedDatabase();
        }

        // --- NEW HELPER: Seed Data ---
        // This runs automatically on startup to populate the In-Memory DB
        private void SeedDatabase()
        {
            if (!_context.KnowledgeBase.Any())
            {
                _context.KnowledgeBase.AddRange(new List<KnowledgeBaseArticle>
                {
                    new KnowledgeBaseArticle
                    {
                        Id = "KB001",
                        Title = "Internet Connection Issues",
                        Category = "Network",
                        Summary = "Steps to resolve basic connectivity problems.",
                        Resolution_Steps = new List<string> { "Check ethernet cable", "Run 'ipconfig /flushdns'", "Restart Router" }
                    },
                    new KnowledgeBaseArticle
                    {
                        Id = "KB002",
                        Title = "System Sluggish / Slow Performance",
                        Category = "Performance",
                        Summary = "Guide to optimizing system speed.",
                        Resolution_Steps = new List<string> { "Check Task Manager", "Close background apps", "Run RAM diagnostics" }
                    },
                    new KnowledgeBaseArticle
                    {
                        Id = "KB003",
                        Title = "Printer Not responding",
                        Category = "Hardware",
                        Summary = "Fixes for common printer communication errors.",
                        Resolution_Steps = new List<string> { "Check USB connection", "Restart Spooler Service", "Reinstall Drivers" }
                    }
                });
                _context.SaveChanges();
            }
        }

        // --- CORE FUNCTIONALITY (Signatures kept exactly the same) ---

        // 1. Create a new Ticket
        public Ticket CreateTicket(string userId, string description, string category)
        {
            var newTicket = new Ticket
            {
                Id = "TKT-" + DateTime.Now.Ticks.ToString().Substring(10, 5),
                User_Id = userId,
                Title = "User Reported Issue via Chat",
                Description = description,
                Category = category,
                Status = "Open",
                Priority = 2,
                Created_At = DateTime.UtcNow.ToString("o"),
                Is_Resolved = false,
                TroubleshootingLogs = new List<string> { $"[{DateTime.Now:HH:mm}] Ticket Created via ApexGPT Chat." }
            };

            _context.Tickets.Add(newTicket);
            _context.SaveChanges(); // Saves to RAM immediately
            return newTicket;
        }

        // 2. Log Action
        public void LogAction(string ticketId, string actionDescription)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                if (ticket.Status == "Open") ticket.Status = "In-Progress";

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] ACTION: {actionDescription}");

                _context.SaveChanges(); // Updates the record in RAM
            }
        }

        // 3. Resolve Ticket
        public void ResolveTicket(string ticketId, string resolutionNotes)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                ticket.Status = "Resolved";
                ticket.Is_Resolved = true;
                ticket.Resolved_At = DateTime.UtcNow.ToString("o");

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] RESOLVED: {resolutionNotes}");

                _context.SaveChanges();
            }
        }

        // 4. Escalate Ticket
        public void EscalateTicket(string ticketId, string reason)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                ticket.Status = "Escalated";
                ticket.Priority = 1; // High Priority

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] ESCALATED: {reason}");

                _context.SaveChanges();
            }
        }

        // 5. Get Active Ticket
        public Ticket? GetActiveTicket(string userId)
        {
            return _context.Tickets
                .Where(t => t.User_Id == userId && !t.Is_Resolved)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.Created_At)
                .FirstOrDefault();
        }

        // 6. Get Ticket History
        public List<Ticket> GetTicketsByUserId(string userId)
        {
            return _context.Tickets
                .Where(t => t.User_Id == userId)
                .OrderBy(t => t.Priority)
                .ThenByDescending(t => t.Created_At)
                .ToList();
        }

        // 7. Get KB Articles
        public List<KnowledgeBaseArticle> GetAllArticles()
        {
            return _context.KnowledgeBase.ToList();
        }
    }
}