using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlBeeWPF.Components;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using log4net;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VariableStatusBarView.xaml
/// </summary>
public partial class VariableStatusBarView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableStatusBarView));

    public static readonly DependencyProperty NameColumnWidthProperty =
        DependencyProperty.Register(
            nameof(NameColumnWidth),
            typeof(GridLength),
            typeof(VariableStatusBarView),
            new PropertyMetadata(new GridLength(1, GridUnitType.Auto)));

    public static readonly DependencyProperty UnitColumnWidthProperty =
        DependencyProperty.Register(
            nameof(UnitColumnWidth),
            typeof(GridLength),
            typeof(VariableStatusBarView),
            new PropertyMetadata(new GridLength(1, GridUnitType.Auto)));

    private readonly IViewFactory _viewFactory;
    private readonly VariableViewModel _viewModel;
    private Action<VariableStatusBarView>? _clickAction;
    private string? _nameSuffix;
    private string? _overrideName;

    public VariableStatusBarView(IViewFactory viewFactory, VariableViewModel viewModel)
    {
        _viewFactory = viewFactory;
        _viewModel = viewModel;
        InitializeComponent();
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    public GridLength NameColumnWidth
    {
        get => (GridLength)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    public GridLength UnitColumnWidth
    {
        get => (GridLength)GetValue(UnitColumnWidthProperty);
        set => SetValue(UnitColumnWidthProperty, value);
    }

    public Brush NameLabelBackGround
    {
        set => NameLabel.Background = value;
    }

    public Brush ValueLabelBackGround
    {
        set => ValueLabel.Background = value;
    }

    public Brush UnitLabelBackGround
    {
        set => UnitLabel.Background = value;
    }

    public string? OverrideName
    {
        get => _overrideName;
        set
        {
            _overrideName = value;
            UpdateName();
        }
    }

    public string? NameSuffix
    {
        get => _nameSuffix;
        set
        {
            _nameSuffix = value;
            UpdateName();
        }
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private void UpdateName()
    {
        var name = OverrideName ?? _viewModel.Name;
        name += NameSuffix ?? "";
        NameLabel.Content = name;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.Name):
                UpdateName();
                break;
            case nameof(_viewModel.Unit):
                if (string.IsNullOrEmpty(_viewModel.Unit))
                {
                    UnitColumn.Width = new GridLength(0);
                    break;
                }

                UnitLabel.Content = _viewModel.Unit;
                break;
            case nameof(_viewModel.Value):
                if (_viewModel.Value is bool)
                {
                    ValueColumn.Width = new GridLength(0);
                    BoolValueRect.Fill = _viewModel.Value is true ? Brushes.LawnGreen : Brushes.WhiteSmoke;
                }
                else
                {
                    BinaryValueColumn.Width = new GridLength(0);
                    ValueLabel.Content = _viewModel.Value?.ToString();
                }

                break;
        }
    }

    public void SetClickAction(Action<VariableStatusBarView> clickAction)
    {
        _clickAction = clickAction;
    }


    private void ToggleBoolValue(bool booleanValue)
    {
        if (
            MessageBox.Show(
                "Do you want to turn this on/off?",
                "Change value",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes
        )
            _viewModel.ToggleBoolValue(booleanValue);
    }

    private void ValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.Value == null)
            return;

        if (_clickAction != null)
        {
            _clickAction(this);
            return;
        }

        if (_viewModel.Value is bool boolValue)
        {
            ToggleBoolValue(boolValue);
            return;
        }

        var newValue = "";
        if (_viewModel.Value is string stringValue)
        {
            var inputBox = new InputBox();
            if (inputBox.ShowDialog() is not true)
                return;
            newValue = inputBox.ResponseText;
        }
        else
        {
            var initialValue = _viewModel.Value.ToString() ?? "0";
            var allowDecimal = _viewModel.Value is double;
            var inputBox = (NumpadView)_viewFactory.CreateWindow(typeof(NumpadView), initialValue, allowDecimal);
            if (inputBox.ShowDialog() is not true)
                return;
            newValue = inputBox.Value;
            newValue = newValue.Replace(",", "");
        }

        _viewModel.ChangeValue(newValue);
    }

    private void BoolValueLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.Value is not bool boolValue)
            return;
        if (_clickAction != null)
        {
            _clickAction(this);
            return;
        }

        ToggleBoolValue(boolValue);
    }

    public void ChangeValue(string newValue)
    {
        _viewModel.ChangeValue(newValue);
    }
}