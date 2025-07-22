using System;

namespace GlassPane.Models
{
    public class PersistentAppAssignment
    {
        public string ProcessName { get; set; }
        public int DesktopNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }

        public PersistentAppAssignment()
        {
            CreatedAt = DateTime.Now;
        }

        public PersistentAppAssignment(string processName, int desktopNumber, string description = null)
        {
            ProcessName = processName;
            DesktopNumber = desktopNumber;
            Description = description ?? $"{processName} → Desktop {desktopNumber}";
            CreatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return Description ?? $"{ProcessName} → Desktop {DesktopNumber}";
        }
    }
} 