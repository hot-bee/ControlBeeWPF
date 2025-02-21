using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Services;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingAxisStatusView.xaml
/// </summary>
public partial class TeachingAxisStatusView : UserControl
{
    public TeachingAxisStatusView(
        string actorName,
        IActorRegistry actorRegistry,
        AxisStatusViewFactory axisStatusViewFactory
    )
    {
        InitializeComponent();
        var actor = actorRegistry.Get(actorName)!;

        var index = 1;
        foreach (var (itemPath, type) in actor.GetItems())
        {
            if (type.IsAssignableTo(typeof(IAxis)))
            {
                var view = axisStatusViewFactory.Create(actorName, itemPath, index, true);
                AxesPanel.Children.Add(view);
                index++;
            }
        }
    }
}
