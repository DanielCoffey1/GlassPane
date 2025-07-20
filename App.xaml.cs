using System.Windows;
using GlassPane.Services;

namespace GlassPane
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup global hotkeys and virtual desktop manager
            VirtualDesktopManager.Instance?.Dispose();
            base.OnExit(e);
        }
    }
} 