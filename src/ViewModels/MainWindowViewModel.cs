using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Monitoring.Models;

namespace Monitoring.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    private Measurement measurement = new();
    public MainWindowViewModel()
    {
        GetMeasurement = new AsyncRelayCommand(GetMeasurementImpl);
    }

    public ICommand GetMeasurement { get; }
    private async Task GetMeasurementImpl()
    {
        await Task.Run(() => measurement.GetMeasurement());
    }
}
