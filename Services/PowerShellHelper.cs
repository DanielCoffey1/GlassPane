using System;
using System.Diagnostics;

namespace GlassPane.Services
{
    public static class PowerShellHelper
    {
        public static string ExecuteCommand(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output.Trim();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell command: {command}", ex);
            }
        }

        public static void ExecuteCommandNoOutput(string command)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell command: {command}", ex);
            }
        }

        public static int GetCurrentDesktopCount()
        {
            try
            {
                string output = ExecuteCommand("(Get-VirtualDesktop).Count");
                if (int.TryParse(output, out int count))
                {
                    return count;
                }
            }
            catch
            {
                // If PowerShell command fails, assume at least 1 desktop exists
            }

            return 1;
        }

        public static void CreateNewDesktop()
        {
            ExecuteCommandNoOutput("New-VirtualDesktop");
        }

        public static void SwitchToDesktop(int desktopNumber)
        {
            // Convert to 0-based index
            int desktopIndex = desktopNumber - 1;
            ExecuteCommandNoOutput($"$desktops = Get-VirtualDesktop; if ($desktops.Count -gt {desktopIndex}) {{ $desktops[{desktopIndex}] | Switch-VirtualDesktop }}");
        }
    }
} 