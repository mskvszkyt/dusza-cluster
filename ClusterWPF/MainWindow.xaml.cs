using ConsoleApp1;
using ControlzEx.Theming;
using Microsoft.Win32;
using System.Runtime;
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
using Windows.UI.ViewManagement;

namespace ClusterWPF
{
    public partial class MainWindow : Window
    {
        List<Cluster> clusters = new List<Cluster>();
        Cluster cluster = new Cluster();
        string path;
        BindingList<string> clusterNames = new();

        public MainWindow()
        {
            InitializeComponent();
            lbClusterNames.ItemsSource = clusterNames;
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
            UpdateAccentColor();
        }

        private void UpdateAccentColor()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            int color = (int)key.GetValue("AccentColor", 0);
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(255, (byte)(color & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF)));

            Application.Current.Resources["AccentBrush"] = new SolidColorBrush(
                Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B)
            );
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcPages.SelectedIndex != -1)
            {
                var tabs = tcPages.Items
                    .Cast<TabItem>()
                    .Where(G => G.IsSelected)
                    .Select(G => G.Header.ToString())
                    .First();

                CurrentPage.Content = tabs switch
                {
                    "Monitor" => new Pages.Monitor(cluster.Instances),
                    "Add new computer" => new Pages.AddNewComputer(cluster, path),
                    "Remove Computer" => new Pages.ComputerRemove(cluster, path),
                    "Remove Program" => new Pages.RemoveProgram(cluster, path),
                    "Stop Program Instance" => new Pages.StopProgramCopy(cluster, path),
                    "Modify Computer" => new Pages.ModifyComputer(cluster, path),
                    "Run program Instance" => new Pages.NewProgramCopy(),
                    "Charts" => new Pages.Charts()
                };
            }
        }

        private void btnAddNewCluster_Click(object sender, RoutedEventArgs e)
        {
            path = tbAddCluster.Text;
            cluster = FileManager.GetClusterRequirements(path);
            cluster.Instances = FileManager.ReadInstances(path);
            clusters.Add(cluster)
            clusterNames.Add(path.Split('\\').Last());
            RefreshCurrentPage();
        }

        private void RefreshCurrentPage()
        {
            var selectedTabHeader = tcPages.SelectedItem is TabItem selectedTab ? selectedTab.Header.ToString() : null;

            if (selectedTabHeader != null)
            {
                CurrentPage.Content = selectedTabHeader switch
                {
                    "Monitor" => new Pages.Monitor(cluster.Instances),
                    "Add new computer" => new Pages.AddNewComputer(cluster, path),
                    "Remove Computer" => new Pages.ComputerRemove(cluster, path),
                    "Remove Program" => new Pages.RemoveProgram(cluster, path),
                    "Stop Program Copy" => new Pages.StopProgramCopy(cluster, path),
                    "Modify Computer" => new Pages.ModifyComputer(cluster, path),
                    "Run program copy" => new Pages.NewProgramCopy(),
                    _ => CurrentPage.Content
                };
            }
        }
    }
}
