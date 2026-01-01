using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingAxisStatusView.xaml
/// </summary>
public partial class TeachingAxisStatusView : UserControl
{
    public TeachingAxisStatusView(
        string actorName,
        IActorRegistry actorRegistry,
        IViewFactory viewFactory
    )
    {
        InitializeComponent();
        var actor = actorRegistry.Get(actorName)!;

        var index = 1;
        foreach (var (itemPath, type) in actor.GetItems())
        {
            if (type.IsAssignableTo(typeof(IAxis)))
            {
                var view = viewFactory.Create(
                    typeof(AxisStatusView),
                    actorName,
                    itemPath,
                    index,
                    true
                );
                AxesPanel.Children.Add(view);
                index++;
            }
        }
    }
}
