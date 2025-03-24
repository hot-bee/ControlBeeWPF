using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using ControlBeeAbstract.Devices;
using log4net;

namespace ControlBeeWPF.ViewModels;

public partial class VisionStatusViewModel : ObservableObject
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VisionStatusViewModel));
    private readonly IVisionDevice? _device;
    private readonly string _visionDeviceName;

    private bool _isConnected;

    public VisionStatusViewModel(string visionDeviceName, IDeviceManager deviceManager)
    {
        _visionDeviceName = visionDeviceName;
        _device = deviceManager.Get(visionDeviceName) as IVisionDevice;
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }

        _device.VisionConnected += DeviceOnVisionConnected;
        _device.VisionDisconnected += DeviceOnVisionDisconnected;
        IsConnected = _device.IsConnected();
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            SetProperty(ref _isConnected, value);
            OnPropertyChanged(nameof(CanConnect));
        }
    }

    public bool CanConnect => !IsConnected;

    private void DeviceOnVisionDisconnected(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => IsConnected = false)
        );
    }

    private void DeviceOnVisionConnected(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => IsConnected = true)
        );
    }

    [RelayCommand]
    private void Connect()
    {
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }

        _device.Connect();
    }
}
