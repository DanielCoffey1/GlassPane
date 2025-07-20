using System;

namespace GlassPane.Models
{
    public class DesktopAssignment
    {
        public int DesktopNumber { get; set; }
        public string DesktopName { get; set; }
        public IntPtr WindowHandle { get; set; }
        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        public DateTime AssignedAt { get; set; }

        public DesktopAssignment()
        {
            AssignedAt = DateTime.Now;
        }

        public DesktopAssignment(int desktopNumber, IntPtr windowHandle, string windowTitle, string processName)
        {
            DesktopNumber = desktopNumber;
            DesktopName = $"Desktop {desktopNumber}";
            WindowHandle = windowHandle;
            WindowTitle = windowTitle;
            ProcessName = processName;
            AssignedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{DesktopName}: {WindowTitle} ({ProcessName})";
        }
    }
} 