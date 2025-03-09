using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.Components;

/// <summary>
///     Interaction logic for DigitalOutputStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class DigitalOutputStatusBar : UserControl, IDisposable
{
    private readonly IActor _actor;
    private readonly ActorItemBinder _binder;
    private readonly string _itemPath;
    private readonly IActor _uiActor;
    private bool? _value;

    public DigitalOutputStatusBar(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        InitializeComponent();
        _itemPath = itemPath;
        _actor = actorRegistry.Get(actorName)!;
        _uiActor = actorRegistry.Get("Ui")!;
        _binder = new ActorItemBinder(actorRegistry, actorName, itemPath);
        _binder.MetaDataChanged += BinderOnMetaDataChanged;
        _binder.DataChanged += Binder_DataChanged;
    }

    public void Dispose()
    {
        _binder.MetaDataChanged -= BinderOnMetaDataChanged;
        _binder.DataChanged -= Binder_DataChanged;
        _binder.Dispose();
    }

    private void BinderOnMetaDataChanged(object? sender, Dictionary<string, object?> e)
    {
        NameLabel.Content = e["Name"];
        ToolTip = e["Desc"];
    }

    private void Binder_DataChanged(object? sender, Dictionary<string, object?> e)
    {
        _value = (bool)e["On"]!;
        ValueRect.Fill = _value is true ? Brushes.LawnGreen : Brushes.WhiteSmoke;
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
