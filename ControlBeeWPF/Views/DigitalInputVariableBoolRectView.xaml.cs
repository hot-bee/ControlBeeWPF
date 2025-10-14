using System.ComponentModel;
using ControlBeeWPF.ViewModels;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace ControlBeeWPF.Views;

public partial class DigitalInputVariableBoolRectView : INotifyPropertyChanged
{
    private readonly DigitalInputViewModel _digitalInputViewModel;
    private readonly VariableViewModel _variableViewModel;

    public event PropertyChangedEventHandler? PropertyChanged;

    public DigitalInputVariableBoolRectView(DigitalInputViewModel digitalInputViewModel, VariableViewModel variableViewModel)
    {
        InitializeComponent();

        _digitalInputViewModel = digitalInputViewModel;
        _variableViewModel = variableViewModel;

        _digitalInputViewModel.PropertyChanged += OnDigitalInputPropertyChanged;
        _variableViewModel.PropertyChanged += OnVariablePropertyChanged;

        DataContext = this;
    }

    private void OnVariablePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VariableViewModel.Value))
            OnPropertyChanged(nameof(VariableBrush));
    }

    private void OnDigitalInputPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DigitalInputViewModel.Value))
            OnPropertyChanged(nameof(DigitalInputBrush));
    }

    public Brush DigitalInputBrush =>
        _digitalInputViewModel.Value is true ? Brushes.Red : Brushes.LightGray;

    public Brush VariableBrush =>
        _variableViewModel.Value is true ? Brushes.LawnGreen : Brushes.White;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}