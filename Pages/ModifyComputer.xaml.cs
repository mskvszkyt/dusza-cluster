using ConsoleApp1;
using System;
using System.Collections.ObjectModel;
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
            dataGrid.ItemsSource = _allInstances;

            // Event handlers
            tbComputerName.TextChanged += TbComputerName_TextChanged;
            sldMemory.ValueChanged += FiltersChanged;
            sldProcessor.ValueChanged += FiltersChanged;
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            btnSaveComputerSpecs.Click += BtnSaveComputerSpecs_Click;
        }

        private void TbComputerName_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void FiltersChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyFilters();

        private void ApplyFilters()
        {
            string searchText = tbComputerName.Text.ToLower(); 

          
            int minMemory = (int)sldMemory.LowerValue;
            int maxMemory = (int)sldMemory.UpperValue;
            int minProcessor = (int)sldProcessor.LowerValue;
            int maxProcessor = (int)sldProcessor.UpperValue;

            
            var filteredInstances = _allInstances
                .Where(i =>
                    (string.IsNullOrEmpty(searchText) || i.Name.ToLower().Contains(searchText)) &&
                   
                    i.MemoryCapacity >= minMemory && i.MemoryCapacity <= maxMemory &&
                    // Processor Capacity filter
                    i.ProcessorCapacity >= minProcessor && i.ProcessorCapacity <= maxProcessor)
                .ToList();

            // Update the DataGrid with the filtered instances
            dataGrid.ItemsSource = new ObservableCollection<Instance>(filteredInstances);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is Instance selectedInstance)
            {
                tbMemoryChange.Text = selectedInstance.MemoryCapacity.ToString();
                tbProcessorChange.Text = selectedInstance.ProcessorCapacity.ToString();
            }
        }

        private void BtnSaveComputerSpecs_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is not Instance selectedInstance)
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
