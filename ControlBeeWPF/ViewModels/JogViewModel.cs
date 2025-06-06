using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBee.Models;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.ViewModels;

public class JogViewModel : ObservableObject
{
    private readonly IActor _actor;
    private readonly Dictionary<Guid, string> _sentIds = [];
    private readonly IUiActor _uiActor;
    public readonly string[] AxisItemPaths;
    public readonly Dictionary<string, double[]> StepJogSizes = new();

    public JogViewModel(
        string actorName,
        IActorRegistry actorRegistry
    )
    {
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
                    ["JogStep"] = jogStep
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
                    ["JogSpeed"] = jogSpeedLevel
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
}