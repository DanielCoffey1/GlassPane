using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GlassPane.Models;
using GlassPane.Services;

namespace GlassPane
{
    public class KeybindButtonTag
    {
        public int DesktopNumber { get; set; }
        public bool IsAssignment { get; set; }
    }

    public partial class KeybindConfigWindow : Window
    {
        private KeybindConfiguration currentConfig;
        private KeybindConfiguration originalConfig;
        private Dictionary<int, Button> assignmentButtons = new Dictionary<int, Button>();
        private Dictionary<int, Button> switchButtons = new Dictionary<int, Button>();
        private Button currentlyCapturingButton = null;

        public KeybindConfigWindow()
        {
            InitializeComponent();
            LoadConfiguration();
            PopulateKeybinds();
            this.KeyDown += KeybindConfigWindow_KeyDown;
            this.KeyUp += KeybindConfigWindow_KeyUp;
        }

        private void LoadConfiguration()
        {
            currentConfig = ConfigurationService.Instance.LoadConfiguration();
            originalConfig = new KeybindConfiguration
            {
                AssignmentKeybinds = new Dictionary<int, KeybindInfo>(currentConfig.AssignmentKeybinds),
                SwitchKeybinds = new Dictionary<int, KeybindInfo>(currentConfig.SwitchKeybinds)
            };
        }

        private void PopulateKeybinds()
        {
            // Clear existing panels
            AssignmentKeybindsPanel.Children.Clear();
            SwitchKeybindsPanel.Children.Clear();
            assignmentButtons.Clear();
            switchButtons.Clear();

            // Create assignment keybinds
            for (int i = 1; i <= 9; i++)
            {
                var assignmentRow = CreateKeybindRow($"Desktop {i}", currentConfig.AssignmentKeybinds[i], i, true);
                AssignmentKeybindsPanel.Children.Add(assignmentRow);
            }

            // Create switch keybinds
            for (int i = 1; i <= 9; i++)
            {
                var switchRow = CreateKeybindRow($"Desktop {i}", currentConfig.SwitchKeybinds[i], i, false);
                SwitchKeybindsPanel.Children.Add(switchRow);
            }
        }

        private Border CreateKeybindRow(string desktopName, KeybindInfo keybind, int desktopNumber, bool isAssignment)
        {
            var border = new Border
            {
                Style = (Style)FindResource("KeybindRowStyle")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var stackPanel = new StackPanel();
            var titleText = new TextBlock
            {
                Text = desktopName,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };
            var descriptionText = new TextBlock
            {
                Text = keybind.Description,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0)
            };

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(descriptionText);

            var button = new Button
            {
                Style = (Style)FindResource("KeybindButton"),
                Content = keybind.ToString(),
                Tag = new KeybindButtonTag { DesktopNumber = desktopNumber, IsAssignment = isAssignment },
                MinWidth = 120
            };
            button.Click += KeybindButton_Click;

            Grid.SetColumn(stackPanel, 0);
            Grid.SetColumn(button, 1);

            grid.Children.Add(stackPanel);
            grid.Children.Add(button);

            border.Child = grid;

            // Store button reference
            if (isAssignment)
            {
                assignmentButtons[desktopNumber] = button;
            }
            else
            {
                switchButtons[desktopNumber] = button;
            }

            return border;
        }

        private void KeybindButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is KeybindButtonTag tag)
            {
                currentlyCapturingButton = button;
                button.Content = "Press keys...";
                button.Background = System.Windows.Media.Brushes.LightYellow;
                button.Focus();
            }
        }

        private void KeybindConfigWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentlyCapturingButton == null) return;

            e.Handled = true;

            var modifiers = Models.ModifierKeys.None;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                modifiers |= Models.ModifierKeys.Control;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                modifiers |= Models.ModifierKeys.Alt;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                modifiers |= Models.ModifierKeys.Shift;
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
                modifiers |= Models.ModifierKeys.Windows;

            // Convert WPF Key to VirtualKey
            var virtualKey = ConvertWpfKeyToVirtualKey(e.Key);
            if (virtualKey != null)
            {
                var keybindInfo = new KeybindInfo
                {
                    Modifiers = modifiers,
                    Key = virtualKey.Value,
                    Description = GetKeybindDescription(modifiers, virtualKey.Value)
                };

                // Update the configuration
                var tag = (KeybindButtonTag)currentlyCapturingButton.Tag;
                int desktopNumber = tag.DesktopNumber;
                bool isAssignment = tag.IsAssignment;

                if (isAssignment)
                {
                    currentConfig.AssignmentKeybinds[desktopNumber] = keybindInfo;
                }
                else
                {
                    currentConfig.SwitchKeybinds[desktopNumber] = keybindInfo;
                }

                // Update button
                currentlyCapturingButton.Content = keybindInfo.ToString();
                currentlyCapturingButton.Style = (Style)FindResource("KeybindButton");
                currentlyCapturingButton = null;
            }
        }

        private void KeybindConfigWindow_KeyUp(object sender, KeyEventArgs e)
        {
            // Handle key up if needed
        }

        private VirtualKey? ConvertWpfKeyToVirtualKey(Key key)
        {
            return key switch
            {
                Key.D0 => VirtualKey.D0,
                Key.D1 => VirtualKey.D1,
                Key.D2 => VirtualKey.D2,
                Key.D3 => VirtualKey.D3,
                Key.D4 => VirtualKey.D4,
                Key.D5 => VirtualKey.D5,
                Key.D6 => VirtualKey.D6,
                Key.D7 => VirtualKey.D7,
                Key.D8 => VirtualKey.D8,
                Key.D9 => VirtualKey.D9,
                Key.A => VirtualKey.A,
                Key.B => VirtualKey.B,
                Key.C => VirtualKey.C,
                Key.D => VirtualKey.D,
                Key.E => VirtualKey.E,
                Key.F => VirtualKey.F,
                Key.G => VirtualKey.G,
                Key.H => VirtualKey.H,
                Key.I => VirtualKey.I,
                Key.J => VirtualKey.J,
                Key.K => VirtualKey.K,
                Key.L => VirtualKey.L,
                Key.M => VirtualKey.M,
                Key.N => VirtualKey.N,
                Key.O => VirtualKey.O,
                Key.P => VirtualKey.P,
                Key.Q => VirtualKey.Q,
                Key.R => VirtualKey.R,
                Key.S => VirtualKey.S,
                Key.T => VirtualKey.T,
                Key.U => VirtualKey.U,
                Key.V => VirtualKey.V,
                Key.W => VirtualKey.W,
                Key.X => VirtualKey.X,
                Key.Y => VirtualKey.Y,
                Key.Z => VirtualKey.Z,
                Key.F1 => VirtualKey.F1,
                Key.F2 => VirtualKey.F2,
                Key.F3 => VirtualKey.F3,
                Key.F4 => VirtualKey.F4,
                Key.F5 => VirtualKey.F5,
                Key.F6 => VirtualKey.F6,
                Key.F7 => VirtualKey.F7,
                Key.F8 => VirtualKey.F8,
                Key.F9 => VirtualKey.F9,
                Key.F10 => VirtualKey.F10,
                Key.F11 => VirtualKey.F11,
                Key.F12 => VirtualKey.F12,
                Key.Tab => VirtualKey.Tab,
                Key.Enter => VirtualKey.Enter,
                Key.Escape => VirtualKey.Escape,
                Key.Space => VirtualKey.Space,
                Key.Insert => VirtualKey.Insert,
                Key.Delete => VirtualKey.Delete,
                Key.Home => VirtualKey.Home,
                Key.End => VirtualKey.End,
                Key.PageUp => VirtualKey.PageUp,
                Key.PageDown => VirtualKey.PageDown,
                _ => null
            };
        }

        private string GetKeybindDescription(Models.ModifierKeys modifiers, VirtualKey key)
        {
            var action = modifiers.HasFlag(Models.ModifierKeys.Control) ? "Assign to" : "Switch to";
            var desktopNumber = GetDesktopNumberFromKey(key);
            return $"{action} Desktop {desktopNumber}";
        }

        private int GetDesktopNumberFromKey(VirtualKey key)
        {
            return key switch
            {
                VirtualKey.D1 => 1,
                VirtualKey.D2 => 2,
                VirtualKey.D3 => 3,
                VirtualKey.D4 => 4,
                VirtualKey.D5 => 5,
                VirtualKey.D6 => 6,
                VirtualKey.D7 => 7,
                VirtualKey.D8 => 8,
                VirtualKey.D9 => 9,
                _ => 1
            };
        }

        private void BtnResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all keybinds to their default values?",
                "Reset to Defaults",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentConfig = new KeybindConfiguration();
                PopulateKeybinds();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigurationService.Instance.SaveConfiguration(currentConfig);
                MessageBox.Show(
                    "Keybind configuration saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save configuration: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (currentlyCapturingButton != null)
            {
                currentlyCapturingButton.Content = "Press keys...";
                currentlyCapturingButton.Style = (Style)FindResource("KeybindButton");
                currentlyCapturingButton = null;
            }
            base.OnClosing(e);
        }
    }
} 