# GlassPane - Windows 11 Virtual Desktop Manager

A powerful Windows 11 desktop management utility that allows you to assign specific applications or windows to virtual desktops and switch between them with global hotkeys.

## Features

- **Window Assignment**: Assign any focused window to a specific virtual desktop
- **Persistent App Mapping**: Pre-configure apps to automatically assign to specific desktops
- **Dynamic Desktop Creation**: Automatically creates new virtual desktops as needed
- **Customizable Global Hotkeys**: Fully customizable keyboard shortcuts for assignment and switching
- **System Tray Integration**: Minimal UI with full functionality from the system tray
- **Modern Interface**: Clean, modern WPF interface for managing assignments
- **Configuration Persistence**: Keybind settings and persistent app mappings are saved and restored between sessions

## Hotkeys

### Default Keybinds
- **Ctrl + [Number]**: Assign the currently focused window to Desktop [Number]
- **Alt + [Number]**: Switch to Desktop [Number] and focus the assigned window

### Customizing Keybinds
GlassPane supports fully customizable keybinds. You can change any keybind to any combination of:
- **Modifiers**: Ctrl, Alt, Shift, Windows key
- **Keys**: Numbers (0-9), Letters (A-Z), Function keys (F1-F12), Tab, Enter, Escape, Space, and more

**To customize keybinds:**
1. **From Main Window**: Click the "Configure Keybinds" button
2. **From System Tray**: Right-click the tray icon → "Configure Keybinds"
3. **In Configuration Window**: 
   - Click any keybind button to capture new keys
   - Press your desired key combination
   - Use "Reset to Defaults" to restore original keybinds
   - Click "Save Configuration" to apply changes

**Configuration Storage**: Keybind settings and persistent app mappings are automatically saved to `%AppData%/GlassPane/keybinds.json` and restored on startup.

**Real-time Updates**: Keybind changes take effect immediately without requiring application restart.

## Requirements

- Windows 11 (or Windows 10 with virtual desktop support)
- .NET 6.0 Runtime
- Administrator privileges (for global hotkey registration)

## Installation

1. **Download the Application**
   - Clone this repository or download the release
   - Ensure you have .NET 6.0 Runtime installed

2. **Build the Application**
   ```bash
   dotnet build --configuration Release
   ```

3. **Run the Application**
   ```bash
   dotnet run --configuration Release
   ```

## Usage

### Basic Workflow

1. **Start GlassPane**: Launch the application - it will minimize to the system tray
2. **Assign Windows**: 
   - Focus on any window (Chrome, Cursor, etc.)
   - Press `Ctrl + 1` to assign it to Desktop 1
   - Press `Ctrl + 2` to assign another window to Desktop 2
   - **Optional**: When manually assigning, you'll be prompted to save as a persistent mapping
3. **Switch Between Desktops**:
   - Press `Alt + 1` to switch to Desktop 1 and focus the assigned window
   - Press `Alt + 2` to switch to Desktop 2 and focus the assigned window

### Persistent App Mapping

**Pre-configure Apps**: Set up apps to automatically assign to specific desktops when they start:

1. **Configure Persistent Apps**:
   - From Main Window: Click "Configure Persistent Apps" button
   - From System Tray: Right-click tray icon → "Configure Persistent Apps"
   - Add mappings like: `chrome.exe` → Desktop 1, `cursor.exe` → Desktop 2

2. **Auto-assignment on Startup**: When GlassPane starts, it automatically:
   - Scans for running processes
   - Assigns apps to their configured desktops
   - Skips already assigned windows

3. **Manual Assignment Prompts**: When you manually assign an app (e.g., Ctrl+3 on Firefox):
   - System checks if it's already a persistent assignment
   - If not, prompts: "Would you like to save this assignment permanently?"
   - Choose to make it persistent for future auto-assignment

### System Tray Menu

Right-click the system tray icon to access:
- **Show Window**: Open the main application window
- **Configure Keybinds**: Open the keybind configuration window
- **Configure Persistent Apps**: Open the persistent app configuration window
- **Start/Stop Service**: Control the hotkey service
- **Assignments**: Quick access to switch between assigned desktops
- **Exit**: Close the application

### Main Window

The main window provides:
- **Service Controls**: Start/stop the hotkey service
- **Configure Keybinds**: Access the keybind configuration window
- **Configure Persistent Apps**: Access the persistent app configuration window
- **Assignment List**: View all current desktop assignments
- **Management**: Remove individual assignments or clear all
- **Status**: See which desktops have assigned windows

## Technical Details

### Architecture

- **WPF Application**: Modern UI built with Windows Presentation Foundation
- **Global Hotkeys**: Uses Windows API `RegisterHotKey` for system-wide shortcuts
- **Virtual Desktop Management**: PowerShell integration for Windows 11 virtual desktop operations
- **System Tray**: Windows Forms integration for tray icon functionality

### Key Components

- `VirtualDesktopManager`: Main service for desktop operations
- `Windows11VirtualDesktopManager`: PowerShell-based desktop management with async support
- `HotkeyService`: Global hotkey registration and handling with customizable keybinds
- `PersistentAppService`: Management of persistent app-to-desktop mappings
- `ConfigurationService`: JSON-based configuration persistence
- `PowerShellHelper`: Optimized PowerShell command execution with caching
- `ErrorHandler`: Centralized error handling and notification system
- `UIHelper`: Common UI operations and message dialogs
- `KeybindConfigWindow`: WPF UI for keybind customization
- `PersistentAppConfigWindow`: WPF UI for persistent app configuration
- `MainWindow`: WPF UI for assignment management

### Windows API Integration

The application uses several Windows APIs:
- Virtual Desktop COM interfaces
- Global hotkey registration
- Window management and focus
- Process information retrieval
- Window enumeration and foreground detection

### Performance Optimizations

GlassPane includes several performance optimizations:
- **Cached PowerShell operations**: Desktop count is cached for 5 seconds to reduce overhead
- **Async desktop switching**: Non-blocking operations prevent UI freezing
- **Optimized window management**: Efficient API calls for faster window operations
- **Minimal delays**: Reduced artificial delays from 1000ms to 50ms for snappy switching
- **Hidden PowerShell processes**: Background execution without visible windows
- **Real-time keybind updates**: Configuration changes apply instantly without restart

## Troubleshooting

### Common Issues

1. **Hotkeys Not Working**
   - Ensure the application is running with administrator privileges
   - Check if other applications are using the same hotkeys
   - Verify your custom keybinds don't conflict with other applications
   - Restart the service from the main window

2. **Keybind Configuration Issues**
   - If keybinds aren't saving, check write permissions to `%AppData%/GlassPane/`
   - Use "Reset to Defaults" if custom keybinds cause problems
   - Ensure no duplicate keybind combinations are configured
   - Keybind changes apply immediately - no restart required
   - Check for conflicts with other applications using the same key combinations

3. **Virtual Desktop Operations Failing**
   - Verify you're running Windows 11 or Windows 10 with virtual desktop support
   - Ensure PowerShell execution policy allows script execution
   - Check Windows permissions for virtual desktop management

4. **Application Won't Start**
   - Verify .NET 6.0 Runtime is installed
   - Check Windows Defender or antivirus software
   - Run as administrator if needed

### Debug Information

The application logs debug information to the console. Common messages:
- `GlassPane: Assigned window to Desktop X`
- `GlassPane: Failed to assign window: [error message]`
- `GlassPane: Failed to switch to desktop: [error message]`
- `GlassPane Error: [detailed error messages]`

**Performance Notes**: The application uses async operations and caching to provide
snappy desktop switching. PowerShell operations are cached for 5 seconds to reduce
overhead, and desktop switching uses non-blocking operations to prevent UI freezing.
Keybind configuration changes are applied in real-time without requiring application restart.

## Development

### Building from Source

1. **Prerequisites**
   - Visual Studio 2022 or .NET 6.0 SDK
   - Windows 11 development environment

2. **Clone and Build**
   ```bash
   git clone https://github.com/yourusername/GlassPane.git
   cd GlassPane
   dotnet restore
   dotnet build
   ```

3. **Run in Development**
   ```bash
   dotnet run
   ```

### Project Structure

```
GlassPane/
├── Models/
│   ├── DesktopAssignment.cs
│   ├── KeybindConfiguration.cs
│   └── PersistentAppAssignment.cs
├── Native/
│   ├── WindowsAPI.cs
│   └── ProcessAPI.cs
├── Services/
│   ├── VirtualDesktopManager.cs
│   ├── Windows11VirtualDesktopManager.cs
│   ├── HotkeyService.cs
│   ├── PersistentAppService.cs
│   ├── ConfigurationService.cs
│   ├── PowerShellHelper.cs
│   ├── ErrorHandler.cs
│   └── UIHelper.cs
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── KeybindConfigWindow.xaml
├── KeybindConfigWindow.xaml.cs
├── PersistentAppConfigWindow.xaml
├── PersistentAppConfigWindow.xaml.cs
└── GlassPane.csproj
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Windows 11 Virtual Desktop API documentation
- PowerShell Virtual Desktop cmdlets
- WPF and Windows Forms integration examples

## Support

For issues, feature requests, or questions:
- Create an issue on GitHub
- Check the troubleshooting section
- Review the debug output for error messages

---

**Note**: This application requires Windows 11 or Windows 10 with virtual desktop support. Some features may not work on older Windows versions. 