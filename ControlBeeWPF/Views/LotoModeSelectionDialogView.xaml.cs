using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for LotoModeSelectionDialogView.xaml
/// </summary>
public partial class LotoModeSelectionDialogView
{
    public enum LotoModeSelection
    {
        Repair,
        OnWorking,
        RestTime,
    }

    public LotoModeSelection SelectedLotoMode { get; private set; } = LotoModeSelection.OnWorking;

    public LotoModeSelectionDialogView()
    {
        InitializeComponent();
        UpdateButtonVisuals();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender.Equals(RepairButton))
            SelectedLotoMode = LotoModeSelection.Repair;
        else if (sender.Equals(OnWorkingButton))
            SelectedLotoMode = LotoModeSelection.OnWorking;
        else if (sender.Equals(RestTimeButton))
            SelectedLotoMode = LotoModeSelection.RestTime;
        else if (sender.Equals(OkButton))
        {
            DialogResult = true;
            return;
        }
        else if (sender.Equals(CancelButton))
        {
            DialogResult = false;
            return;
        }

        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        RepairButton.Opacity = 0.7;
        OnWorkingButton.Opacity = 0.7;
        RestTimeButton.Opacity = 0.7;

        RepairButton.BorderBrush = Brushes.Transparent;
        OnWorkingButton.BorderBrush = Brushes.Transparent;
        RestTimeButton.BorderBrush = Brushes.Transparent;

        RepairButton.BorderThickness = new Thickness(0);
        OnWorkingButton.BorderThickness = new Thickness(0);
        RestTimeButton.BorderThickness = new Thickness(0);

        var selectedButton = SelectedLotoMode switch
        {
            LotoModeSelection.Repair => RepairButton,
            LotoModeSelection.OnWorking => OnWorkingButton,
            LotoModeSelection.RestTime => RestTimeButton,
            _ => OnWorkingButton,
        };

        selectedButton.Opacity = 1.0;
        selectedButton.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0));
        selectedButton.BorderThickness = new Thickness(4);
    }
}
