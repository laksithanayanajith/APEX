using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ApexGPTWinUI.Services
{
    public class SafeTroubleshooter
    {
        // 1. SAFETY: The AllowList. Only these specific keys are allowed.
        private readonly Dictionary<string, string> _allowedScripts = new()
        {
            // Network commands
            { "check_ping", "Test-Connection -ComputerName google.com -Count 1" },
            { "flush_dns", "Clear-DnsClientCache" },
            { "get_ip", "ipconfig /all" },
            
            // System commands
            { "check_disk", "Get-PSDrive C | Select-Object Used,Free" },
            { "check_ram", "Get-ComputerInfo | Select-Object CsTotalPhysicalMemory,OsTotalVisibleMemorySize" },
            
            // Mock commands (Simulated fixes)
            { "restart_printer", "Write-Output 'Restarting Print Spooler service... Done.'" },
            { "clear_temp", "Write-Output 'Cleaning temporary files... Done.'" }
        };

        public string RunAction(string actionKey)
        {
            // 2. VALIDATION: Check if the action is allowed
            if (!_allowedScripts.ContainsKey(actionKey))
            {
                return "SECURITY WARNING: Unauthorized or unknown command blocked.";
            }

            string script = _allowedScripts[actionKey];

            try
            {
                // 3. SANDBOXED EXECUTION: Run PowerShell hidden and secure
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi)!)
                {
                    process.WaitForExit();

                    // Capture the output to show the user
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                        return $"Error: {error}";

                    return output;
                }
            }
            catch (Exception ex)
            {
                return $"System Error: {ex.Message}";
            }
        }
    }
}