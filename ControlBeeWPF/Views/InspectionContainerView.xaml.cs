using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using Button = System.Windows.Controls.Button;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;
using Panel = System.Windows.Forms.Panel;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for InspectionContainerView.xaml
/// </summary>
public partial class InspectionContainerView : IRefreshable, INotifyPropertyChanged
{
    private readonly string _mode;
    private readonly Dict _options;
    private readonly IUiActor _uiActor;
    private readonly VisionStatusViewModel _viewModel;
    private bool _autoRunning;

    public InspectionContainerView(
        VisionStatusViewModel viewModel,
        ISystemConfigurations systemConfigurations,
        IActorRegistry actorRegistry,
        Dict options
    )
    {
        _viewModel = viewModel;
        _options = options;
        InitializeComponent();
        HostControl.Child = new Panel();
        Loaded += OnLoaded;

        _mode = (string)_options["Mode"]!;
        var showChannelTabs = _options.GetValueOrDefault("ShowChannelTabs") as bool? ?? true;
        var channelCount = systemConfigurations.VisionChannelCount;
        if (_mode == "VisionFrame" && showChannelTabs && 1 < channelCount)
        {
            var allButton = new Button { Content = "All", Tag = true };
            allButton.Click += (sender, args) =>
            {
                ActiveChannel = -1;
                SetActiveButton((Button)sender!);
                Refresh();
            };
            ChannelPanel.Children.Add(allButton);

            for (var channel = 0; channel < channelCount; channel++)
            {
                var button = new Button { Content = $"Ch. {channel}", Tag = false };
                var channel1 = channel;
                button.Click += (sender, args) =>
                {
                    ActiveChannel = channel1;
                    SetActiveButton((Button)sender!);
                    Refresh();
                };
                ChannelPanel.Children.Add(button);
            }
        }

        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;
    }

    public int ActiveChannel
    {
        get => _options.GetValueOrDefault("Channel") as int? ?? -1;
        private set
        {
            _options["Channel"] = value;
            OnPropertyChanged();
        }
    }

    public IntPtr HostHandle => HostControl.Child.Handle;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh()
    {
        _viewModel.EmbedVisionCommand.Execute((HostHandle, _options));
        if (
            _mode == "VisionFrame"
            && !_autoRunning
            && _options.GetValueOrDefault("UseContinuous") is not false
            && _options.GetValueOrDefault("Channel") is int channel
        )
            _viewModel.StartContinuousCommand.Execute(channel);
    }

    private void UiActorOnMessageArrived(object? sender, Message message)
    {
        if (message.Name == "_status" && message.ActorName == "Syncer")
            _autoRunning = message.DictPayload!.GetValueOrDefault("_auto") is true;
    }

    private void SetActiveButton(Button selected)
    {
        foreach (var button in ChannelPanel.Children.OfType<Button>())
            button.Tag = ReferenceEquals(button, selected);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Refresh();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
