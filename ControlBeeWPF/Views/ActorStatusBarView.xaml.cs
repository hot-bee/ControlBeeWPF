using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Utils;
using log4net;
using Brushes = System.Windows.Media.Brushes;
using Message = ControlBee.Models.Message;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

// ReSharper disable once InconsistentNaming
public partial class ActorStatusBarView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );

    private readonly string _actorName;
    private readonly Guid _propertyReadId;
    private readonly string _statusPath;
    private readonly string[] _statusPathKeys;
    private readonly IUiActor _uiActor;

    private object? _value;

    public ActorStatusBarView(IActorRegistry actorRegistry, string actorName, string statusPath)
    {
        InitializeComponent();
        var actor = actorRegistry.Get(actorName)!;
        _actorName = actorName;
        _statusPath = statusPath;
        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;
        statusPath = statusPath.Trim('/');
        _statusPathKeys = statusPath.Split('/');
        var propertyPath = string.Join('/', "Status", statusPath);
        _propertyReadId = actor.Send(new Message(_uiActor, "_propertyRead", propertyPath));
        NameLabel.Content = _statusPathKeys[^1];
    }

    private object? Value
    {
        get => _value;
        set
        {
            if (_value == value)
                return;
            _value = value;
            OnDataChanged();
        }
    }

    public void Dispose()
    {
        _uiActor.MessageArrived -= UiActorOnMessageArrived;
    }

    private void UiActorOnMessageArrived(object? sender, Message message)
    {
        switch (message.Name)
        {
            case "_status":
                if (message.Sender.Name == _actorName)
                {
                    var item = DictPath.Start(message.DictPayload);
                    foreach (var key in _statusPathKeys)
                        item = DictPath.Start(message.DictPayload)[key];

                    Value = item.Value;
                }

                break;
            case "_property":
                if (message.RequestId == _propertyReadId)
                {
                    var propertyValue = message.DictPayload;
                    if (propertyValue == null)
                    {
                        Logger.Warn(
                            $"Payload of `_property` is null. ({_actorName}, {_statusPath})"
                        );
                        break;
                    }

                    NameLabel.Content = propertyValue.GetValueOrDefault("Name");
                    var desc = propertyValue.GetValueOrDefault("Desc") as string;
                    if (!string.IsNullOrEmpty(desc))
                        ToolTip = desc;
                }

                break;
        }
    }

    private void OnDataChanged()
    {
        if (Value is bool value)
        {
            ValueLabel.Visibility = Visibility.Collapsed;
            BoolValueRect.Fill = value ? Brushes.Gold : Brushes.WhiteSmoke;
        }
        else
        {
            BoolValueLabel.Visibility = Visibility.Collapsed;
            ValueLabel.Content = Value?.ToString();
        }
    }
}
