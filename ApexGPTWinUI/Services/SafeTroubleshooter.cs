using System;
using System.Diagnostics;
using System.Linq; // Added for Linq used in some PowerShell commands

namespace ApexGPTWinUI.Services
{
    /// <summary>
    /// Executes local, elevated OS commands based on commands received from the AI.
    /// This file requires the WinUI application to be run as an Administrator.
    /// </summary>
    public class SafeTroubleshooter
    {
        public string RunAction(string actionName)
        {
            // Map the command name to the actual process arguments
            (string fileName, string arguments) command = actionName.ToLower() switch
            {
                "check_disk" => ("cmd.exe", "/C wmic diskdrive get model,size,status"), // Check Disk Status
                "check_ping" => ("cmd.exe", "/C ping -n 4 8.8.8.8"), // Check Network Connectivity
                "flush_dns" => ("ipconfig", "/flushdns"), // Flush DNS Cache
                "reduce_ram" => ("powershell.exe", "Get-Process | Sort-Object WS -Descending | Select-Object -First 5 Name, @{Name='WorkingSetMB';Expression={$_.WS / 1MB -as [int]}} | Format-List"),
                "clean_disk" => ("powershell.exe", "$size = (Get-ChildItem -Path $env:TEMP -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum; Write-Host \"Total Temp Files Size: $($size / 1MB -as [int]) MB. Consider running Disk Cleanup.\";"),

                _ => ("", "")
            };

            if (string.IsNullOrEmpty(command.fileName)) return $"ERROR: Unknown command '{actionName}'.";

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = command.fileName;
                process.StartInfo.Arguments = command.arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true; // Prevents the CMD window from flashing up

                // This action requires the main application to be running as admin
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return $"SUCCESS: Command executed. Output:\n{output}";
                }
                else if (process.ExitCode == 5 || process.ExitCode == 1003) // Access Denied or other privilege issue
                {
                    return "FAILURE: ACCESS DENIED. The command failed. Ensure the application is running as an Administrator.";
                }
                else
                {
                    return $"FAILURE: Command returned exit code {process.ExitCode}. Output:\n{output}";
                }
            }
            catch (Exception ex)
            {
                return $"EXECUTION ERROR: Failed to start process. Details: {ex.Message}";
            }
        }
    }
}