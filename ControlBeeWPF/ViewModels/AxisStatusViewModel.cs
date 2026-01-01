using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using ControlBee.Models;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.ViewModels;

public partial class AxisStatusViewModel : ObservableObject, IDisposable
{
    private readonly IActor _actor;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly IActor _uiActor;

    [ObservableProperty]
    private double _actualPosition;

    [ObservableProperty]
    private double _commandPosition;

    [ObservableProperty]
    private string _desc = string.Empty;

    [ObservableProperty]
    private bool _isAlarmed;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private bool _isHomeDet;

    [ObservableProperty]
    private bool _isInitializing;

    [ObservableProperty]
    private bool _isMoving;

    [ObservableProperty]
    private bool _isNegativeLimitDet;

    [ObservableProperty]
    private bool _isPositiveLimitDet;

    [ObservableProperty]
    private string _name = string.Empty;

    public AxisStatusViewModel(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        _itemPath = itemPath;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;

        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    private void BinderOnMetaDataChanged(object? sender, Dict e)
    {
        Name = e["Name"] as string ?? _itemPath;
        Desc = e["Desc"] as string ?? string.Empty;
    }

    private void Binder_DataChanged(object? sender, Dict e)
    {
        CommandPosition = (double)e[nameof(CommandPosition)]!;
        ActualPosition = (double)e[nameof(ActualPosition)]!;
        IsMoving = (bool)e[nameof(IsMoving)]!;
        IsAlarmed = (bool)e[nameof(IsAlarmed)]!;
        IsEnabled = (bool)e[nameof(IsEnabled)]!;
        IsInitializing = (bool)e[nameof(IsInitializing)]!;
        IsHomeDet = (bool)e[nameof(IsHomeDet)]!;
        IsNegativeLimitDet = (bool)e[nameof(IsNegativeLimitDet)]!;
        IsPositiveLimitDet = (bool)e[nameof(IsPositiveLimitDet)]!;
    }

    [RelayCommand]
    private void SwitchEnable()
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                _itemPath,
                "_itemDataWrite",
                new Dict { ["Enable"] = !IsEnabled }
            )
        );
    }

    [RelayCommand]
    private void Initialize()
    {
        _actor.Send(new ActorItemMessage(_uiActor, _itemPath, "_initialize"));
    }
}
