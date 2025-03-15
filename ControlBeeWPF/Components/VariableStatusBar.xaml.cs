using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlBee.Exceptions;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
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
    private readonly object[] _subItemPath;
    private readonly IActor _uiActor;
    private object? _value;

    public VariableStatusBar(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath,
        object[]? subItemPath
    )
    {
        _actorName = actorName;
        _itemPath = itemPath;
        _subItemPath = subItemPath ?? [];
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

    private void BinderOnMetaDataChanged(object? sender, Dict e)
    {
        NameLabel.Content = e["Name"];
        UnitLabel.Content = e["Unit"];
        var desc = e["Desc"]?.ToString();
        if (!string.IsNullOrEmpty(desc))
            ToolTip = desc;
    }

    private void Binder_DataChanged(object? sender, Dict e)
    {
        var valueChangedArgs = e[nameof(ValueChangedArgs)] as ValueChangedArgs;
        var location = valueChangedArgs?.Location!;
        var newValue = valueChangedArgs?.NewValue!;
        var value = GetValue(location, newValue);
        if (value == null)
            return;
        _value = value;
        ValueLabel.Content = _value.ToString();
    }

    private object? GetValue(object[] location, object newValue)
    {
        var paths = _subItemPath.ToArray();
        foreach (var o in location)
            if (paths[0].Equals(o))
                paths = paths[1..];
            else
                return null;

        var curValue = newValue;
        foreach (var pathPart in paths)
            if (curValue is IArray1D array1D)
            {
                if (pathPart is int index)
                    curValue = array1D.GetValue(index);
                else
                    return null;
            }
            else
            {
                if (pathPart is string propertyName)
                {
                    var propertyInfo = curValue?.GetType().GetProperty(propertyName);
                    if (propertyInfo == null)
                    {
                        Logger.Warn($"PropertyInfo is null. ({_actorName}, {_itemPath})");
                        curValue = null;
                        break;
                    }

                    curValue = propertyInfo.GetValue(curValue);
                }
                else
                {
                    return null;
                }
            }

        return curValue;
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
                _actor.Send(
                    new ActorItemMessage(
                        _uiActor,
                        _itemPath,
                        "_itemDataWrite",
                        new ItemDataWriteArgs(_subItemPath, !booleanValue)
                    )
                );
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
                            new ItemDataWriteArgs(_subItemPath, int.Parse(newValue))
                        )
                    );
                    break;
                case double value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, double.Parse(newValue))
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
