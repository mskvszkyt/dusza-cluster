/// <summary>
/// Interakciós logika a SzamitogepHozzaadasa.xaml számára
/// </summary>using ConsoleApp1;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ConsoleApp1;
using Path = System.IO.Path;

namespace ClusterWPF.Pages
{
    /// <summary>
    /// Interakciós logika a SzamitogepHozzaadasa.xaml számára
    /// </summary>
    public partial class AddNewComputer : Page
    {
        private Cluster _cluster;
        private string _path;

        public AddNewComputer(Cluster cluster, string path)
        {
            InitializeComponent();
            _cluster = cluster;
            _path = path;

            btnAddNewComputer.Click += BtnAddNewComputer_Click;
        }

        private void BtnAddNewComputer_Click(object sender, RoutedEventArgs e)
        {
            string instanceName = tbNewComputerName.Text;
            int processor;
            int memory;

            if (int.TryParse(tbNewComputerProcessor.Text, out processor) && processor > 0 &&
                int.TryParse(tbNewComputerMemory.Text, out memory) && memory > 0)
            {
                var newInstance = new Instance
                {
                    Name = instanceName,
                    ProcessorCapacity = processor,
                    MemoryCapacity = memory
                };

                _cluster.Instances.Add(newInstance);

                string instancePath = Path.Combine(_path, instanceName);
                Directory.CreateDirectory(instancePath);
                File.WriteAllText(Path.Combine(instancePath, ".szamitogep_config"), $"{processor}\n{memory}");

                MessageBox.Show("Új számítógép sikeresen hozzáadva!", "Sikeres", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Kérjük, érvényes numerikus értékeket adjon meg a processzorhoz és a memóriához.", "Érvénytelen bevitel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
