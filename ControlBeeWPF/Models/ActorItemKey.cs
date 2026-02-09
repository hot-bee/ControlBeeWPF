namespace ControlBeeWPF.Models;

public record ActorItemKey(string ActorName, string ItemPath, object[]? SubItemPath = null)
{
    public object[] SubItemPath { get; init; } = SubItemPath ?? [];
}
