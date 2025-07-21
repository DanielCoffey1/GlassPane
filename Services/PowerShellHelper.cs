using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GlassPane.Services
{
    public static class PowerShellHelper
    {
        private static int? cachedDesktopCount = null;
        private static DateTime lastCacheUpdate = DateTime.MinValue;
        private static readonly TimeSpan cacheTimeout = TimeSpan.FromSeconds(5);

        private static ProcessStartInfo CreateProcessStartInfo(string command, bool redirectOutput = false)
        {
            return new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = redirectOutput,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        public static string ExecuteCommand(string command)
        {
            try
            {
                using (var process = Process.Start(CreateProcessStartInfo(command, true)))
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
                using (var process = Process.Start(CreateProcessStartInfo(command, false)))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell command: {command}", ex);
            }
        }

        public static async Task<string> ExecuteCommandAsync(string command)
        {
            try
            {
                using (var process = Process.Start(CreateProcessStartInfo(command, true)))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    return output.Trim();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell command: {command}", ex);
            }
        }

        public static async Task ExecuteCommandNoOutputAsync(string command)
        {
            try
            {
                using (var process = Process.Start(CreateProcessStartInfo(command, false)))
                {
                    await process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute PowerShell command: {command}", ex);
            }
        }

        public static int GetCurrentDesktopCount()
        {
            // Use cached value if available and not expired
            if (cachedDesktopCount.HasValue && DateTime.Now - lastCacheUpdate < cacheTimeout)
            {
                return cachedDesktopCount.Value;
            }

            try
            {
                string output = ExecuteCommand("(Get-VirtualDesktop).Count");
                if (int.TryParse(output, out int count))
                {
                    cachedDesktopCount = count;
                    lastCacheUpdate = DateTime.Now;
                    return count;
                }
            }
            catch
            {
                // If PowerShell command fails, assume at least 1 desktop exists
            }

            cachedDesktopCount = 1;
            lastCacheUpdate = DateTime.Now;
            return 1;
        }

        public static void CreateNewDesktop()
        {
            ExecuteCommandNoOutput("New-VirtualDesktop");
            // Invalidate cache after creating new desktop
            cachedDesktopCount = null;
        }

        public static void SwitchToDesktop(int desktopNumber)
        {
            // Use more efficient command - direct switch without checking count
            int desktopIndex = desktopNumber - 1;
            ExecuteCommandNoOutput($"(Get-VirtualDesktop)[{desktopIndex}] | Switch-VirtualDesktop");
        }

        public static async Task SwitchToDesktopAsync(int desktopNumber)
        {
            // Use more efficient command - direct switch without checking count
            int desktopIndex = desktopNumber - 1;
            await ExecuteCommandNoOutputAsync($"(Get-VirtualDesktop)[{desktopIndex}] | Switch-VirtualDesktop");
        }

        public static void InvalidateCache()
        {
            cachedDesktopCount = null;
            lastCacheUpdate = DateTime.MinValue;
        }
    }
} 