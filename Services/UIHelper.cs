using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GlassPane.Services
{
    public static class UIHelper
    {
        public static void ShowError(string message, string title = "Error")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public static void ShowInfo(string message, string title = "Information")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public static bool ShowConfirmation(string message, string title = "Confirm")
        {
            var result = Application.Current.Dispatcher.Invoke(() =>
            {
                return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            });
            
            return result == MessageBoxResult.Yes;
        }

        public static void RestoreButtonStyle(Button button, string styleName)
        {
            if (button != null)
            {
                // Try to find the resource in the button's visual tree first
                var style = button.TryFindResource(styleName) as Style;
                if (style == null)
                {
                    // Fallback to application resources
                    style = Application.Current.TryFindResource(styleName) as Style;
                }
                button.Style = style;
            }
        }

        public static void SetButtonCapturingState(Button button, bool isCapturing)
        {
            if (button != null)
            {
                if (isCapturing)
                {
                    button.Content = "Press keys...";
                    button.Background = Brushes.LightYellow;
                }
                else
                {
                    RestoreButtonStyle(button, "KeybindButton");
                }
            }
        }
    }
} 