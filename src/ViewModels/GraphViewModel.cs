using System;
using System.Collections.Generic;
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
    public int MaxData = 30;
    [ObservableProperty] public string nama = "";
    public ObservableCollection<ISeries> CpuLoad { get; set; }
    public ObservableCollection<ISeries> RamLoad { get; set; }
    public ObservableCollection<ISeries> GpuLoad { get; set; }
    private List<ObservableCollection<ObservableValue>> loadList = new();
    private ObservableCollection<ObservableValue> cpuLoad { get; } = new();
    private ObservableCollection<ObservableValue> ramLoad { get; } = new();
    private ObservableCollection<ObservableValue> gpuLoad { get; } = new();
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
        loadList.Add(cpuLoad);
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
        loadList.Add(ramLoad);
        GpuLoad = new ObservableCollection<ISeries>
        {
            new ColumnSeries<ObservableValue>
            {
                Name = "GPU Load",
                Values = gpuLoad,
                Fill = gradient,
                Stroke = null,
                MaxBarWidth = double.MaxValue,
                Padding = 0,
            }
        };
        loadList.Add(gpuLoad);
    }
    /// <summary>
    /// Value of data
    /// </summary>
    /// <param name="load">cpu load, ram load, gpu load</param>
    public void AddData(params double[] load)
    {
        for (int i=load.Length-1; i>=0; i--) 
        {
            if (loadList[i].Count > MaxData) loadList[i].RemoveAt(0);
            loadList[i].Add(new(load[i]));
        }
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