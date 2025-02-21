using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBee.Models;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.ViewModels;

public class TeachingJogViewModel : ObservableObject
{
    private readonly IActor _actor;
    private readonly IActor _uiActor;
    public readonly string[] AxisItemPaths;
    public readonly double[] StepJogSizes;

    public TeachingJogViewModel(
        string actorName,
        string positionItemPath,
        IActorRegistry actorRegistry
    )
    {
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("ui")!;

        var axisItemPaths = new List<string>();
        foreach (var (itemPath, type) in _actor.GetItems())
            if (type.IsAssignableTo(typeof(IAxis)))
                axisItemPaths.Add(itemPath);

        AxisItemPaths = axisItemPaths.ToArray();
        StepJogSizes = _actor.GetStepJogSizes();
    }

    public void ContinuousMoveStart(string axisItemPath, AxisDirection direction, int speedIndex)
    {
        _actor.Send(
            new Message(
                _uiActor,
                "_jogStart",
                new Dict
                {
                    ["AxisItemPath"] = axisItemPath,
                    ["Direction"] = direction,
                    ["JogSpeed"] = (JogSpeed)speedIndex,
                }
            )
        );
    }

    public void ContinuousMoveStop(string axisItemPath)
    {
        _actor.Send(
            new Message(_uiActor, "_jogStop", new Dict { ["AxisItemPath"] = axisItemPath })
        );
    }
}
