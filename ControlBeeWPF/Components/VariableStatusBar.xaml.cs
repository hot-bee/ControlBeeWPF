﻿using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using ControlBee.Exceptions;
using ControlBee.Interfaces;
using ControlBee.Models;
using log4net;

namespace ControlBeeWPF.Components;

/// <summary>
///     Interaction logic for VariableStatusBar.xaml
/// </summary>
public partial class VariableStatusBar : UserControl
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );
    private readonly IActor _actor;
    private readonly IActorRegistry _actorRegistry;
    private readonly string _itemPath;
    private readonly IActor _uiActor;
    private object? _value;

    public VariableStatusBar(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        _actorRegistry = actorRegistry;
        _itemPath = itemPath;
        InitializeComponent();
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("ui")!;
        var binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        binder.MetaDataChanged += BinderOnMetaDataChanged;
        binder.DataChanged += Binder_DataChanged;
    }

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        NameLabel.Content = e["Name"];
        UnitLabel.Content = e["Unit"];
        var desc = e["Desc"]?.ToString();
        if (!string.IsNullOrEmpty(desc))
            ToolTip = desc;
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        _value = e["NewValue"];
        ValueLabel.Content = _value?.ToString() ?? string.Empty;
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var inputBox = new InputBox();
        if (inputBox.ShowDialog() is not true)
            return;
        var newValue = inputBox.ResponseText;
        try
        {
            switch (_value)
            {
                case int value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            int.Parse(newValue)
                        )
                    );
                    break;
                case double value:
                    _actor.Send(
                        new ActorItemMessage(
                            _uiActor,
                            _itemPath,
                            "_itemDataWrite",
                            double.Parse(newValue)
                        )
                    );
                    break;
                default:
                    throw new ValueError();
            }
        }
        catch (FormatException error)
        {
            Logger.Error(error);
        }
    }
}
