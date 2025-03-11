using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlBee.Exceptions;
using ControlBee.Interfaces;
using ControlBee.Models;
using log4net;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.Components;

/// <summary>
///     Interaction logic for VariableStatusBar.xaml
/// </summary>
public partial class VariableStatusBar : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableStatusBar));

    private readonly IActor _actor;
    private readonly string _actorName;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly string? _propertyName;
    private readonly IActor _uiActor;
    private object? _value;

    public VariableStatusBar(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath,
        string? propertyName
    )
    {
        _actorName = actorName;
        _itemPath = itemPath;
        _propertyName = propertyName;
        InitializeComponent();
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
    }

    public VariableStatusBar(IActorRegistry actorRegistry, string actorName, string itemPath)
        : this(actorRegistry, actorName, itemPath, null) { }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        NameLabel.Content = e["Name"];
        UnitLabel.Content = e["Unit"];
        var desc = e["Desc"]?.ToString();
        if (!string.IsNullOrEmpty(desc))
            ToolTip = desc;
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        var location = e["Location"];
        var newValue = e["NewValue"];
        if (_propertyName != null)
        {
            if (location == null)
            {
                if (newValue == null)
                {
                    Logger.Warn($"NewValue is null. ({_actorName}, {_itemPath})");
                }
                else
                {
                    var propertyInfo = newValue.GetType().GetProperty(_propertyName);
                    if (propertyInfo == null)
                        Logger.Warn($"PropertyInfo is null. ({_actorName}, {_itemPath})");
                    else
                        _value = propertyInfo.GetValue(newValue);
                }
            }
            else
            {
                if (_propertyName.Equals(location))
                    _value = newValue;
            }
        }
        else
        {
            _value = newValue;
        }

        ValueLabel.Content = _value?.ToString() ?? string.Empty;
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_value is bool booleanValue)
        {
            if (
                MessageBox.Show(
                    "Do you want to turn this on/off?",
                    "Change value",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes
            )
            {
                if (_propertyName == null)
                    _actor.Send(
                        new ActorItemMessage(_uiActor, _itemPath, "_itemDataWrite", !booleanValue)
                    );
                else
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataModify",
                            new Dict { [_propertyName] = !booleanValue }
                        )
                    );
            }

            return;
        }

        var inputBox = new InputBox();
        if (inputBox.ShowDialog() is not true)
            return;
        var newValue = inputBox.ResponseText;
        try
        {
            switch (_value)
            {
                case int value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            int.Parse(newValue)
                        )
                    );
                    break;
                case double value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            double.Parse(newValue)
                        )
                    );
                    break;
                default:
                    throw new ValueError();
            }
        }
        catch (FormatException error)
        {
            Logger.Error(error);
        }
    }
}
