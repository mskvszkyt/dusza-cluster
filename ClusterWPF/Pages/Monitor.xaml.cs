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
using System.Windows.Input;
using System.IO;

namespace ClusterWPF.Pages
{
    public partial class Monitor : Page
    {
        private List<Instance> _instances;
        bool _clusterState;
        private string _path;

        public Monitor(List<Instance> instances, bool clusterState, string path)
        {
            _instances = instances;
            _clusterState = clusterState;
            _path = path;
            InitializeComponent();
            PopulateUI();
            PopulateStatistics();
            PopulateSearchComboBox();
            cbSelect.SelectionChanged += ComboBox_SelectionChanged;
            if (path != null)
            {
               lbMonitorName.Content = path.TrimEnd('\\').Split('\\').Last();
            }
        }

        private void PopulatePrograms()
        {
            var groupedPrograms = _instances
                .SelectMany(instance => instance.Programs.Select(program => new { instance, program }))
                .GroupBy(x => x.program.ProgramName.Split('-')[0]);


            // Clear existing items
            ProgramDetailsContainer.Children.Clear();

            foreach (var group in groupedPrograms)
            {
                var expander = new Expander
                {
                    Padding = new Thickness(10),
                    Header = $"Programnév: {group.Key}",
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
                    stackPanel.Children.Add(new Label { Content = item.program.IsRunning ? "Aktív" : "Inaktív", Foreground = statusColor });

                    listBoxItem.Content = stackPanel;
                    listBox.Items.Add(listBoxItem);
                }

                expander.Content = listBox;
                ProgramDetailsContainer.Children.Add(expander);
            }
        }

        private void PopulateUI()
        {
            InstanceContainer.Children.Clear();
            PopulatePrograms();
            PopulateStatistics();

            if (_clusterState)
            {
                lbClusterStatus.Content = "Megfelelő";
                lbClusterStatus.Foreground = Brushes.Green;

            } else
            {
                lbClusterStatus.Content = "Inkorrekt";
                lbClusterStatus.Foreground = Brushes.Red;
            }

            // Group programs by base name (before '-')
            var groupedPrograms = _instances
                .SelectMany(instance => instance.Programs.Select(program => new { instance, program }))
                .GroupBy(x => x.program.ProgramName.Split('-')[0]);

           
            // Clear existing items
            ProgramDetailsContainer.Children.Clear();

            foreach (var group in groupedPrograms)
            {
                var expander = new Expander
                {
                    Padding = new Thickness(10),
                    Header = $"Programnév: {group.Key}",
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
                    stackPanel.Children.Add(new Label { Content = item.program.IsRunning ? "Aktív" : "Inaktív", Foreground = statusColor });

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
                RunningOn = $"Futtató gép: {instance.Name}",
                MemoryUsage = $"Memória használat: {program.MemoryUsage}MB",
                ProcessorUsage = $"Processzor használat: {program.ProcessorUsage}mMg"
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

                //stackPanel.Children.Add(new Border
                //{
                //    BorderThickness = new Thickness(1),
                //    BorderBrush = (Brush)FindResource("MahApps.Brushes.Button.Border")
                //});

                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = (Brush)FindResource("AccentBrush")
                });

                // PC Image
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Pages/Images/PC2.png");
                bitmap.EndInit();

                stackPanel.Children.Add(new Image
                {
                    Width = 100,
                    Height = 80,
                    Source = bitmap,
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
                    Content = "Memória használat:",
                    Padding = new Thickness(5)
                });

                stackPanel.Children.Add(new Label
                {
                    Content = $"{instance.CalculateMemoryUsage()}/{instance.MemoryCapacity}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0, 5, 0, 0)
                });

                var memoryUsage = (double)instance.CalculateMemoryUsage() / instance.MemoryCapacity * 100;
                var memoryProgressBar = new ProgressBar
                {
                    Margin = new Thickness(5),
                    Value = memoryUsage
                };


                if (memoryUsage <= 60)
                    memoryProgressBar.Foreground = new SolidColorBrush(Colors.Green);
                else if (memoryUsage <= 80)
                    memoryProgressBar.Foreground = new SolidColorBrush(Colors.Yellow);
                else
                    memoryProgressBar.Foreground = new SolidColorBrush(Colors.Red);

                stackPanel.Children.Add(memoryProgressBar);



                // Processor Usage
                stackPanel.Children.Add(new Label
                {
                    Content = "Processzor használat:",
                    Padding = new Thickness(5)
                });

                stackPanel.Children.Add(new Label
                {
                    Content = $"{instance.CalculateProcessorUsage()}/{instance.ProcessorCapacity}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Padding = new Thickness(0, 5, 0, 0)
                });

                

                var usage = (double)instance.CalculateProcessorUsage() / instance.ProcessorCapacity * 100;
                var progressBar = new ProgressBar
                {
                    Margin = new Thickness(5),
                    Value = usage
                };

                // Set color based on usage
                if (usage <= 60)
                    progressBar.Foreground = new SolidColorBrush(Colors.Green);
                else if (usage <= 80)
                    progressBar.Foreground = new SolidColorBrush(Colors.Yellow);
                else
                    progressBar.Foreground = new SolidColorBrush(Colors.Red);

                stackPanel.Children.Add(progressBar);


                stackPanel.Children.Add(new Border
                {
                    BorderThickness = new Thickness(2),
                    Margin = new Thickness(0, 5, 0, 0),
                    BorderBrush = (Brush)FindResource("AccentBrush")
                });

                // Remove Button
                var removeButton = new Button
                {
                    Content = "Törlés",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 200,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand
                };
                removeButton.Click += (s, e) => RemoveInstance(instance);
                stackPanel.Children.Add(removeButton);

                // Add instance content to the InstanceContainer
                InstanceContainer.Children.Add(instanceBorder);
            }
        }

        private void RemoveInstance(Instance instance)
        {
            if (instance.Programs.Any())
            {
                var moveChoice = MessageBox.Show(
                    "Ez a számítógép futtat programokat. Át szeretnéd helyezni ezeket egy másik számítógépre?",
                    "Programok áthelyezése",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (moveChoice == MessageBoxResult.Yes)
                {
                    var availableInstances = _instances.Where(i => i != instance).ToList();

                    if (!availableInstances.Any())
                    {
                        MessageBox.Show("Nincs elérhető számítógép a programok áthelyezésére!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    foreach (var program in instance.Programs.ToList())
                    {
                        var targetInstance = availableInstances.FirstOrDefault(i =>
                            i.CalculateProcessorUsage() + program.ProcessorUsage <= i.ProcessorCapacity &&
                            i.CalculateMemoryUsage() + program.MemoryUsage <= i.MemoryCapacity);

                        if (targetInstance != null)
                        {
                            targetInstance.Programs.Add(program);
                            instance.Programs.Remove(program);

                            // Programfájl áthelyezése
                            string oldPath = Path.Combine(_path, instance.Name, program.ProgramName);
                            string newPath = Path.Combine(_path, targetInstance.Name, program.ProgramName);

                            if (File.Exists(oldPath))
                            {
                                try
                                {
                                    File.Move(oldPath, newPath);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Nem sikerült áthelyezni a(z) {program.ProgramName} programot!\nHiba: {ex.Message}",
                                        "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Nincs elég erőforrás a(z) {program.ProgramName} áthelyezésére!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (moveChoice == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Számítógép eltávolítása
            _instances.Remove(instance);

            string instanceFolderPath = Path.Combine(_path, instance.Name);
            if (Directory.Exists(instanceFolderPath))
            {
                try
                {
                    Directory.Delete(instanceFolderPath, true);
                    MessageBox.Show($"Törölve: {instanceFolderPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nem sikerült törölni a(z) {instanceFolderPath} mappát!\nHiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

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
                    RunningOn = $"Futtató gép: {_instances.FirstOrDefault(instance => instance.Programs.Contains(program))?.Name}",
                    MemoryUsage = $"Memória használat: {program.MemoryUsage}MB",
                    ProcessorUsage = $"Processzor használat: {program.ProcessorUsage}mm"
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
            if (cbSelect.SelectedIndex == 0)
            {
                svProgramDetails.Visibility = Visibility.Visible;
                scrollViewerSpecific.Visibility = Visibility.Collapsed;
                PopulatePrograms();
            }
            else
            {
                svProgramDetails.Visibility = Visibility.Collapsed;
                scrollViewerSpecific.Visibility = Visibility.Visible;
            }
        }

    }
}
