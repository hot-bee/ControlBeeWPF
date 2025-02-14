using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IOStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class AxisStatusView : UserControl, IDisposable
{
    private readonly AxisStatusViewModel _viewModel;

    public AxisStatusView(AxisStatusViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.Desc):
                if (string.IsNullOrEmpty(_viewModel.Desc))
                    break;
                NameLabel.ToolTip = _viewModel.Desc;
                break;
            case nameof(_viewModel.IsMoving):
                MovingLabel.Background = _viewModel.IsMoving
                    ? Brushes.GreenYellow
                    : Brushes.WhiteSmoke;
                break;
            case nameof(_viewModel.IsAlarmed):
                AlarmLabel.Background = _viewModel.IsAlarmed ? Brushes.Pink : Brushes.White;
                break;
            case nameof(_viewModel.IsEnabled):
                EnableGrid.Background = _viewModel.IsEnabled
                    ? Brushes.GreenYellow
                    : Brushes.WhiteSmoke;
                break;
            case nameof(_viewModel.IsInitializing):
                AlarmLabel.Background = _viewModel.IsInitializing
                    ? Brushes.GreenYellow
                    : Brushes.White;
                break;
            case nameof(_viewModel.IsNegativeLimitDet):
                NegativeLimitLabel.Background = _viewModel.IsNegativeLimitDet
                    ? Brushes.OrangeRed
                    : Brushes.White;
                break;
            case nameof(_viewModel.IsHomeDet):
                HomeSensorLabel.Background = _viewModel.IsHomeDet
                    ? Brushes.OrangeRed
                    : Brushes.White;
                break;
            case nameof(_viewModel.IsPositiveLimitDet):
                PositiveLimitLabel.Background = _viewModel.IsPositiveLimitDet
                    ? Brushes.OrangeRed
                    : Brushes.White;
                break;
        }
    }
}
