using ConsoleApp1;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClusterWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Cluster cluster = new Cluster();
        string path;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabs = (sender as TabControl).Items
                .Cast<TabItem>()
                .Where(G => G.IsSelected)
                .Select(G => G.Header.ToString())
                .First();

            CurrentPage.Content = tabs switch
            {
                "Monitor" => new Pages.Monitor(),
                "Add new computer" => new Pages.SzamitogepHozzaadasa(cluster, path),
                "Remove Computer" => new Pages.ComputerRemove(cluster, path),
                "Remove Program" => new Pages.RemoveProgram(cluster, path),
                "Stop Program Copy" => new Pages.StopProgramCopy(cluster, path),
                "Modify Computer" => new Pages.ModifyComputer(cluster, path),
                _ => new Pages.Monitor()
            };
        }

        private void btnAddNewCluster_Click(object sender, RoutedEventArgs e)
        {
            path = tbAddCluster.Text;
            cluster = FileManager.GetClusterRequirements(path);
            cluster.Instances = FileManager.ReadInstances(path);
        }
    }
}