using System.Collections.Generic;

namespace GlassPane.Models
{
    public class KeybindConfiguration
    {
        public Dictionary<int, KeybindInfo> AssignmentKeybinds { get; set; } = new Dictionary<int, KeybindInfo>();
        public Dictionary<int, KeybindInfo> SwitchKeybinds { get; set; } = new Dictionary<int, KeybindInfo>();
        public List<PersistentAppAssignment> PersistentAppAssignments { get; set; } = new List<PersistentAppAssignment>();

        public KeybindConfiguration()
        {
            InitializeDefaultKeybinds();
        }

        private void InitializeDefaultKeybinds()
        {
            // Initialize with default keybinds
            for (int i = 1; i <= 9; i++)
            {
                AssignmentKeybinds[i] = CreateDefaultAssignmentKeybind(i);
                SwitchKeybinds[i] = CreateDefaultSwitchKeybind(i);
            }
        }

        public static KeybindInfo CreateDefaultAssignmentKeybind(int desktopNumber)
        {
            return new KeybindInfo
            {
                Modifiers = ModifierKeys.Control,
                Key = (VirtualKey)('0' + desktopNumber),
                Description = $"Assign to Desktop {desktopNumber}"
            };
        }

        public static KeybindInfo CreateDefaultSwitchKeybind(int desktopNumber)
        {
            return new KeybindInfo
            {
                Modifiers = ModifierKeys.Alt,
                Key = (VirtualKey)('0' + desktopNumber),
                Description = $"Switch to Desktop {desktopNumber}"
            };
        }

        public void EnsureAllKeybindsExist()
        {
            for (int i = 1; i <= 9; i++)
            {
                if (!AssignmentKeybinds.ContainsKey(i))
                {
                    AssignmentKeybinds[i] = CreateDefaultAssignmentKeybind(i);
                }

                if (!SwitchKeybinds.ContainsKey(i))
                {
                    SwitchKeybinds[i] = CreateDefaultSwitchKeybind(i);
                }
            }
        }
    }

    public class KeybindInfo
    {
        public ModifierKeys Modifiers { get; set; }
        public VirtualKey Key { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            var modifiers = new List<string>();
            
            if ((Modifiers & ModifierKeys.Control) != 0) modifiers.Add("Ctrl");
            if ((Modifiers & ModifierKeys.Alt) != 0) modifiers.Add("Alt");
            if ((Modifiers & ModifierKeys.Shift) != 0) modifiers.Add("Shift");
            if ((Modifiers & ModifierKeys.Windows) != 0) modifiers.Add("Win");

            var keyName = Key.ToString();
            if (keyName.StartsWith("D") && keyName.Length == 2 && char.IsDigit(keyName[1]))
            {
                keyName = keyName[1].ToString();
            }

            return $"{string.Join("+", modifiers)}+{keyName}";
        }
    }

    [System.Flags]
    public enum ModifierKeys
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public enum VirtualKey
    {
        // Number keys
        D0 = 0x30, D1 = 0x31, D2 = 0x32, D3 = 0x33, D4 = 0x34,
        D5 = 0x35, D6 = 0x36, D7 = 0x37, D8 = 0x38, D9 = 0x39,
        
        // Letter keys
        A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45,
        F = 0x46, G = 0x47, H = 0x48, I = 0x49, J = 0x4A,
        K = 0x4B, L = 0x4C, M = 0x4D, N = 0x4E, O = 0x4F,
        P = 0x50, Q = 0x51, R = 0x52, S = 0x53, T = 0x54,
        U = 0x55, V = 0x56, W = 0x57, X = 0x58, Y = 0x59, Z = 0x5A,
        
        // Function keys
        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74,
        F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79,
        F11 = 0x7A, F12 = 0x7B,
        
        // Other keys
        Tab = 0x09, Enter = 0x0D, Escape = 0x1B, Space = 0x20,
        Insert = 0x2D, Delete = 0x2E, Home = 0x24, End = 0x23,
        PageUp = 0x21, PageDown = 0x22
    }
} 