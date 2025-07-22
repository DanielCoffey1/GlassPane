using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GlassPane.Native
{
    public static class ProcessAPI
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, uint nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // Process access rights
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static string GetProcessName(uint processId)
        {
            try
            {
                IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
                if (processHandle != IntPtr.Zero)
                {
                    StringBuilder fileName = new StringBuilder(260);
                    uint result = GetModuleFileNameEx(processHandle, IntPtr.Zero, fileName, (uint)fileName.Capacity);
                    CloseHandle(processHandle);

                    if (result > 0)
                    {
                        string fullPath = fileName.ToString();
                        return System.IO.Path.GetFileNameWithoutExtension(fullPath);
                    }
                }
            }
            catch
            {
                // Ignore errors and return default
            }

            return "Unknown Process";
        }

        public static WindowInfo GetForegroundWindowInfo()
        {
            try
            {
                IntPtr hWnd = GetForegroundWindow();
                if (hWnd != IntPtr.Zero)
                {
                    uint processId;
                    GetWindowThreadProcessId(hWnd, out processId);
                    
                    StringBuilder windowTitle = new StringBuilder(256);
                    GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
                    
                    string processName = GetProcessName(processId);
                    
                    return new WindowInfo
                    {
                        Handle = hWnd,
                        ProcessId = processId,
                        ProcessName = processName,
                        WindowTitle = windowTitle.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting foreground window: {ex.Message}");
            }
            
            return null;
        }

        public static List<WindowInfo> GetWindowsByProcessName(string processName)
        {
            var windows = new List<WindowInfo>();
            
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        uint processId;
                        GetWindowThreadProcessId(hWnd, out processId);
                        
                        string currentProcessName = GetProcessName(processId);
                        
                        if (string.Equals(currentProcessName, processName, StringComparison.OrdinalIgnoreCase))
                        {
                            StringBuilder windowTitle = new StringBuilder(256);
                            GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
                            
                            windows.Add(new WindowInfo
                            {
                                Handle = hWnd,
                                ProcessId = processId,
                                ProcessName = currentProcessName,
                                WindowTitle = windowTitle.ToString()
                            });
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting windows by process name: {ex.Message}");
            }
            
            return windows;
        }
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public uint ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
    }
} 