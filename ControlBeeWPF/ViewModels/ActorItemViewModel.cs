﻿namespace ControlBeeWPF.ViewModels;

public class ActorItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ItemPath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Type? Type { get; set; }
    public object? Value { get; set; }
}
