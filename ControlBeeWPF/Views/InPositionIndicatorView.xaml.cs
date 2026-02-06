using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for InPositionIndicatorView.xaml
/// </summary>
public partial class InPositionIndicatorView
{
    public InPositionIndicatorView(InPositionIndicatorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
