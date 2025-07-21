using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using GlassPane.Models;
using GlassPane.Services;

namespace GlassPane
{
    public partial class MainWindow : Window
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private VirtualDesktopManager desktopManager;
        private HotkeyService hotkeyService;
        private ObservableCollection<DesktopAssignment> assignments;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
            InitializeDesktopManager();
            LoadAssignments();
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "GlassPane - Virtual Desktop Manager";
            trayIcon.Visible = true;

            // Create tray menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Window", null, (s, e) => ShowWindow());
            trayMenu.Items.Add("Configure Keybinds", null, (s, e) => ConfigureKeybinds());
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Start Service", null, (s, e) => StartService());
            trayMenu.Items.Add("Stop Service", null, (s, e) => StopService());
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += (s, e) => ShowWindow();
        }

        private void InitializeDesktopManager()
        {
            desktopManager = VirtualDesktopManager.Instance;
            desktopManager.AssignmentChanged += OnAssignmentChanged;
            
            hotkeyService = new HotkeyService(desktopManager);
        }

        private void LoadAssignments()
        {
            assignments = new ObservableCollection<DesktopAssignment>();
            AssignmentsList.ItemsSource = assignments;
            RefreshAssignmentsList();
        }

        private void RefreshAssignmentsList()
        {
            assignments.Clear();
            foreach (var assignment in desktopManager.GetAllAssignments())
            {
                assignments.Add(assignment);
            }
            UpdateTrayMenu();
        }

        private void UpdateTrayMenu()
        {
            // Remove existing assignment items (after separator)
            while (trayMenu.Items.Count > 6) // Updated to account for new menu items
            {
                trayMenu.Items.RemoveAt(6);
            }

            if (assignments.Count > 0)
            {
                trayMenu.Items.Add("-"); // Separator
                trayMenu.Items.Add("Assignments:", null, null).Enabled = false;

                foreach (var assignment in assignments)
                {
                    var item = trayMenu.Items.Add($"{assignment.DesktopName}: {assignment.WindowTitle}");
                    item.Tag = assignment.DesktopNumber;
                    item.Click += (s, e) => SwitchToDesktop(assignment.DesktopNumber);
                }
            }
        }

        private void OnAssignmentChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => RefreshAssignmentsList());
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void StartService()
        {
            try
            {
                desktopManager.Start();
                btnStartService.IsEnabled = false;
                btnStopService.IsEnabled = true;
                trayIcon.Text = "GlassPane - Virtual Desktop Manager (Running)";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to start service: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopService()
        {
            try
            {
                desktopManager.Stop();
                btnStartService.IsEnabled = true;
                btnStopService.IsEnabled = false;
                trayIcon.Text = "GlassPane - Virtual Desktop Manager";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to stop service: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show($"Failed to switch to desktop: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureKeybinds()
        {
            try
            {
                var configWindow = new KeybindConfigWindow();
                configWindow.Owner = this;
                
                if (configWindow.ShowDialog() == true)
                {
                    // Reload hotkey configuration
                    hotkeyService?.ReloadConfiguration();
                    System.Windows.MessageBox.Show(
                        "Keybind configuration has been updated. The new keybinds are now active.",
                        "Configuration Updated",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to open keybind configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnStartService_Click(object sender, RoutedEventArgs e)
        {
            StartService();
        }

        private void BtnStopService_Click(object sender, RoutedEventArgs e)
        {
            StopService();
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "Are you sure you want to clear all desktop assignments?", 
                "Confirm Clear All", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                desktopManager.ClearAllAssignments();
                RefreshAssignmentsList();
            }
        }

        private void BtnConfigureKeybinds_Click(object sender, RoutedEventArgs e)
        {
            ConfigureKeybinds();
        }

        private void RemoveAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is int desktopNumber)
            {
                desktopManager.RemoveAssignment(desktopNumber);
                RefreshAssignmentsList();
            }
        }

        private void ExitApplication()
        {
            trayIcon.Visible = false;
            hotkeyService?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Initialize hotkey service
            hotkeyService.Initialize(this);
            
            // Start service automatically
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StartService();
            }));
        }
    }
} 