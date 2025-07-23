using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GlassPane.Models;
using GlassPane.Native;

namespace GlassPane.Services
{
    public class PersistentAppService
    {
        private VirtualDesktopManager desktopManager;
        private KeybindConfiguration config;

        public PersistentAppService(VirtualDesktopManager desktopManager)
        {
            this.desktopManager = desktopManager;
            this.config = ConfigurationService.Instance.LoadConfiguration();
        }

        public void ReloadConfiguration()
        {
            config = ConfigurationService.Instance.LoadConfiguration();
        }

        public async Task AutoAssignPersistentAppsAsync()
        {
            if (config.PersistentAppAssignments == null || config.PersistentAppAssignments.Count == 0)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Auto-assigning {config.PersistentAppAssignments.Count} persistent apps...");
                
                // Get all running processes
                var runningProcesses = Process.GetProcesses();
                var processNames = runningProcesses.Select(p => p.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var assignment in config.PersistentAppAssignments)
                {
                    if (processNames.Contains(assignment.ProcessName))
                    {
                        System.Diagnostics.Debug.WriteLine($"Found running process: {assignment.ProcessName}");
                        
                        // Find windows for this process and assign them
                        var windows = ProcessAPI.GetWindowsByProcessName(assignment.ProcessName);
                        System.Diagnostics.Debug.WriteLine($"Found {windows.Count} windows for {assignment.ProcessName}");
                        
                        foreach (var window in windows)
                        {
                            try
                            {
                                // Skip windows with empty titles (usually system windows)
                                if (string.IsNullOrWhiteSpace(window.WindowTitle))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Skipping window with empty title for {assignment.ProcessName}");
                                    continue;
                                }

                                // Check if window is already assigned to a desktop
                                var existingAssignments = desktopManager.GetAllAssignments();
                                var isAlreadyAssigned = existingAssignments.Any(a => a.WindowHandle == window.Handle);

                                if (!isAlreadyAssigned)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Auto-assigning {assignment.ProcessName} ({window.WindowTitle}) to Desktop {assignment.DesktopNumber}");
                                    
                                    // Assign the window to the specified desktop
                                    desktopManager.AssignWindowToDesktop(assignment.DesktopNumber, window.Handle);
                                    
                                    // Small delay to prevent overwhelming the system
                                    await Task.Delay(100);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Window {window.WindowTitle} is already assigned");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to auto-assign {assignment.ProcessName}: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Process {assignment.ProcessName} is not running");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Auto-assignment completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AutoAssignPersistentAppsAsync: {ex.Message}");
            }
        }

        public async Task<bool> PromptForPersistentAssignmentAsync(string processName, int desktopNumber)
        {
            var result = MessageBox.Show(
                $"Would you like to save this assignment permanently?\n\n{processName} → Desktop {desktopNumber}\n\nThis will automatically assign {processName} to Desktop {desktopNumber} whenever the app starts.",
                "Save Persistent Assignment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await AddPersistentAssignmentAsync(processName, desktopNumber);
                return true;
            }

            return false;
        }

        public Task AddPersistentAssignmentAsync(string processName, int desktopNumber, string description = null)
        {
            // Remove any existing assignment for this process
            config.PersistentAppAssignments.RemoveAll(a => 
                string.Equals(a.ProcessName, processName, StringComparison.OrdinalIgnoreCase));

            // Add new assignment
            var assignment = new PersistentAppAssignment(processName, desktopNumber, description);
            config.PersistentAppAssignments.Add(assignment);

            // Save configuration
            ConfigurationService.Instance.SaveConfiguration(config);

            // Show confirmation
            ErrorHandler.ShowNotification($"Saved persistent assignment: {processName} → Desktop {desktopNumber}");
            
            return Task.CompletedTask;
        }

        public void RemovePersistentAssignment(string processName)
        {
            var removed = config.PersistentAppAssignments.RemoveAll(a => 
                string.Equals(a.ProcessName, processName, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                ConfigurationService.Instance.SaveConfiguration(config);
                ErrorHandler.ShowNotification($"Removed persistent assignment for {processName}");
            }
        }

        public List<PersistentAppAssignment> GetAllPersistentAssignments()
        {
            return config.PersistentAppAssignments?.ToList() ?? new List<PersistentAppAssignment>();
        }

        public bool HasPersistentAssignment(string processName)
        {
            return config.PersistentAppAssignments?.Any(a => 
                string.Equals(a.ProcessName, processName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        public PersistentAppAssignment GetPersistentAssignment(string processName)
        {
            return config.PersistentAppAssignments?.FirstOrDefault(a => 
                string.Equals(a.ProcessName, processName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasRunningPersistentApps()
        {
            if (config.PersistentAppAssignments == null || config.PersistentAppAssignments.Count == 0)
                return false;

            var runningProcesses = Process.GetProcesses();
            var processNames = runningProcesses.Select(p => p.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return config.PersistentAppAssignments.Any(assignment => 
                processNames.Contains(assignment.ProcessName));
        }

        public async Task ManualTriggerAutoAssignmentAsync()
        {
            System.Diagnostics.Debug.WriteLine("Manual trigger of auto-assignment requested");
            await AutoAssignPersistentAppsAsync();
        }
    }
} 