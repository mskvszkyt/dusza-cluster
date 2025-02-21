using ConsoleApp1;
using ControlzEx.Theming;
using Microsoft.Win32;
using System.ComponentModel;
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
using ClusterWPF.ViewModels;
using System.Collections.ObjectModel;

namespace ClusterWPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Cluster> clusters { get; set; } = new();
        bool _clusterState = false;
        public Cluster cluster { get; set; } = new();
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
                    "Monitor" => new Pages.Monitor(),
                    "Új gép hozzáadása" => new Pages.AddNewComputer(),
                    "Gép eltávolítása" => new Pages.ComputerRemove(),
                    "Program eltávolítása" => new Pages.RemoveProgram(),
                    "Programpéldány leállítása" => new Pages.StopProgramCopy(),
                    "Gép módosítása" => new Pages.ModifyComputer(),
                    "Program futtatása" => new Pages.NewProgramCopy(),
                    "Diagram" => new Pages.Charts(cluster),
                    "Klaszter műveletek" => new Pages.ClustersManagement()
                };

            }
        }

        private void btnAddNewCluster_Click(object sender, RoutedEventArgs e)
        {
            if (!clusterNames.Contains(tbAddCluster.Text.TrimEnd('\\').Split('\\').Last()))
            {
                try
                {
                    path = tbAddCluster.Text;
                    cluster = FileManager.GetClusterRequirements(path.TrimEnd('\\'));
                    cluster.Instances = FileManager.ReadInstances(path.TrimEnd('\\'));
                    clusters.Add(cluster);
                    clusterNames.Add(path.TrimEnd('\\').Split('\\').Last());
                    RefreshCurrentPage();
                    tbAddCluster.Text = string.Empty;
                    lbClusterNames.SelectedItem = path.TrimEnd('\\').Split('\\').Last();
                }
                catch (Exception exception)
                {
                    _clusterState = false;
                    MessageBox.Show(exception.Message);
                }
            }
            else
            {
                MessageBox.Show($"Ez a klaszter már be van töltve.");
            }
        }
            
        private void RefreshCurrentPage()
        {
            var selectedTabHeader = tcPages.SelectedItem is TabItem selectedTab ? selectedTab.Header.ToString() : null;

            if (selectedTabHeader != null)
            {
                CurrentPage.Content = selectedTabHeader switch
                {
                    "Monitor" => new Pages.Monitor(),
                    "Új gép hozzáadása" => new Pages.AddNewComputer(),
                    "Gép eltávolítása" => new Pages.ComputerRemove(),
                    "Program eltávolítása" => new Pages.RemoveProgram(),
                    "Programpéldány leállítása" => new Pages.StopProgramCopy(),
                    "Gép módosítása" => new Pages.ModifyComputer(),
                    "Program futtatása" => new Pages.NewProgramCopy(),
                    "Diagram" => new Pages.Charts(cluster),
                    "Klaszter műveletek" => new Pages.ClustersManagement()
                };
            }
        }

        private void Cluster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cluster = clusters.FirstOrDefault(c => c.Path.EndsWith(lbClusterNames.SelectedItem.ToString()));
            path = cluster.Path;
            RefreshCurrentPage();
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog
            {
                Title = "Tallózás",
                Multiselect = true 
            };

            if (dialog.ShowDialog() == true)
            {
                string[] selectedFolderPath = dialog.FolderNames;
                List<string> failed = new();
                Console.WriteLine(selectedFolderPath);
                selectedFolderPath.ToList().ForEach(currentpath =>
                {
                    if (!clusterNames.Contains(currentpath.TrimEnd('\\').Split('\\').Last()))
                    {
                        try
                        {
                                path = currentpath;
                                cluster = FileManager.GetClusterRequirements(path);
                                cluster.Instances = FileManager.ReadInstances(path);
                                clusters.Add(cluster);
                                clusterNames.Add(path.TrimEnd('\\').Split('\\').Last());
                                RefreshCurrentPage();
                                lbClusterNames.SelectedItem = path.TrimEnd('\\').Split('\\').Last();
                        }
                        catch (Exception exception)
                        {
                            _clusterState = false;
                            MessageBox.Show(exception.Message);
                        }
                    }
                    else
                    {
                        failed.Add(currentpath);
                    }
                });
                if (failed.Count != 0)
                {
                    MessageBox.Show($"A következő klaszterek már be vannak toltve: {string.Join(", ", failed)}");
                }
            }
        }
    }
}
