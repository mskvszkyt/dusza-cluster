using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using ConsoleApp1;

namespace ClusterWPF.Pages
{
    public partial class RemoveProgram : Page
    {
        private readonly Cluster _cluster;
        private readonly string _path;

        // Use ObservableCollection for dynamic updates
        private ObservableCollection<dynamic> _programs;

        public RemoveProgram(Cluster cluster, string path)
        {
            InitializeComponent();
            _cluster = cluster;
            _path = path;
            LoadPrograms();

            btnRemoveProgram.Click += BtnRemoveProgram_Click;
        }

        private void LoadPrograms()
        {
            // Fetch all scheduled programs and display them in the ListBox
            _programs = new ObservableCollection<dynamic>(_cluster.ScheduledPrograms
                .Select(p => p.ProgramName)
                .ToList());

            cbPrograms.ItemsSource = _programs;
        }

        private void BtnRemoveProgram_Click(object sender, RoutedEventArgs e)
        {
            if (cbPrograms.SelectedItem == null)
            {
                MessageBox.Show("Válassz ki az eltávolítandó programot.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the selected program details
            var selectedProgram = cbPrograms.SelectedItem as dynamic;
            string programName = selectedProgram;

            // Call the method to shut down the program
            ShutDownProgram(programName, _path, _cluster);

            // Reload the list of programs after removal
            LoadPrograms();
        }

        public static void ShutDownProgram(string programName, string path, Cluster cluster)
        {
            // 1. Remove from ScheduledPrograms
            var scheduledProgram = cluster.ScheduledPrograms.FirstOrDefault(sp => sp.ProgramName == programName);
            if (scheduledProgram == null)
            {
                ShowError("Hiba: Nem található program.");
                return;
            }
            cluster.ScheduledPrograms.Remove(scheduledProgram);

            // 2. Delete all related program instances and files
            bool anyDeleted = false;
            foreach (var instance in cluster.Instances)
            {
                var programsToRemove = instance.Programs
                    .Where(p => p.ProgramName.StartsWith($"{programName}-", StringComparison.Ordinal))
                    .ToList();

                foreach (var program in programsToRemove)
                {
                    string filePath = Path.Combine(path, instance.Name, program.ProgramName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        anyDeleted = true;
                    }

                    instance.Programs.Remove(program);
                }
            }

            // 3. Update cluster configuration file
            FileManager.WriteCluster(path, cluster);

            MessageBox.Show(anyDeleted
                ? "A program és minden példánya eltávolításra került."
                : "Figyelmeztetés: A program nem futott egyik gépen sem.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
