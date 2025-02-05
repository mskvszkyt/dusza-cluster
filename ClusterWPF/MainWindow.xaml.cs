﻿using System.Text;
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
                "Add new computer" => new Pages.SzamitogepHozzaadasa(),
                "Remove Computer" => new Pages.ComputerRemove(),
                "Remove Program" => new Pages.RemoveProgram(),
                "Stop Program Copy" => new Pages.StopProgramCopy(),
                "Modify Computer" => new Pages.ModifyComputer(),
                "Run program copy" => new Pages.NewProgramCopy(),
                _ => new Pages.Monitor()
            };
        }
    }
}