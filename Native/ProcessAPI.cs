using System;
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
    }
} 