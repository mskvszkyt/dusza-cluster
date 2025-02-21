using ConsoleApp1;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace ClusterWPF.Pages
{
    public partial class StopProgramCopy : Page
    {
        private MainWindow mainWindow;

        // ObservableCollection for dynamic updates
        private ObservableCollection<dynamic> _programCopies;

        public StopProgramCopy()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            LoadProgramCopies();

            btnRemoveProgramCopy.Click += BtnRemoveProgramCopy_Click;
        }

        private void LoadProgramCopies()
        {
            // Get the program copies from all instances and project them into an anonymous type
            _programCopies = new ObservableCollection<dynamic>(mainWindow.cluster.Instances
                .SelectMany(i => i.Programs)
                .Where(i => i.IsRunning)
                .Select(p =>
                    p.ProgramName
                )
                .ToList());

            cbProgramInstances.ItemsSource = _programCopies;
        }

        private void BtnRemoveProgramCopy_Click(object sender, RoutedEventArgs e)
        {
            // Explicitly cast the selected item to an anonymous type
            var selectedProgramCopy = cbProgramInstances.SelectedItem as dynamic;

            if (selectedProgramCopy == null)
            {
                MessageBox.Show("Válaszd ki a leállítandó programpéldányt.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extract the program name from the selected item
            string programName = selectedProgramCopy;

            foreach (var instance in mainWindow.cluster.Instances)
            {
                var programCopy = instance.Programs.FirstOrDefault(p => p.ProgramName == programName);

                if (programCopy != null)
                {
                    // Delete the physical file
                    string filePath = Path.Combine(mainWindow.cluster.Path, instance.Name, programName);
                    if (File.Exists(filePath)) File.Delete(filePath);

                    // Remove the program copy from the instance's list
                    instance.Programs.Remove(programCopy);

                    MessageBox.Show($"Példány sikeresen leállítva: {programName}", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProgramCopies(); // Reload the program copies list
                    return;
                }
            }

            MessageBox.Show("Hiba: A programpéldány nem található.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
