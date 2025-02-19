using ClusterWPF.ViewModels;
using ConsoleApp1;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Collections.Generic;
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
using ClusterWPF.ViewModels;

namespace ClusterWPF.Pages
{
    /// <summary>
    /// Interaction logic for Charts.xaml
    /// </summary>
    public partial class Charts : Page
    {
        public Charts( Cluster cluster)
        {
            InitializeComponent();
            SetChartDatas(cluster);
        }
        private void SetChartDatas(Cluster cluster)
        {
            List<double> memoryPercentages = new List<double>();
            List<double> processorPercentages = new List<double>();
            List<double> averagePercentages = new List<double>();
            List<string> names = new List<string>();

            cluster.Instances.ForEach(x => {
                memoryPercentages.Add(Math.Round(x.MemoryUsagePercentage * 100, 1));
                processorPercentages.Add(Math.Round(x.ProcessorUsagePercentage * 100,1));
                averagePercentages.Add(Math.Round(((x.MemoryUsagePercentage + x.ProcessorUsagePercentage) / 2.0) * 100,1));
                names.Add(x.Name);
            });

            Dispatcher.Invoke(() =>
            {
                if (chartView.DataContext is ViewModel viewModel)
                {
                    viewModel.UpdateChartData(memoryPercentages, processorPercentages, averagePercentages, names);
                }
            });
        }
    }
}
