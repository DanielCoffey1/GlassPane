using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GlassPane.Models;
using GlassPane.Native;

namespace GlassPane.Services
{
    public class Windows11VirtualDesktopManager : IDisposable
    {
        private Dictionary<int, DesktopAssignment> assignments;
        private bool isDisposed;

        public event EventHandler AssignmentChanged;

        public Windows11VirtualDesktopManager()
        {
            assignments = new Dictionary<int, DesktopAssignment>();
        }

        public void Start()
        {
            // Service is always running
        }

        public void Stop()
        {
            // Cleanup if needed
        }

        public void AssignWindowToDesktop(int desktopNumber)
        {
            try
            {
                IntPtr foregroundWindow = WindowsAPI.GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                {
                    throw new InvalidOperationException("No foreground window found");
                }

                // Get window information
                StringBuilder windowTitle = new StringBuilder(256);
                WindowsAPI.GetWindowText(foregroundWindow, windowTitle, windowTitle.Capacity);

                uint processId;
                WindowsAPI.GetWindowThreadProcessId(foregroundWindow, out processId);

                string processName = ProcessAPI.GetProcessName(processId);

                // Create desktop if it doesn't exist
                EnsureDesktopExists(desktopNumber);

                // Store the assignment
                var assignment = new DesktopAssignment(desktopNumber, foregroundWindow, windowTitle.ToString(), processName);
                assignments[desktopNumber] = assignment;

                OnAssignmentChanged();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to assign window to desktop {desktopNumber}", ex);
            }
        }

        public void SwitchToDesktop(int desktopNumber)
        {
            try
            {
                if (!assignments.ContainsKey(desktopNumber))
                {
                    throw new InvalidOperationException($"No assignment found for desktop {desktopNumber}");
                }

                var assignment = assignments[desktopNumber];

                // Switch to the desktop using PowerShell
                SwitchToDesktopByNumber(desktopNumber);

                // Focus and maximize the assigned window
                Task.Delay(1000).ContinueWith(_ =>
                {
                    FocusAndMaximizeWindow(assignment.WindowHandle);
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to switch to desktop {desktopNumber}", ex);
            }
        }

        public void RemoveAssignment(int desktopNumber)
        {
            if (assignments.ContainsKey(desktopNumber))
            {
                assignments.Remove(desktopNumber);
                OnAssignmentChanged();
            }
        }

        public void ClearAllAssignments()
        {
            assignments.Clear();
            OnAssignmentChanged();
        }

        public IEnumerable<DesktopAssignment> GetAllAssignments()
        {
            return assignments.Values.OrderBy(a => a.DesktopNumber);
        }

        private void EnsureDesktopExists(int desktopNumber)
        {
            try
            {
                // Get current desktop count
                int currentCount = GetCurrentDesktopCount();

                // Create additional desktops if needed
                while (currentCount < desktopNumber)
                {
                    CreateNewDesktop();
                    currentCount++;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to ensure desktop {desktopNumber} exists", ex);
            }
        }

        private int GetCurrentDesktopCount()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"(Get-VirtualDesktop).Count\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (int.TryParse(output.Trim(), out int count))
                    {
                        return count;
                    }
                }
            }
            catch
            {
                // If PowerShell command fails, assume at least 1 desktop exists
            }

            return 1;
        }

        private void CreateNewDesktop()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"New-VirtualDesktop\"",
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
                throw new InvalidOperationException("Failed to create new virtual desktop", ex);
            }
        }

        private void SwitchToDesktopByNumber(int desktopNumber)
        {
            try
            {
                // Convert to 0-based index
                int desktopIndex = desktopNumber - 1;

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"$desktops = Get-VirtualDesktop; if ($desktops.Count -gt {desktopIndex}) {{ $desktops[{desktopIndex}] | Switch-VirtualDesktop }}\"",
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
                throw new InvalidOperationException($"Failed to switch to desktop {desktopNumber}", ex);
            }
        }

        private void FocusAndMaximizeWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero && WindowsAPI.IsWindowVisible(windowHandle))
            {
                WindowsAPI.SetForegroundWindow(windowHandle);
                
                if (WindowsAPI.IsIconic(windowHandle))
                {
                    WindowsAPI.ShowWindow(windowHandle, WindowsAPI.SW_RESTORE);
                }
                
                WindowsAPI.ShowWindow(windowHandle, WindowsAPI.SW_MAXIMIZE);
            }
        }



        private void OnAssignmentChanged()
        {
            AssignmentChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                assignments.Clear();
                isDisposed = true;
            }
        }
    }
} 