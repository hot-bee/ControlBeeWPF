using System.Windows.Controls;
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IOStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class DigitalInputStatusBarView : UserControl, IDisposable
{
    private readonly ActorItemBinder _binder;
    private bool? _value;

    public DigitalInputStatusBarView(IActorRegistry actorRegistry, string actorName, string itemPath)
    {
        InitializeComponent();
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
        _value = (bool)e["IsOn"]!;
        ValueRect.Fill = _value is true ? Brushes.OrangeRed : Brushes.WhiteSmoke;
    }
}
