using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GlassPane.Models;
using GlassPane.Services;

namespace GlassPane
{
    public partial class PersistentAppConfigWindow : Window
    {
        private PersistentAppService persistentAppService;
        private ObservableCollection<PersistentAppAssignment> assignments;

        public PersistentAppConfigWindow()
        {
            InitializeComponent();
            persistentAppService = new PersistentAppService(VirtualDesktopManager.Instance);
            assignments = new ObservableCollection<PersistentAppAssignment>();
            AssignmentsListBox.ItemsSource = assignments;
            
            // Set default desktop number
            DesktopNumberComboBox.SelectedIndex = 0;
            
            LoadAssignments();
        }

        private void LoadAssignments()
        {
            assignments.Clear();
            var allAssignments = persistentAppService.GetAllPersistentAssignments();
            foreach (var assignment in allAssignments.OrderBy(a => a.ProcessName))
            {
                assignments.Add(assignment);
            }
        }

        private void AddAssignment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var processName = ProcessNameTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(processName))
                {
                    MessageBox.Show("Please enter a process name.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DesktopNumberComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a desktop number.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var desktopNumber = int.Parse(((System.Windows.Controls.ComboBoxItem)DesktopNumberComboBox.SelectedItem).Content.ToString());
                var description = DescriptionTextBox.Text?.Trim();

                // Check if assignment already exists
                if (persistentAppService.HasPersistentAssignment(processName))
                {
                    var result = MessageBox.Show(
                        $"An assignment for {processName} already exists. Do you want to replace it?",
                        "Assignment Exists",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        return;
                }

                // Add the assignment
                persistentAppService.AddPersistentAssignmentAsync(processName, desktopNumber, description).Wait();

                // Clear form
                ProcessNameTextBox.Text = "";
                DescriptionTextBox.Text = "";
                DesktopNumberComboBox.SelectedIndex = 0;

                // Reload assignments
                LoadAssignments();

                MessageBox.Show($"Successfully added persistent assignment: {processName} → Desktop {desktopNumber}", 
                    "Assignment Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add assignment: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveAssignment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as System.Windows.Controls.Button;
                var assignment = button.DataContext as PersistentAppAssignment;

                if (assignment != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove the persistent assignment for {assignment.ProcessName}?",
                        "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        persistentAppService.RemovePersistentAssignment(assignment.ProcessName);
                        LoadAssignments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove assignment: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 