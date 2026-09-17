using System.ComponentModel;
using System.Runtime.CompilerServices;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.ViewModels;

public class DoubleActingActuatorViewModel : IDisposable, INotifyPropertyChanged
{
    private readonly IActor _actor;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly IActor _uiActor;
    private bool _commandOn;
    private string _name = "";
    private bool _offDetect;
    private bool _onDetect;
    private string _toolTip = "";

    public DoubleActingActuatorViewModel(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath
    )
    {
        _itemPath = itemPath;
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
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

    public bool CommandOn
    {
        get => _commandOn;
        set => SetField(ref _commandOn, value);
    }

    public bool OnDetect
    {
        get => _onDetect;
        set => SetField(ref _onDetect, value);
    }

    public bool OffDetect
    {
        get => _offDetect;
        set => SetField(ref _offDetect, value);
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RequestMetaData()
    {
        _binder.RequestMetaData();
    }

    public void RequestData()
    {
        _binder.RequestData();
    }

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        Name = (string)e["Name"]!;
        ToolTip = (string)e["Desc"]!;
        if (string.IsNullOrEmpty(ToolTip))
            ToolTip = Name;
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        CommandOn = e["CommandOn"] is true;
        OnDetect = e["OnDetect"] is true;
        OffDetect = e["OffDetect"] is true;
    }

    public void ToggleValue()
    {
        SetValue(!CommandOn);
    }

    public void SetValue(bool value)
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                _itemPath,
                "_itemDataWrite",
                new Dictionary<string, object?> { ["On"] = value }
            )
        );
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
