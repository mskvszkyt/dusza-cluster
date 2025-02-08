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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                    "Add new computer" => new Pages.AddNewComputer(),
                    "Remove Computer" => new Pages.ComputerRemove(),
                    "Remove Program" => new Pages.RemoveProgram(),
                    "Stop Program Copy" => new Pages.StopProgramCopy(),
                    "Modify Computer" => new Pages.ModifyComputer(),
                    "Run program copy" => new Pages.NewProgramCopy(),
                };
            }
        }
    }
}