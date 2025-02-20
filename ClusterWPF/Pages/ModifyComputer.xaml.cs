using ConsoleApp1;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClusterWPF.Pages
{
    public partial class ModifyComputer : Page
    {
        private Cluster _cluster;
        private string _clusterPath;
        private ObservableCollection<Instance> _allInstances;

        public ModifyComputer(Cluster cluster, string clusterPath)
        {
            InitializeComponent();
            _cluster = cluster;
            _clusterPath = clusterPath;

            // Initialize the ObservableCollection with the Instances
            _allInstances = new ObservableCollection<Instance>(_cluster.Instances);

            // Bind the DataGrid to the ObservableCollection
            dgdatas.ItemsSource = _allInstances;

            // Event handlers
            tbComputerName.TextChanged += TbComputerName_TextChanged;
            dgdatas.SelectionChanged += DataGrid_SelectionChanged;
            btnSaveComputerSpecs.Click += BtnSaveComputerSpecs_Click;
            
        }

        private void TbComputerName_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void FiltersChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyFilters();

        /// <summary>
        /// Applies filters to the list of instances based on the text entered in the search box.
        /// It filters instances by comparing their names (case-insensitive) with the search text.
        /// </summary>
        private void ApplyFilters()
        {
            string searchText = tbComputerName.Text.ToLower();



            var filteredInstances = _allInstances
                .Where(i => i.Name.ToLower().Contains(searchText)).ToList();

            // Update the DataGrid with the filtered instances
            dgdatas.ItemsSource = new ObservableCollection<Instance>(filteredInstances);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgdatas.SelectedItem is Instance selectedInstance)
            {
                tbMemoryChange.Text = selectedInstance.MemoryCapacity.ToString();
                tbProcessorChange.Text = selectedInstance.ProcessorCapacity.ToString();
            }
        }

        private void BtnSaveComputerSpecs_Click(object sender, RoutedEventArgs e)
        {
            if (dgdatas.SelectedItem is not Instance selectedInstance)
            {
                MessageBox.Show("Please select a computer to modify.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(tbMemoryChange.Text, out int newMemory) &&
                int.TryParse(tbProcessorChange.Text, out int newProcessor))
            {
                // Update the instance properties
                selectedInstance.MemoryCapacity = newMemory;
                selectedInstance.ProcessorCapacity = newProcessor;

                // Update the .szamitogep_config file
                string instancePath = Path.Combine(_clusterPath, selectedInstance.Name);
                string configFilePath = Path.Combine(instancePath, ".szamitogep_config");

                if (File.Exists(configFilePath))
                {
                    try
                    {
                        File.WriteAllText(configFilePath, $"{newProcessor}\n{newMemory}");
                        MessageBox.Show("Computer specifications updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update config file.\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Configuration file not found. Please check the instance folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Since the ObservableCollection is bound, changes should automatically reflect in the DataGrid
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter numeric values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
