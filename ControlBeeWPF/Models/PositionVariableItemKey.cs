namespace ControlBeeWPF.Models;

public record PositionVariableItemKey(
    string ActorName,
    string ItemPath,
    object[]? SubItemPath,
    string AxisItemPath
) : ActorItemKey(ActorName, ItemPath, SubItemPath);
