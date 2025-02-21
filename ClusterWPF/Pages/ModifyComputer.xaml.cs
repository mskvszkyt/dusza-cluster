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
        private MainWindow mainWindow;
        private ObservableCollection<Instance> _allInstances;

        public ModifyComputer()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;

            // Initialize the ObservableCollection with the Instances
            _allInstances = new ObservableCollection<Instance>(mainWindow.cluster.Instances);

            // Bind the DataGrid to the ObservableCollection
            dgdatas.ItemsSource = _allInstances;

            // Event handlers
            tbComputerName.TextChanged += TbComputerName_TextChanged;
            dgdatas.SelectionChanged += DataGrid_SelectionChanged;
            btnSaveComputerSpecs.Click += BtnSaveComputerSpecs_Click;
            
        }

        private void TbComputerName_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void FiltersChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyFilters();

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
                MessageBox.Show("Válassz ki egy módosítandó gépet.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(tbMemoryChange.Text, out int newMemory) &&
                int.TryParse(tbProcessorChange.Text, out int newProcessor))
            {
                // Update the instance properties
                selectedInstance.MemoryCapacity = newMemory;
                selectedInstance.ProcessorCapacity = newProcessor;

                // Update the .szamitogep_config file
                string instancePath = Path.Combine(mainWindow.cluster.Path, selectedInstance.Name);
                string configFilePath = Path.Combine(instancePath, ".szamitogep_config");

                if (File.Exists(configFilePath))
                {
                    try
                    {
                        File.WriteAllText(configFilePath, $"{newProcessor}\n{newMemory}");
                        MessageBox.Show("A gép sikeresen módosítva lett!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Hiba történt a config fájl módosítása során.\nHiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Konfigurációs fájl nem található. Ellenőrizd a gép mappáját.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Since the ObservableCollection is bound, changes should automatically reflect in the DataGrid
            }
            else
            {
                MessageBox.Show("Kérjük, érvényes numerikus értékeket adjon meg a processzorhoz és a memóriához.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
