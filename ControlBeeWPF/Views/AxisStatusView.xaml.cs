using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlBeeWPF.ViewModels;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for IOStatusBar.xaml
/// </summary>
// ReSharper disable once InconsistentNaming
public partial class AxisStatusView : UserControl, IDisposable
{
    private readonly AxisStatusViewModel _viewModel;

    public AxisStatusView(AxisStatusViewModel viewModel, bool shortMode, int index)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        IndexLabel.Content = $"{index}";
        viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        if (shortMode)
        {
            // PositionColumn.Width = new GridLength(0);
            JogColumn.Width = new GridLength(0);
            // TODO: Gray area should be fixed.
        }
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
                InitLabel.Background = _viewModel.IsInitializing
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
