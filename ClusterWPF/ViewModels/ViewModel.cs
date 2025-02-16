using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace ClusterWPF.ViewModels;

public partial class ViewModel : ObservableObject
{
    public ISeries[] Series { get; set; } = [
        new ColumnSeries<double>
        {
            Values = [2, 5, 4, 3],
            IsVisible = true
        },
        new ColumnSeries<double>
        {
            Values = [6, 3, 2, 8],
            IsVisible = true
        },
        new ColumnSeries<double>
        {
            Values = [4, 2, 8, 7],
            IsVisible = true
        }
    ];

    // Define custom X-axis labels
    public Axis[] XAxes { get; set; } =
    [
        new Axis
        {
            Labels = ["PC1", "PC2", "PC3", "PC4"], // Custom labels for 4 data points
            LabelsRotation = 0, // Optional: Rotate labels (0 degrees)
            SeparatorsAtCenter = false, // Hide separators (grid lines at the center)
            ForceStepToMin = true // Ensure all labels are shown
        }
    ];

    [RelayCommand]
    public void ToggleSeries0() => Series[0].IsVisible = !Series[0].IsVisible;

    [RelayCommand]
    public void ToggleSeries1() => Series[1].IsVisible = !Series[1].IsVisible;

    [RelayCommand]
    public void ToggleSeries2() => Series[2].IsVisible = !Series[2].IsVisible;
}