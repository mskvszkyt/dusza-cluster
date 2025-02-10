/// <summary>
/// Interaction logic for SzamitogepHozzaadasa.xaml
/// </summary>using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ConsoleApp1;
using Path = System.IO.Path;

namespace ClusterWPF.Pages
{
    /// <summary>
    /// Interaction logic for SzamitogepHozzaadasa.xaml
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

                MessageBox.Show("New computer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            else
            {
                MessageBox.Show("Please enter valid numeric values for processor and memory.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}