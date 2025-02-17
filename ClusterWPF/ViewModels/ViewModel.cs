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
            Values = [2, 5, 4, 3, 5, 6, 3, 2, 8, 5, 7, 3, 9, 2, 6, 4, 5, 8, 1, 7],
            IsVisible = true
        },
        new ColumnSeries<double>
        {
            Values = [6, 3, 2, 8, 5, 6, 3, 2, 8, 5, 4, 7, 5, 6, 9, 2, 3, 8, 5, 7],
            IsVisible = true
        },
        new ColumnSeries<double>
        {
            Values = [4, 2, 8, 7, 5, 6, 3, 2, 8, 5, 9, 3, 4, 6, 2, 7, 3, 5, 6, 8],
            IsVisible = true
        }
    ];

    // Define custom X-axis labels (updated with 10 new labels)
    public Axis[] XAxes { get; set; } =
    [
        new Axis
        {
            Labels = ["PC1", "PC2", "PC3", "PC4", "PC5", "PC6", "PC7", "PC8", "PC9", "PC10", "PC11", "PC12", "PC13", "PC14", "PC15", "PC16", "PC17", "PC18", "PC19", "PC20"], // Added 10 new labels
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

    public double ChartWidth => XAxes[0].Labels.Count * 100;
}
