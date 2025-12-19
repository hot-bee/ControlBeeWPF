using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.ViewModels;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;

namespace ControlBeeWPF.Views;

public partial class DigitalInputVariableBoolRectView : INotifyPropertyChanged
{
    private readonly DigitalInputViewModel _digitalInputViewModel;
    private readonly VariableViewModel _variableViewModel;

    public DigitalInputVariableBoolRectView(DigitalInputViewModel digitalInputViewModel,
        VariableViewModel variableViewModel)
    {
        InitializeComponent();

        _digitalInputViewModel = digitalInputViewModel;
        _variableViewModel = variableViewModel;

        _digitalInputViewModel.PropertyChanged += OnDigitalInputPropertyChanged;
        _variableViewModel.PropertyChanged += OnVariablePropertyChanged;

        DataContext = this;
    }

    public double VariableRectSize
    {
        get => (double)GetValue(VariableRectSizeProperty);
        set => SetValue(VariableRectSizeProperty, value);
    }

    public static readonly DependencyProperty VariableRectSizeProperty =
        DependencyProperty.Register(nameof(VariableRectSize), typeof(double), typeof(DigitalInputVariableBoolRectView),
            new PropertyMetadata(20.0));

    public double InputRectSize
    {
        get => (double)GetValue(InputRectSizeProperty);
        set => SetValue(InputRectSizeProperty, value);
    }

    public static readonly DependencyProperty InputRectSizeProperty =
        DependencyProperty.Register(nameof(InputRectSize), typeof(double), typeof(DigitalInputVariableBoolRectView),
            new PropertyMetadata(10.0));

    public Brush DigitalInputBrush =>
        _digitalInputViewModel.Value is true ? Brushes.Red : Brushes.LightGray;

    public Brush VariableBrush =>
        _variableViewModel.Value is true ? Brushes.LawnGreen : Brushes.White;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_variableViewModel.Value is not bool boolValue)
            return;

        if (
            MessageBox.Show(
                "Do you want to turn this on/off?",
                "Change value",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes
        )
            _variableViewModel.ChangeValue(!boolValue);
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}