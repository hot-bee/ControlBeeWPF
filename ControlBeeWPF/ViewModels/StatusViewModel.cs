using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Utils;
using log4net;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.ViewModels;

public class StatusViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(StatusViewModel));
    private readonly string _actorName;
    private readonly string[] _statusPathKeys;
    private readonly IUiActor _uiActor;

    private object? _value;

    public StatusViewModel(IActorRegistry actorRegistry, string actorName, string statusPath)
    {
        _actorName = actorName;
        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        var actor = actorRegistry.Get(actorName)!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;
        statusPath = statusPath.Trim('/');
        _statusPathKeys = statusPath.Split('/');
        actor.Send(new Message(_uiActor, "_requestStatus"));
    }

    public object? Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActorOnMessageArrived;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ReadStatusValue(Dict statusDict)
    {
        var item = DictPath.Start(statusDict);
        foreach (var key in _statusPathKeys)
            item = item[key];

        Value = item.Value;
    }

    private void UiActorOnMessageArrived(object? sender, Message message)
    {
        switch (message.Name)
        {
            case "_status":
                if (message.Sender.Name == _actorName)
                    ReadStatusValue(message.DictPayload!);

                break;
        }
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
