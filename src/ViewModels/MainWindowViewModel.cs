using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Monitoring.Models;
using ReactiveUI;

namespace Monitoring.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    public ObservableCollection<GraphViewModel> Graph { get; } = new();
    private Measurement measurement = new();
    private IDisposable? running;
    public MainWindowViewModel()
    {
        GetMeasurement = new RelayCommand<string>(GetMeasurementImpl);
        Graph.Add(new("RAM Load"));
        Graph.Add(new("CPU Load"));
    }

    public ICommand GetMeasurement { get; }
    private void GetMeasurementImpl(string? cmd)
    {
        if (String.IsNullOrWhiteSpace(cmd)) return;
        // stop yang lama dulu
        running?.Dispose();
        running = cmd switch
        {
            "Start" => Observable.Interval(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
                                .SubscribeOn(RxApp.MainThreadScheduler)
                                .Subscribe(_ =>
                                {
                                    measurement.GetMeasurement(out double load, out double ram);
                                    Graph[0].AddData(load, ram);
                                    // Graph[0].AddData(ram);
                                    // Graph[1].AddData(load);
                                }),
            _ => null,
        };
    }
}
