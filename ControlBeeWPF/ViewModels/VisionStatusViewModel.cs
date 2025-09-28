using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using ControlBeeAbstract.Devices;
using log4net;
using Application = System.Windows.Application;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.ViewModels;

public partial class VisionStatusViewModel : ObservableObject
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VisionStatusViewModel));
    private readonly IVisionDevice? _device;
    private readonly string _visionDeviceName;

    [ObservableProperty] private int _channel;

    [ObservableProperty] private int _inspIndex;

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
            OnPropertyChanged(nameof(CanTrigger));
        }
    }

    public bool CanConnect => !IsConnected;
    public bool CanTrigger => IsConnected;

    private void DeviceOnVisionDisconnected(object? sender, EventArgs e)
    {
        // TODO: Null pointer exceptions sometimes happen.
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

    [RelayCommand]
    private void Trigger()
    {
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }

        try
        {
            _device.Trigger(Channel, InspIndex);
            _device.Wait(Channel, InspIndex, 5000);
            var result = _device.GetResult(Channel, InspIndex);
            var resultString = result.ToString();
            Logger.Info(resultString);
        }
        catch(Exception error)
        {
            Logger.Error(error.Message);
        }
    }

    [RelayCommand]
    private void EmbedVision((IntPtr parentHandle, Dict options) param)
    {
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }
        _device.EmbedVisionView(param.parentHandle, param.options);
    }

    [RelayCommand]
    private void StartContinuous(int channel)
    {
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }
        _device.StartContinuous(channel);
    }
    [RelayCommand]
    private void StopContinuous(int channel)
    {
        if (null == _device)
        {
            Logger.Error($"Cannot find a vision device from {_visionDeviceName}.");
            return;
        }
        _device.StopContinuous(channel);
    }
}