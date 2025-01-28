using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.Components;

/// <summary>
///     Interaction logic for DoubleActingActuatorStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class DoubleActingActuatorStatusBar : UserControl
{
    private readonly IActor _actor;
    private readonly string _itemPath;
    private readonly IActor _uiActor;
    private bool? _value;

    public DoubleActingActuatorStatusBar(
        IActorRegistry actorRegistry,
        string actorName,
        string itemPath
    )
    {
        InitializeComponent();
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
        ToolTip = e["Desc"];
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        _value = (bool)e["On"]!;
        InputOffRect.Fill = e["OffDetect"] is true ? Brushes.OrangeRed : Brushes.RosyBrown;
        InputOnRect.Fill = e["OnDetect"] is true ? Brushes.OrangeRed : Brushes.RosyBrown;
        ValueRect.Fill = _value is true ? Brushes.LawnGreen : Brushes.DarkOliveGreen;
    }

    private void ValueRect_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var newValue = _value is null or false;
        _actor.Send(
            new ActorItemMessage(
                _uiActor,
                _itemPath,
                "_itemDataWrite",
                new Dictionary<string, object?> { ["On"] = newValue }
            )
        );
    }
}
