using System.Windows.Controls;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for ActorMonitorView.xaml
/// </summary>
public partial class ActorMonitorView : UserControl
{
    public ActorMonitorView(ActorMonitorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
