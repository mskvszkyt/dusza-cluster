using ConsoleApp1;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ProgressBar = ModernWpf.Controls.ProgressBar;
using MahApps.Metro.Controls;
using ModernWpf.Controls.Primitives;
using Page = System.Windows.Controls.Page;
using System.Diagnostics;

namespace ClusterWPF.Pages
{
    public partial class Monitor : Page
    {
        private List<Instance> _instances;
        private bool _isLoaded = false; // Flag to track if the window is initialized

        public Monitor(List<Instance> instances)
        {
            _instances = instances;
            InitializeComponent();
            PopulateUI();
            PopulateStatistics();
            PopulateSearchComboBox();
        }

        

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
        }

        private void PopulateUI()
        {

            // Clear existing items
            ProgramDetailsContainer.Children.Clear();

            // Group programs by base name (before '-')
            var groupedPrograms = _instances
                .SelectMany(instance => instance.Programs.Select(program => new { instance, program }))
                .GroupBy(x => x.program.ProgramName.Split('-')[0]);

            foreach (var group in groupedPrograms)
            {
                var expander = new Expander
                {
                    Padding = new Thickness(10),
                    Header = $"ProgramName: {group.Key}",
                    ExpandDirection = ExpandDirection.Down,
                    IsExpanded = false,
                    FontSize = 16,
                    Focusable = false
                };

                var listBox = new ListBox { VerticalAlignment = VerticalAlignment.Stretch, IsHitTestVisible = false };
                foreach (var item in group)
                {
                    var statusColor = item.program.IsRunning ? Brushes.Green : Brushes.Red;

                    var listBoxItem = new ListBoxItem { VerticalAlignment = VerticalAlignment.Stretch };
                    var stackPanel = new SimpleStackPanel { Orientation = Orientation.Horizontal };

                    stackPanel.Children.Add(new Label { Content = item.program.ProgramName.Split("-")[1] });
                    stackPanel.Children.Add(new Label { Content = ":", Margin = new Thickness(10, 0, 10, 0) });
                    stackPanel.Children.Add(new Label { Content = item.program.IsRunning ? "Active" : "Inactive", Foreground = statusColor });

                    listBoxItem.Content = stackPanel;
                    listBox.Items.Add(listBoxItem);
                }

                expander.Content = listBox;
                ProgramDetailsContainer.Children.Add(expander);
            }

            // Clear existing items
            ProgramDetailsContainer.Children.Clear();

            foreach (var group in groupedPrograms)
            {
                var expander = new Expander
                {
                    Padding = new Thickness(10),
                    Header = $"ProgramName: {group.Key}",
                    ExpandDirection = ExpandDirection.Down,
                    IsExpanded = false,
                    FontSize = 16,
                    Focusable = false
                };

                var listBox = new ListBox { VerticalAlignment = VerticalAlignment.Stretch, IsHitTestVisible = false };
                foreach (var item in group)
                {
                    var statusColor = item.program.IsRunning ? Brushes.Green : Brushes.Red;

                    var listBoxItem = new ListBoxItem { VerticalAlignment = VerticalAlignment.Stretch };
                    var stackPanel = new SimpleStackPanel { Orientation = Orientation.Horizontal };

                    stackPanel.Children.Add(new Label { Content = item.program.ProgramName.Split("-")[1] });
                    stackPanel.Children.Add(new Label { Content = ":", Margin = new Thickness(10, 0, 10, 0) });
                    stackPanel.Children.Add(new Label { Content = item.program.IsRunning ? "Active" : "Inactive", Foreground = statusColor });

                    listBoxItem.Content = stackPanel;
                    listBox.Items.Add(listBoxItem);
                }

                expander.Content = listBox;
                ProgramDetailsContainer.Children.Add(expander);
            }

            // Initialize the program instances collection for the ItemsControl
            var programList = _instances.SelectMany(instance => instance.Programs.Where(x => x.IsRunning == true).Select(program => new
            {
                Name = program.ProgramName,
                RunningOn = $"Running on: {instance.Name}",
                MemoryUsage = $"Memory usage: {program.MemoryUsage}MB",
                ProcessorUsage = $"Processor usage: {program.ProcessorUsage}mm"
            })).ToList();

            // Set the ItemsSource of the ItemsControl to the list of program details
            programInstances.ItemsSource = programList;

            foreach (var instance in _instances)
            {
                var instanceBorder = new Border
                {
                    BorderBrush = (Brush)FindResource("AccentBrush"),
                    BorderThickness = new Thickness(2),
                    Margin = new Thickness(5, 10, 5, 10),
                    Background = (Brush)FindResource("MahApps.Brushes.SystemControlBackgroundListLow")
                };

                // Create the SimpleStackPanel for instance content
                var stackPanel = new SimpleStackPanel();
                instanceBorder.Child = stackPanel;

                // PC Name Label
                stackPanel.Children.Add(new Label
                {
                    Content = instance.Name,
                    FontSize = 20,
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = (Brush)FindResource("MahApps.Brushes.Button.Border")
                });

                // PC Image
                stackPanel.Children.Add(new Image
                {
                    Width = 100,
                    Height = 80,
                    Source = new BitmapImage(new Uri("Images/PC.png", UriKind.Relative)),
                    RenderTransformOrigin = new System.Windows.Point(0.5, 0.5)
                });

                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)FindResource("AccentBrush")
                });

                // Memory Usage
                stackPanel.Children.Add(new Label
                {
                    Content = "Memory Usage:",
                    Padding = new Thickness(5)
                });

                stackPanel.Children.Add(new Label
                {
                    Content = $"{instance.CalculateMemoryUsage()}/{instance.MemoryCapacity}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0, 5, 0, 0)
                });

                stackPanel.Children.Add(new ProgressBar
                {
                    Margin = new Thickness(5),
                    Value = (double)instance.CalculateMemoryUsage() / instance.MemoryCapacity * 100
                });

                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)FindResource("AccentBrush")
                });

                // Processor Usage
                stackPanel.Children.Add(new Label
                {
                    Content = "Processor Usage:",
                    Padding = new Thickness(5)
                });

                stackPanel.Children.Add(new Label
                {
                    Content = $"{instance.CalculateProcessorUsage()}/{instance.ProcessorCapacity}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0, 5, 0, 0)
                });

                stackPanel.Children.Add(new ProgressBar
                {
                    Margin = new Thickness(5),
                    Value = (double)instance.CalculateProcessorUsage() / instance.ProcessorCapacity * 100
                });

                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)FindResource("AccentBrush")
                });

                // Remove Button
                var removeButton = new Button
                {
                    Content = "Remove",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 200,
                    Margin = new Thickness(5)
                };
                removeButton.Click += (s, e) => RemoveInstance(instance);
                stackPanel.Children.Add(removeButton);

                // Add instance content to the InstanceContainer
                InstanceContainer.Children.Add(instanceBorder);
            }
        }

        private void RemoveInstance(Instance instance)
        {
            _instances.Remove(instance);
            InstanceContainer.Children.Clear();
            PopulateUI();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement search functionality here
            var query = searchComboBox.Text.ToLower();

            // Filter the programs based on the search query
            var filteredPrograms = _instances.SelectMany(instance => instance.Programs)
                .Where(program => program.ProgramName.ToLower().Contains(query) && program.IsRunning == true)
                .Select(program => new
                {
                    Name = program.ProgramName, // Get the instance name
                    ProgramName = _instances.FirstOrDefault(instance => instance.Programs.Contains(program))?.Name.ToLower(),
                    RunningOn = $"Running on: {_instances.FirstOrDefault(instance => instance.Programs.Contains(program))?.Name}",
                    MemoryUsage = $"Memory usage: {program.MemoryUsage}MB",
                    ProcessorUsage = $"Processor usage: {program.ProcessorUsage}mm"
                }).ToList();

            // Update the ItemsControl with filtered results
            programInstances.ItemsSource = filteredPrograms;
        }

        private void PopulateStatistics()
        {
            // Count active and inactive processes across all instances
            int activeCount = _instances.SelectMany(instance => instance.Programs)
                                        .Count(program => program.IsRunning);

            int inactiveCount = _instances.SelectMany(instance => instance.Programs)
                                          .Count(program => !program.IsRunning);

            lbActiveProcesses.Content = activeCount.ToString();
            lbInactiveProcesses.Content = inactiveCount.ToString();
        }

        private void PopulateSearchComboBox()
        {
           
            var programNames = _instances.SelectMany(instance => instance.Programs)
                                         .Where(program => program.IsRunning)
                                         .Select(program => program.ProgramName)
                                         .Distinct()
                                         .ToList();

            searchComboBox.ItemsSource = programNames;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return; // Prevents execution before UI is fully loaded

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedText = selectedItem.Content?.ToString();

                if (selectedText == "Running programs details")
                {
                    Dispatcher.Invoke(() =>
                    {
                        svProgramDetails.Visibility = Visibility.Visible;
                        svSpecificProgram.Visibility = Visibility.Collapsed;
                        Debug.WriteLine(svProgramDetails.Visibility);
                        svProgramDetails.UpdateLayout();
                    });


                }
                else if (selectedText == "Search a specific program")
                {
                    Dispatcher.Invoke(() =>
                    {
                        svProgramDetails.Visibility = Visibility.Collapsed;
                        svSpecificProgram.Visibility = Visibility.Visible;
                        svSpecificProgram.UpdateLayout();
                        svProgramDetails.UpdateLayout();
                        Debug.WriteLine(svSpecificProgram.Visibility);
                    });
                }
            }
        }

    }
}
