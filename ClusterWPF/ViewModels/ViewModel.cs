using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ClusterWPF.ViewModels;

public partial class ViewModel : ObservableObject
{
    // Use ObservableProperty to auto-implement INotifyPropertyChanged
    [ObservableProperty]
    private ObservableCollection<ISeries> _series;

    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes;

    [ObservableProperty]
    private Axis[] _yAxes;

    public ViewModel()
    {
        var brush = (SolidColorBrush)System.Windows.Application.Current.Resources["ChartLabelBrush"];
        var skColor = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);

        _series = new ObservableCollection<ISeries>();
        _xAxes = new ObservableCollection<Axis>();

        _yAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0, 
                MaxLimit = 100,
                Name = "Usage (%)",
                NamePaint = new SolidColorPaint(skColor),
                LabelsPaint = new SolidColorPaint(skColor)
            }
        };

    }

    // Method to update chart data
    public void UpdateChartData(
        List<double> memoryPercentages,
        List<double> processorPercentages,
        List<double> averagePercentages,
        List<string> labels)
    {
        // Clear existing data
        Series.Clear();
        Series.Add(new ColumnSeries<double> { Values = memoryPercentages, IsVisible = true });
        Series.Add(new ColumnSeries<double> { Values = processorPercentages, IsVisible = true });
        Series.Add(new ColumnSeries<double> { Values = averagePercentages, IsVisible = true });

        // Clear and update XAxes
        XAxes.Clear();

        var brush = (SolidColorBrush)System.Windows.Application.Current.Resources["ChartLabelBrush"];
        var skColor = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);

        XAxes.Add(new Axis
        {
            LabelsPaint = new SolidColorPaint(skColor),
            NamePaint = new SolidColorPaint(skColor),
            Labels = labels,
            LabelsRotation = 0,
            SeparatorsAtCenter = false,
            ForceStepToMin = true
        });

        // Explicitly notify changes
        OnPropertyChanged(nameof(Series));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(ChartWidth));
    }


    // Calculate chart width (200 pixels per label)
    public double ChartWidth => XAxes.Count() > 0 ? XAxes[0].Labels.Count * 200 : 0;

    [RelayCommand]
    private void ToggleSeries0() => Series[0].IsVisible = !Series[0].IsVisible;

    [RelayCommand]
    private void ToggleSeries1() => Series[1].IsVisible = !Series[1].IsVisible;

    [RelayCommand]
    private void ToggleSeries2() => Series[2].IsVisible = !Series[2].IsVisible;
}