using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;

namespace Monitoring.ViewModels;

public partial class GraphViewModel: ObservableObject
{
    [ObservableProperty] public string nama = "";
    public ObservableCollection<ISeries> CpuLoad { get; set; }
    public ObservableCollection<ISeries> RamLoad { get; set; }
    private ObservableCollection<ObservableValue> cpuLoad { get; } = new();
    private ObservableCollection<ObservableValue> ramLoad { get; } = new();
    public GraphViewModel(string name)
    {
        Nama = name;
        var gradient = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(
                new[] { SKColors.Green, SKColors.Orange, SKColors.Red }, // The Colors
                new SKPoint(0.5f, 1), new SKPoint(0.5f, 0)  // Vertical Gradient
            );
        CpuLoad = new ObservableCollection<ISeries>
        {
            new ColumnSeries<ObservableValue>
            {
                Name = "CPU Load",
                Values = cpuLoad,
                Fill = gradient,
                Stroke = null,
                MaxBarWidth = double.MaxValue,
                Padding = 0,
            }
        };
        RamLoad = new ObservableCollection<ISeries>
        {
            new ColumnSeries<ObservableValue>
            {
                Name = "RAM Load",
                Values = ramLoad,
                Fill = gradient,
                Stroke = null,
                MaxBarWidth = double.MaxValue,
                Padding = 0,
            }
        };
    }

    public void AddData(double cpuload, double ramload)
    {
        // only maintain 20 data
        if (cpuLoad.Count > 20) cpuLoad.RemoveAt(0);
        cpuLoad.Add(new(cpuload));

        // only maintain 20 data
        if (ramLoad.Count > 20) ramLoad.RemoveAt(0);
        ramLoad.Add(new(ramload));
    }
    public Axis[] YAxes { get; set; }
        = new Axis[]
        {
            new Axis
            {
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 100,
            }
        };
    public Axis[] XAxes { get; set; } 
        = new Axis[]
        {
            new Axis
            {
                IsVisible = false
            }
        };

    public LiveChartsCore.Measure.Margin Margin { get; set; } = new LiveChartsCore.Measure.Margin() {
            Top=0, Bottom=5, 
            Left=LiveChartsCore.Measure.Margin.Auto, 
            Right=LiveChartsCore.Measure.Margin.Auto, 
        };
}