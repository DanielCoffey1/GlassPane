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

                // Focus and maximize the assigned window with minimal delay
                Task.Delay(50).ContinueWith(_ =>
                {
                    FocusAndMaximizeWindow(assignment.WindowHandle);
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to switch to desktop {desktopNumber}", ex);
            }
        }

        public async Task SwitchToDesktopAsync(int desktopNumber)
        {
            try
            {
                if (!assignments.ContainsKey(desktopNumber))
                {
                    throw new InvalidOperationException($"No assignment found for desktop {desktopNumber}");
                }

                var assignment = assignments[desktopNumber];

                // Switch to the desktop using PowerShell asynchronously
                await SwitchToDesktopByNumberAsync(desktopNumber);

                // Focus and maximize the assigned window with minimal delay
                await Task.Delay(50);
                FocusAndMaximizeWindow(assignment.WindowHandle);
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
            return PowerShellHelper.GetCurrentDesktopCount();
        }

        private void CreateNewDesktop()
        {
            PowerShellHelper.CreateNewDesktop();
        }

        private void SwitchToDesktopByNumber(int desktopNumber)
        {
            PowerShellHelper.SwitchToDesktop(desktopNumber);
        }

        private async Task SwitchToDesktopByNumberAsync(int desktopNumber)
        {
            await PowerShellHelper.SwitchToDesktopAsync(desktopNumber);
        }

        private void FocusAndMaximizeWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero && WindowsAPI.IsWindowVisible(windowHandle))
            {
                // Use SetWindowPos for more efficient window operations
                WindowsAPI.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, 
                    WindowsAPI.SWP_NOMOVE | WindowsAPI.SWP_NOSIZE | WindowsAPI.SWP_SHOWWINDOW);
                
                WindowsAPI.SetForegroundWindow(windowHandle);
                
                // Only restore if minimized, then maximize
                if (WindowsAPI.IsIconic(windowHandle))
                {
                    WindowsAPI.ShowWindow(windowHandle, WindowsAPI.SW_RESTORE);
                }
                
                // Only maximize if not already maximized
                if (!WindowsAPI.IsZoomed(windowHandle))
                {
                    WindowsAPI.ShowWindow(windowHandle, WindowsAPI.SW_MAXIMIZE);
                }
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