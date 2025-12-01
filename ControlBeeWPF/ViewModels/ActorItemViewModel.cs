using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Variables;

namespace ControlBeeWPF.ViewModels;

public partial class ActorItemViewModel : ObservableObject
{
    [ObservableProperty] private bool _visible = true;
    public string Name { get; set; } = string.Empty;
    public string ItemPath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Type? Type { get; set; }
    public object? Value { get; set; }
    public VariableScope? Scope { get; set; }
}