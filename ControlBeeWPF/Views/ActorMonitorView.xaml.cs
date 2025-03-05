using System.Windows.Controls;
using System.Windows.Input;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for ActorMonitorView.xaml
/// </summary>
public partial class ActorMonitorView : UserControl
{
    private readonly ActorMonitorViewModel _viewModel;

    public ActorMonitorView(ActorMonitorViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGridRow row)
            return;
        var index = row.GetIndex();
        _viewModel.Poke(index);
    }
}
