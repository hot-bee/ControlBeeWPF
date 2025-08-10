using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.Views;
using log4net;
using Dict = System.Collections.Generic.Dictionary<string, object?>;


namespace ControlBeeWPF.ViewModels;

public class VariableViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableStatusBarView));

    private readonly IActor _actor;
    private readonly string _actorName;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly object[] _subItemPath;
    private readonly IActor _uiActor;
    private string _name = "";
    private string? _toolTip = "";
    private string? _unit = "";
    private object? _value;

    public VariableViewModel(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath,
        object[]? subItemPath
    )
    {
        _actorName = actorName;
        _itemPath = itemPath;
        _subItemPath = subItemPath ?? [];

        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
    }

    public string Name
    {
        get => _name;
        private set => SetField(ref _name, value);
    }

    public object? Value
    {
        get => _value;
        private set => SetField(ref _value, value);
    }

    public string? Unit
    {
        get => _unit;
        private set => SetField(ref _unit, value);
    }

    public string? ToolTip
    {
        get => _toolTip;
        private set => SetField(ref _toolTip, value);
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void BinderOnMetaDataChanged(object? sender, Dict e)
    {
        var name = e["Name"] as string;
        var unit = e["Unit"]?.ToString();
        var desc = e["Desc"]?.ToString();
        if (!string.IsNullOrEmpty(name)) Name = name;
        Unit = unit ?? "";
        ToolTip = desc ?? "";
    }

    private void Binder_DataChanged(object? sender, Dict e)
    {
        var valueChangedArgs = e[nameof(ValueChangedArgs)] as ValueChangedArgs;
        var location = valueChangedArgs?.Location!;
        var newValue = valueChangedArgs?.NewValue!;
        var value = GetValue(location, newValue);
        if (value == null)
        {
            Logger.Error("Couldn't find the value.");
            return;
        }

        Value = value;
    }

    private object? GetValue(object[] location, object newValue)
    {
        var paths = _subItemPath.ToArray();
        foreach (var o in location)
            if (paths.Length > 0 && paths[0].Equals(o))
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

    public void ToggleBoolValue(bool booleanValue)
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                _itemPath,
                "_itemDataWrite",
                new ItemDataWriteArgs(_subItemPath, !booleanValue)
            )
        );
    }

    public void ChangeValue(string newValue)
    {
        try
        {
            switch (Value)
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}