using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.Components;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Services;
using ControlBeeWPF.ViewModels;
using log4net;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using String = ControlBee.Variables.String;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VariableStatusBarView.xaml
/// </summary>
public partial class VariableStatusBarView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableStatusBarView));

    private readonly IActor _actor;
    private readonly IViewFactory _viewFactory;
    private readonly string _actorName;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly object[] _subItemPath;
    private readonly IActor _uiActor;
    private object? _value;

    public VariableStatusBarView(
        IViewFactory viewFactory,
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath,
        object[]? subItemPath
    )
    {
        _viewFactory = viewFactory;
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

    public VariableStatusBarView(
        IViewFactory viewFactory,
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath
    )
        : this(viewFactory, actorRegistry, actorName, itemPath, null)
    {
    }

    public double NameWidth
    {
        set => NameColumn.Width = new GridLength(value);
    }

    public double UnitWidth
    {
        set => UnitColumn.Width = new GridLength(value);
    }

    public Brush NameLabelBackGround
    {
        set => NameLabel.Background = value;
    }

    public Brush ValueLabelBackGround
    {
        set => ValueLabel.Background = value;
    }

    public Brush UnitLabelBackGround
    {
        set => UnitLabel.Background = value;
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    private void BinderOnMetaDataChanged(object? sender, Dict e)
    {
        NameLabel.Content = e["Name"];
        var unit = e["Unit"]?.ToString();
        var desc = e["Desc"]?.ToString();

        if (string.IsNullOrEmpty(unit))
            UnitColumn.Width = new GridLength(0);
        else
            UnitLabel.Content = unit;
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
        if (value is bool)
        {
            ValueColumn.Width = new GridLength(0);
            BoolValueRect.Fill = _value is true ? Brushes.LawnGreen : Brushes.WhiteSmoke;
        }
        else
        {
            BinaryValueColumn.Width = new GridLength(0);
            ValueLabel.Content = _value.ToString();
        }
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
            if (curValue is IIndex1D index1D)
            {
                if (pathPart is int index)
                    curValue = index1D.GetValue(index);
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

    private void ToggleBoolValue(bool booleanValue)
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
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_value == null)
            return;
        if (_value is bool boolValue)
        {
            ToggleBoolValue(boolValue);
            return;
        }

        var newValue = "";
        if (_value is string stringValue)
        {
            var inputBox = new InputBox();
            if (inputBox.ShowDialog() is not true)
                return;
            newValue = inputBox.ResponseText;
        }
        else
        {
            var initialValue = _value.ToString() ?? "0";
            var allowDecimal = _value is double;
            var inputBox = (NumpadView)_viewFactory.CreateWindow(typeof(NumpadView), initialValue, allowDecimal);
            if (inputBox.ShowDialog() is not true)
                return;
            newValue = inputBox.Value;
            newValue = newValue.Replace(",", "");
        }


        try
        {
            switch (_value)
            {
                case string value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, newValue)
                        )
                    );
                    break;
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

    private void BoolValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_value is not bool boolValue)
            return;
        ToggleBoolValue(boolValue);
    }
}