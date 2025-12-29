using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeAbstract.Exceptions;
using log4net;
using Dict = System.Collections.Generic.Dictionary<string, object?>;


namespace ControlBeeWPF.ViewModels;

public class VariableViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableViewModel));

    private readonly IActor _actor;
    private readonly string _actorName;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly object[] _subItemPath;
    private readonly IActor _uiActor;
    private string? _maxValue;
    private string? _minValue;
    private string _name = "";
    private object? _oldValue;
    private string? _toolTip;
    private string? _unit;
    private object? _value;

    public VariableViewModel(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath,
        object[]? subItemPath)
    {
        _actorName = actorName;
        _itemPath = itemPath;
        _subItemPath = subItemPath ?? [];
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;

        try
        {
            _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
            _binder.MetaDataChanged += BinderOnMetaDataChanged;
            _binder.DataChanged += Binder_DataChanged;
            _binder.ErrorOccurred += BinderOnErrorOccurred;
        }
        catch (ArgumentException)
        {
            HasBindingFailed = true;
            Logger.Error($"ItemPath must start with '/'. (actor='{actorName}', itemPath='{itemPath}')");
        }
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

    public object? OldValue
    {
        get => _oldValue;
        private set => SetField(ref _oldValue, value);
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

    public string? MinValue
    {
        get => _minValue;
        private set => SetField(ref _minValue, value);
    }

    public string? MaxValue
    {
        get => _maxValue;
        private set => SetField(ref _maxValue, value);
    }

    public bool HasBindingFailed { get; private set; }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<string>? WriteFailed;

    private void BinderOnMetaDataChanged(object? sender, Dict e)
    {
        var name = e["Name"] as string;
        var unit = e["Unit"]?.ToString();
        var desc = e["Desc"]?.ToString();
        var min = e["MinValue"]?.ToString();
        var max = e["MaxValue"]?.ToString();
        if (!string.IsNullOrEmpty(name)) Name = name;
        Unit = unit ?? "";
        ToolTip = desc ?? "";
        MinValue = min ?? null;
        MaxValue = max ?? null;
    }

    private void Binder_DataChanged(object? sender, Dict e)
    {
        var valueChangedArgs = e[nameof(ValueChangedArgs)] as ValueChangedArgs;
        var location = valueChangedArgs?.Location!;

        var newValueArg = valueChangedArgs?.NewValue!;
        var newValue = GetValue(location, newValueArg);
        if (newValue != null) Value = newValue;

        var oldValueArg = valueChangedArgs?.OldValue;
        if (oldValueArg != null)
        {
            var oldValue = GetValue(location, oldValueArg);
            if (oldValue != null) OldValue = oldValue;
        }
    }

    private void BinderOnErrorOccurred(object? sender, Dict e)
    {
        if (e.TryGetValue("ErrorMessage", out var errorObject) &&
            errorObject is string errorMessage)
            WriteFailed?.Invoke(this, errorMessage);
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
            else if (curValue is IIndex2D index2D)
            {
                if (pathPart is (int index1, int index2))
                    curValue = index2D.GetValue(index1, index2);
                else
                    return null;
            }
            else if (curValue is IIndex3D index3D)
            {
                if (pathPart is (int index1, int index2, int index3))
                    curValue = index3D.GetValue(index1, index2, index3);
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

    public void ChangeValue(bool newValue)
    {
        ChangeValue(newValue.ToString());
    }

    public void ChangeValue(string newValue)
    {
        try
        {
            switch (Value)
            {
                case string value:
                {
                    if (value == newValue) return;
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, newValue)
                        )
                    );
                    break;
                }
                case int value:
                {
                    var parsedValue = int.Parse(newValue);
                    if (value == parsedValue) return;
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, int.Parse(newValue))
                        )
                    );
                    break;
                }
                case double value:
                {
                    var parsedValue = double.Parse(newValue);
                    if (value == parsedValue) return;
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, double.Parse(newValue))
                        )
                    );
                    break;
                }
                case bool value:
                {
                    var parsedValue = bool.Parse(newValue);
                    if (value == parsedValue) return;
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            new ItemDataWriteArgs(_subItemPath, parsedValue)
                        )
                    );
                    break;
                }
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