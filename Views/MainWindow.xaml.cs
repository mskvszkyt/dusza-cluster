using System.Windows;
using DuszaArpadWPF.Services;

namespace DuszaArpadWPF.Views
{
    public partial class MainWindow : Window
    {
        private ClusterService _clusterService;

        public MainWindow()
        {
            InitializeComponent();

            var pathDialog = new PathDialog();
            if (pathDialog.ShowDialog() == true)
            {
                var clusterService = new ClusterService();
                clusterService.SetRootPath(pathDialog.Path);
                _clusterService = clusterService;
            }
        }

        public MainWindow(ClusterService clusterservice)
        {
            InitializeComponent();
            _clusterService = clusterservice;
        }

        private void Monitoring_Click(object sender, RoutedEventArgs e)
        {
            var computers = _clusterService.LoadComputers();
            ComputersGrid.ItemsSource = computers;
        }

        private void AddComputer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddComputerDialog();
            if (dialog.ShowDialog() == true)
            {
                _clusterService.AddComputer(dialog.ComputerName, dialog.CpuCapacity, dialog.MemoryCapacity);
            }
        }
    }
}