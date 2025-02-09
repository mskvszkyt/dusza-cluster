using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClusterWPF.Pages
{
    public partial class ComputerRemove : Page
    {
        private Cluster _cluster;
        private List<Instance> _allInstances;
        private readonly string _clusterPath;

        public ComputerRemove(Cluster cluster, string clusterPath)
        {
            InitializeComponent();
            _cluster = cluster;
            _clusterPath = clusterPath;
            _allInstances = _cluster.Instances.ToList();
            lbRemovableComputers.ItemsSource = _allInstances.Select(i => i.Name).ToList();

            tbSearchRemoveableComputers.TextChanged += TbSearchRemoveableComputers_TextChanged;
            btnRemoveComputer.Click += BtnRemoveComputer_Click;
        }

        private void TbSearchRemoveableComputers_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = tbSearchRemoveableComputers.Text.ToLower();
            lbRemovableComputers.ItemsSource = _allInstances
                .Where(i => i.Name.ToLower().Contains(searchText))
                .Select(i => i.Name)
                .ToList();
        }

        private void BtnRemoveComputer_Click(object sender, RoutedEventArgs e)
        {
            if (lbRemovableComputers.SelectedItem is not string selectedInstanceName)
            {
                MessageBox.Show("Please select a computer to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var instanceToRemove = _cluster.Instances.FirstOrDefault(i => i.Name == selectedInstanceName);
            if (instanceToRemove != null)
            {
                _cluster.Instances.Remove(instanceToRemove);

                string instanceFolderPath = Path.Combine(_clusterPath, instanceToRemove.Name);
                if (Directory.Exists(instanceFolderPath))
                {
                    try
                    {
                        Directory.Delete(instanceFolderPath, true);
                        MessageBox.Show($"Deleted folder: {instanceFolderPath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete folder {instanceFolderPath}.\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Remove cluster folder if empty
                if (Directory.Exists(_clusterPath) && !Directory.EnumerateFileSystemEntries(_clusterPath).Any())
                {
                    try
                    {
                        Directory.Delete(_clusterPath);
                        MessageBox.Show($"Deleted empty cluster folder: {_clusterPath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete cluster folder {_clusterPath}.\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            // Refresh the list
            _allInstances = _cluster.Instances.ToList();
            lbRemovableComputers.ItemsSource = _allInstances.Select(i => i.Name).ToList();
        }
    }
}
