using ApexGPT.Bot.Models; // Ensure this matches your namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ApexGPT.Bot.Services
{
    public class TicketService
    {
        private AppOneData _database;
        private readonly string _filePath;

        public TicketService()
        {
            // 1. Locate the JSON file
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ticketing_system_data_new.json");
            LoadData();
        }

        // --- CORE FUNCTIONALITY ---

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
                Urgency = "Medium",
                Created_At = DateTime.UtcNow.ToString("o"),
                Is_Resolved = false,
                TroubleshootingLogs = new List<string>
                {
                    $"[{DateTime.Now:HH:mm}] Ticket Created via ApexGPT Chat."
                }
            };

            _database.Tickets.Add(newTicket);
            SaveChanges();
            return newTicket;
        }

        // 2. Log Action
        public void LogAction(string ticketId, string actionDescription)
        {
            var ticket = _database.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                if (ticket.Status == "Open") ticket.Status = "In-Progress";

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] ACTION: {actionDescription}");

                SaveChanges();
            }
        }

        // 3. Resolve Ticket
        public void ResolveTicket(string ticketId, string resolutionNotes)
        {
            var ticket = _database.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                ticket.Status = "Resolved";
                ticket.Is_Resolved = true;
                ticket.Resolved_At = DateTime.UtcNow.ToString("o");

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] RESOLVED: {resolutionNotes}");

                SaveChanges();
            }
        }

        // 4. Escalate Ticket (For the "Escalate" Requirement)
        public void EscalateTicket(string ticketId, string reason)
        {
            var ticket = _database.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                ticket.Status = "Escalated";
                ticket.Priority = 1; // High Priority

                if (ticket.TroubleshootingLogs == null) ticket.TroubleshootingLogs = new List<string>();
                ticket.TroubleshootingLogs.Add($"[{DateTime.Now:HH:mm}] ESCALATED: {reason}");

                SaveChanges();
            }
        }

        // 5. Get Active Ticket (Now sorted by Priority)
        public Ticket? GetActiveTicket(string userId)
        {
            // ✅ CHANGE: Sorts by Priority (1=High) and then by creation time
            return _database.Tickets
                .Where(t => t.User_Id == userId && !t.Is_Resolved)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.Created_At)
                .FirstOrDefault();
        }

        // 6. Get Ticket History (Now sorted by Priority)
        public List<Ticket> GetTicketsByUserId(string userId)
        {
            // ✅ CHANGE: Sorts by Priority (1=High) then by creation time
            return _database.Tickets
                .Where(t => t.User_Id == userId)
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.Created_At)
                .ToList() ?? new List<Ticket>();
        }

        // 7. Get KB Articles
        public List<KnowledgeBaseArticle> GetAllArticles()
        {
            return _database.Knowledge_Base ?? new List<KnowledgeBaseArticle>();
        }

        // --- DATABASE MANAGEMENT ---

        private void LoadData()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _database = JsonSerializer.Deserialize<AppOneData>(json, options) ?? new AppOneData();
                }
                catch
                {
                    _database = new AppOneData();
                }
            }
            else
            {
                _database = new AppOneData();
            }
        }

        private void SaveChanges()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_database, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving DB: {ex.Message}");
            }
        }
    }
}