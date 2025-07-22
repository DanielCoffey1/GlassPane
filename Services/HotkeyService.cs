using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using GlassPane.Native;
using GlassPane.Models;
using GlassPane.Services;

namespace GlassPane.Services
{
    public class HotkeyService : IDisposable
    {
        private HwndSource hwndSource;
        private Dictionary<int, Action> hotkeyActions;
        private Dictionary<int, int> hotkeyIds; // Maps desktop number to hotkey ID
        private VirtualDesktopManager desktopManager;
        private KeybindConfiguration keybindConfig;
        private PersistentAppService persistentAppService;
        private bool isDisposed;

        public HotkeyService(VirtualDesktopManager desktopManager)
        {
            this.desktopManager = desktopManager;
            hotkeyActions = new Dictionary<int, Action>();
            hotkeyIds = new Dictionary<int, int>();
            persistentAppService = new PersistentAppService(desktopManager);
            LoadConfiguration();
        }

        public void Initialize(Window window)
        {
            hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            hwndSource.AddHook(WndProc);
            RegisterHotkeys();
        }

        private void LoadConfiguration()
        {
            keybindConfig = ConfigurationService.Instance.LoadConfiguration();
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
            persistentAppService.ReloadConfiguration();
            if (hwndSource != null)
            {
                UnregisterAllHotkeys();
                RegisterHotkeys();
            }
        }

        private void RegisterHotkeys()
        {
            int hotkeyIdCounter = 1;

            // Register assignment keybinds
            hotkeyIdCounter = RegisterKeybindSet(keybindConfig.AssignmentKeybinds, AssignWindowToDesktop, hotkeyIdCounter);

            // Register switch keybinds
            hotkeyIdCounter = RegisterKeybindSet(keybindConfig.SwitchKeybinds, SwitchToDesktop, hotkeyIdCounter);
        }

        private int RegisterKeybindSet(Dictionary<int, KeybindInfo> keybinds, Action<int> action, int startId)
        {
            int hotkeyId = startId;

            foreach (var kvp in keybinds)
            {
                int desktopNumber = kvp.Key;
                var keybind = kvp.Value;
                
                uint modifiers = ConvertModifiers(keybind.Modifiers);
                uint virtualKey = (uint)keybind.Key;

                if (WindowsAPI.RegisterHotKey(hwndSource.Handle, hotkeyId, modifiers, virtualKey))
                {
                    hotkeyActions[hotkeyId] = () => action(desktopNumber);
                    hotkeyIds[desktopNumber] = hotkeyId;
                    hotkeyId++;
                }
            }

            return hotkeyId;
        }

        private uint ConvertModifiers(ModifierKeys modifiers)
        {
            uint result = 0;
            if ((modifiers & ModifierKeys.Alt) != 0) result |= WindowsAPI.MOD_ALT;
            if ((modifiers & ModifierKeys.Control) != 0) result |= WindowsAPI.MOD_CONTROL;
            if ((modifiers & ModifierKeys.Shift) != 0) result |= WindowsAPI.MOD_SHIFT;
            if ((modifiers & ModifierKeys.Windows) != 0) result |= WindowsAPI.MOD_WIN;
            return result;
        }

        private void UnregisterAllHotkeys()
        {
            foreach (int hotkeyId in hotkeyActions.Keys)
            {
                WindowsAPI.UnregisterHotKey(hwndSource.Handle, hotkeyId);
            }
            hotkeyActions.Clear();
            hotkeyIds.Clear();
        }

        private async void AssignWindowToDesktop(int desktopNumber)
        {
            try
            {
                // Get the currently active window info
                var activeWindow = ProcessAPI.GetForegroundWindowInfo();
                if (activeWindow != null)
                {
                    var processName = activeWindow.ProcessName;
                    
                    // Check if this app already has a persistent assignment
                    if (!persistentAppService.HasPersistentAssignment(processName))
                    {
                        // Prompt user to save as persistent assignment
                        await persistentAppService.PromptForPersistentAssignmentAsync(processName, desktopNumber);
                    }
                }

                desktopManager.AssignWindowToDesktop(desktopNumber);
                ErrorHandler.ShowNotification($"Assigned window to Desktop {desktopNumber}");
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowNotification($"Failed to assign window: {ex.Message}", true);
            }
        }

        private void SwitchToDesktop(int desktopNumber)
        {
            try
            {
                // Use async version for better performance, but don't await to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await desktopManager.SwitchToDesktopAsync(desktopNumber);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.ShowNotification($"Failed to switch to desktop: {ex.Message}", true);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowNotification($"Failed to switch to desktop: {ex.Message}", true);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsAPI.WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (hotkeyActions.ContainsKey(hotkeyId))
                {
                    hotkeyActions[hotkeyId]?.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (hwndSource != null)
                {
                    UnregisterAllHotkeys();
                    hwndSource.RemoveHook(WndProc);
                    hwndSource.Dispose();
                    hwndSource = null;
                }

                hotkeyActions.Clear();
                hotkeyIds.Clear();
                persistentAppService = null;
                isDisposed = true;
            }
        }
    }
} 