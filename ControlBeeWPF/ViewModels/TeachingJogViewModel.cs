using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBeeAbstract.Constants;
using ControlBeeWPF.Interfaces;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;

namespace ControlBeeWPF.ViewModels;

public partial class TeachingJogViewModel : ObservableObject
{
    private readonly IActor _actor;
    private readonly IDialogService _dialogService;
    private readonly string _positionItemPath;
    private readonly object[] _location;
    private readonly Dictionary<Guid, string> _sentIds = [];
    private readonly IUiActor _uiActor;
    public readonly string[] AxisItemPaths;
    public readonly Dictionary<string, double[]> StepJogSizes = new();

    public TeachingJogViewModel(
        string actorName,
        string positionItemPath,
        object[] location,
        IActorRegistry actorRegistry,
        IDialogService dialogService
    )
    {
        _positionItemPath = positionItemPath;
        _location = location;
        _dialogService = dialogService;
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = (IUiActor)actorRegistry.Get("Ui")!;
        _uiActor.MessageArrived += UiActorOnMessageArrived;

        var axisItemPaths = new List<string>();
        foreach (var (itemPath, type) in _actor.GetItems())
            if (type.IsAssignableTo(typeof(IAxis)))
            {
                axisItemPaths.Add(itemPath);
                var id = _actor.Send(new ActorItemMessage(_uiActor, itemPath, "_getStepJogSizes"));
                _sentIds[id] = itemPath;
            }

        AxisItemPaths = axisItemPaths.ToArray();
    }

    private void UiActorOnMessageArrived(object? sender, Message e)
    {
        switch (e.Name)
        {
            case "_stepJogSizes":
                if (_sentIds.TryGetValue(e.RequestId, out var itemPath))
                {
                    var sizes = (double[])e.Payload!;
                    StepJogSizes[itemPath] = sizes;
                    if (StepJogSizes.Count == _sentIds.Count)
                        OnLoaded();
                }

                break;
        }
    }

    public event EventHandler? Loaded;

    public void StepMoveStart(string axisItemPath, AxisDirection direction, JogStep jogStep)
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                axisItemPath,
                "_jogStart",
                new Dict
                {
                    ["Type"] = "Step",
                    ["Direction"] = direction,
                    ["JogStep"] = jogStep,
                }
            )
        );
    }

    public void ContinuousMoveStart(
        string axisItemPath,
        AxisDirection direction,
        JogSpeedLevel jogSpeedLevel
    )
    {
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                axisItemPath,
                "_jogStart",
                new Dict
                {
                    ["Type"] = "Continuous",
                    ["Direction"] = direction,
                    ["JogSpeed"] = jogSpeedLevel,
                }
            )
        );
    }

    public void ContinuousMoveStop(string axisItemPath)
    {
        _actor.Send(new ActorItemMessage(_uiActor, axisItemPath, "_jogStop"));
    }

    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void MoveToHomePos()
    {
        _actor.Send(
            new VariableActorItemMessage(_uiActor, _positionItemPath, _location, "MoveToHomePos")
        );
    }

    [RelayCommand]
    private void MoveToSavedPos()
    {
        _actor.Send(
            new VariableActorItemMessage(_uiActor, _positionItemPath, _location, "MoveToSavedPos")
        );
    }

    [RelayCommand]
    private void SetPos()
    {
        if (
            _dialogService.Confirm(
                $"Do you want to set {_positionItemPath}",
                "Confirm setting position"
            )
        )
            _actor.Send(
                new VariableActorItemMessage(_uiActor, _positionItemPath, _location, "SetPos")
            );
    }

    [RelayCommand]
    private void RestorePos()
    {
        // TODO
    }
}
