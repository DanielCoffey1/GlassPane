using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using GlassPane.Native;

namespace GlassPane.Services
{
    public class HotkeyService : IDisposable
    {
        private HwndSource hwndSource;
        private Dictionary<int, Action> hotkeyActions;
        private VirtualDesktopManager desktopManager;
        private bool isDisposed;

        public HotkeyService(VirtualDesktopManager desktopManager)
        {
            this.desktopManager = desktopManager;
            hotkeyActions = new Dictionary<int, Action>();
        }

        public void Initialize(Window window)
        {
            hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            hwndSource.AddHook(WndProc);
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            // Register Ctrl + Number keys (1-9) for assignment
            for (int i = 1; i <= 9; i++)
            {
                int hotkeyId = i;
                uint modifiers = (uint)(WindowsAPI.MOD_CONTROL);
                uint virtualKey = (uint)('0' + i);

                if (WindowsAPI.RegisterHotKey(hwndSource.Handle, hotkeyId, modifiers, virtualKey))
                {
                    int desktopNumber = i;
                    hotkeyActions[hotkeyId] = () => AssignWindowToDesktop(desktopNumber);
                }
            }

            // Register Alt + Number keys (1-9) for switching
            for (int i = 1; i <= 9; i++)
            {
                int hotkeyId = i + 100; // Use different IDs for Alt combinations
                uint modifiers = (uint)(WindowsAPI.MOD_ALT);
                uint virtualKey = (uint)('0' + i);

                if (WindowsAPI.RegisterHotKey(hwndSource.Handle, hotkeyId, modifiers, virtualKey))
                {
                    int desktopNumber = i;
                    hotkeyActions[hotkeyId] = () => SwitchToDesktop(desktopNumber);
                }
            }
        }

        private void AssignWindowToDesktop(int desktopNumber)
        {
            try
            {
                desktopManager.AssignWindowToDesktop(desktopNumber);
                ShowNotification($"Assigned window to Desktop {desktopNumber}");
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to assign window: {ex.Message}");
            }
        }

        private void SwitchToDesktop(int desktopNumber)
        {
            try
            {
                desktopManager.SwitchToDesktop(desktopNumber);
            }
            catch (Exception ex)
            {
                ShowNotification($"Failed to switch to desktop: {ex.Message}");
            }
        }

        private void ShowNotification(string message)
        {
            // In a real implementation, you might want to show a toast notification
            // For now, we'll just log to debug output
            System.Diagnostics.Debug.WriteLine($"GlassPane: {message}");
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
                    // Unregister all hotkeys
                    foreach (int hotkeyId in hotkeyActions.Keys)
                    {
                        WindowsAPI.UnregisterHotKey(hwndSource.Handle, hotkeyId);
                    }

                    hwndSource.RemoveHook(WndProc);
                    hwndSource.Dispose();
                    hwndSource = null;
                }

                hotkeyActions.Clear();
                isDisposed = true;
            }
        }
    }
} 