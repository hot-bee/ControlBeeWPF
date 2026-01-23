namespace ControlBeeWPF.Services;

public sealed class InitializationExcludedActors(IReadOnlyList<string> value)
{
    public IReadOnlyList<string> Value { get; } = value;
}
