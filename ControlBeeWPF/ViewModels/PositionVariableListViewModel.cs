using System.Collections.ObjectModel;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBeeWPF.Models;

namespace ControlBeeWPF.ViewModels;

public sealed class PositionVariableListViewModel(IActorRegistry actorRegistry)
{
    private readonly IUiActor _uiActor = (IUiActor)actorRegistry.Get("Ui")!;

    public readonly record struct Row(
        string RowName,
        IReadOnlyList<PositionVariableItemKey> PositionVariableItemKeys,
        IReadOnlyList<AxisStatusViewModel> AxisStatusViewModels,
        IReadOnlyList<VariableViewModel> VariableViewModels,
        IReadOnlyList<string> AxisItemPaths
    );

    public ObservableCollection<Row> Rows { get; } = [];

    public void AddVariable(
        string rowName,
        IEnumerable<PositionVariableItemKey> positionVariableItemKeys
    )
    {
        var keys = positionVariableItemKeys.ToList();
        var variableViewModels = new ObservableCollection<VariableViewModel>();
        var axisStatusViewModels = new ObservableCollection<AxisStatusViewModel>();
        var axisItemPaths = new ObservableCollection<string>();

        foreach (var key in keys)
        {
            axisStatusViewModels.Add(
                new AxisStatusViewModel(actorRegistry, key.ActorName, key.AxisItemPath)
            );
            variableViewModels.Add(
                new VariableViewModel(actorRegistry, key.ActorName, key.ItemPath, key.SubItemPath)
            );
            axisItemPaths.Add(key.AxisItemPath);
        }

        Rows.Add(new Row(rowName, keys, axisStatusViewModels, variableViewModels, axisItemPaths));
    }

    public void SetPosition(Row row)
    {
        foreach (var positionVariableItemKey in row.PositionVariableItemKeys)
        {
            var actor = actorRegistry.Get(positionVariableItemKey.ActorName);
            actor?.Send(
                new VariableActorItemMessage(
                    _uiActor,
                    positionVariableItemKey.ItemPath,
                    positionVariableItemKey.SubItemPath,
                    "SetPos"
                )
            );
        }
    }
}
