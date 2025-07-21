using System;
using System.Windows;

namespace GlassPane.Services
{
    public static class ErrorHandler
    {
        public static void HandleException(Exception ex, string operation, bool showMessageBox = true)
        {
            string errorMessage = $"Failed to {operation}: {ex.Message}";
            
            // Log to debug output
            System.Diagnostics.Debug.WriteLine($"GlassPane Error: {errorMessage}");
            
            // Show message box if requested
            if (showMessageBox)
            {
                UIHelper.ShowError(errorMessage);
            }
        }

        public static void ShowNotification(string message, bool isError = false)
        {
            // Log to debug output
            System.Diagnostics.Debug.WriteLine($"GlassPane: {message}");
            
            // In a real implementation, you might want to show a toast notification
            // For now, we'll just log to debug output
        }

        public static bool ConfirmAction(string message, string title = "Confirm")
        {
            return UIHelper.ShowConfirmation(message, title);
        }
    }
} 