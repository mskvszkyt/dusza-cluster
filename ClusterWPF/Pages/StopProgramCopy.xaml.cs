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
        private readonly Cluster _cluster;
        private readonly string _path;

        // ObservableCollection for dynamic updates
        private ObservableCollection<dynamic> _programCopies;

        public StopProgramCopy(Cluster cluster, string path)
        {
            InitializeComponent();
            _cluster = cluster;
            _path = path;
            LoadProgramCopies();

            btnRemoveProgramCopy.Click += BtnRemoveProgramCopy_Click;
        }

        private void LoadProgramCopies()
        {
            // Get the program copies from all instances and project them into an anonymous type
            _programCopies = new ObservableCollection<dynamic>(_cluster.Instances
                .SelectMany(i => i.Programs)
                .Where(i => i.IsRunning)
                .Select(p => new
                {
                    Id = p.ProgramName.Split('-').Last(),
                    FullName = p.ProgramName
                })
                .ToList());

            cbProgramInstances.ItemsSource = _programCopies;
        }

        private void BtnRemoveProgramCopy_Click(object sender, RoutedEventArgs e)
        {
            // Explicitly cast the selected item to an anonymous type
            var selectedProgramCopy = cbProgramInstances.SelectedItem as dynamic;

            if (selectedProgramCopy == null)
            {
                MessageBox.Show("Please select a program copy to stop.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extract the program name from the selected item
            string programName = selectedProgramCopy.FullName;

            foreach (var instance in _cluster.Instances)
            {
                var programCopy = instance.Programs.FirstOrDefault(p => p.ProgramName == programName);

                if (programCopy != null)
                {
                    // Delete the physical file
                    string filePath = Path.Combine(_path, instance.Name, programName);
                    if (File.Exists(filePath)) File.Delete(filePath);

                    // Remove the program copy from the instance's list
                    instance.Programs.Remove(programCopy);

                    MessageBox.Show($"Successfully stopped program copy: {programName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProgramCopies(); // Reload the program copies list
                    return;
                }
            }

            MessageBox.Show("Error: Program copy not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
