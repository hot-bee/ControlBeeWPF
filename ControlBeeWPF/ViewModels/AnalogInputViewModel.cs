using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.ViewModels;

public class AnalogInputViewModel : IDisposable, INotifyPropertyChanged
{
    private readonly ActorItemBinder _binder;
    private string _name = "";
    private string _toolTip = "";
    private long? _value;

    public AnalogInputViewModel(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
    }

    public long? Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string ToolTip
    {
        get => _toolTip;
        set => SetField(ref _toolTip, value);
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        Name = (string)e["Name"]!;
        ToolTip = (string)e["Desc"]!;
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        Value = (long)e["Data"]!;
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