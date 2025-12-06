using ApexGPTWinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApexGPTWinUI.Services
{
    public class DataService
    {
        public SystemData Database { get; private set; } = new SystemData();
        public DataService()
        {
            LoadData();
        }

        private void LoadData()
        {
            // 1. Get Path
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ticketing_system_data_new.json");

            if (File.Exists(filePath))
            {
                // 2. Read File
                string jsonString = File.ReadAllText(filePath);

                // 3. Configure Options (CRITICAL for matching "user_id" to "User_Id")
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // This allows "users" to match "Users"
                };

                // 4. Deserialize
                try
                {
                    Database = JsonSerializer.Deserialize<SystemData>(jsonString, options)!;
                }
                catch (Exception ex)
                {
                    // If JSON is bad, create an empty DB to prevent crash
                    System.Diagnostics.Debug.WriteLine($"Error reading JSON: {ex.Message}");
                    Database = new SystemData();
                }
            }
            else
            {
                Database = new SystemData();
            }
        }
    }
}
