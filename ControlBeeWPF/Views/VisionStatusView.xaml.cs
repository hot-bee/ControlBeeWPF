using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VisionStatusView.xaml
/// </summary>
public partial class VisionStatusView : UserControl
{
    private readonly VisionStatusViewModel _viewModel;

    public VisionStatusView(VisionStatusViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        UpdateUi();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(VisionStatusViewModel.IsConnected):
                UpdateUi();
                break;
        }
    }

    private void UpdateUi()
    {
        StatusText.Text = _viewModel.IsConnected ? "Connected" : "Disconnected";
        StatusText.Background = _viewModel.IsConnected
            ? Brushes.GreenYellow
            : Brushes.PaleVioletRed;
        StatusText.Foreground = _viewModel.IsConnected ? Brushes.Black : Brushes.White;
    }
}
