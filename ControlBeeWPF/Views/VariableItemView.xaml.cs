using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ControlBeeWPF.Components;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using log4net;
using Brushes = System.Windows.Media.Brushes;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VariableItemView.xaml
/// </summary>
public partial class VariableItemView
{
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(VariableItemView),
        new PropertyMetadata(new CornerRadius(10))
    );

    public static readonly DependencyProperty BorderMarginProperty = DependencyProperty.Register(
        nameof(BorderMargin),
        typeof(Thickness),
        typeof(VariableItemView),
        new PropertyMetadata(new Thickness(2))
    );

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight),
        typeof(double),
        typeof(VariableItemView),
        new PropertyMetadata(30.0)
    );

    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth),
        typeof(double),
        typeof(VariableItemView),
        new PropertyMetadata(90.0)
    );

    public static readonly DependencyProperty ItemFontSizeProperty = DependencyProperty.Register(
        nameof(ItemFontSize),
        typeof(double),
        typeof(VariableItemView),
        new PropertyMetadata(14.0)
    );

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness BorderMargin
    {
        get => (Thickness)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemFontSize
    {
        get => (double)GetValue(ItemFontSizeProperty);
        set => SetValue(ItemFontSizeProperty, value);
    }

    private static readonly ILog Logger = LogManager.GetLogger(nameof(VariableItemView));
    private readonly IViewFactory _viewFactory;
    private readonly VariableViewModel _viewModel;

    public VariableItemView(IViewFactory viewFactory, VariableViewModel viewModel)
    {
        _viewFactory = viewFactory;
        _viewModel = viewModel;
        InitializeComponent();
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        RefreshContent();
    }

    public string? OverrideText { get; set; }
    public Dict? OverrideTextByValue { get; set; }
    public Func<double, double>? DisplayConverter { get; set; }
    public Func<double, double>? InputConverter { get; set; }

    public void Refresh() => RefreshContent();

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(VariableViewModel.Value):
                RefreshContent();
                break;
        }
    }

    private void RefreshContent()
    {
        if (_viewModel.Value is bool value)
        {
            Content = OverrideText ?? (value ? "On" : "Off");
            Background = value ? Brushes.LawnGreen : Brushes.LightGray;
        }
        else
        {
            string valueContent;
            double? numericValue = _viewModel.Value switch
            {
                double doubleValue => doubleValue,
                int intValue => (double)intValue,
                _ => null,
            };
            if (numericValue != null && DisplayConverter != null)
            {
                var converted = DisplayConverter(numericValue.Value);
                valueContent =
                    _viewModel.Value is int
                        ? ((int)Math.Round(converted)).ToString()
                        : converted.ToString();
            }
            else
                valueContent = _viewModel.Value?.ToString() ?? "";
            Content =
                OverrideTextByValue?.GetValueOrDefault(valueContent) as string ?? valueContent;
        }
    }

    private void VariableItemView_OnPreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e
    )
    {
        if (_viewModel.Value == null)
            return;

        if (_viewModel.Value is bool boolValue)
        {
            _viewModel.ChangeValue(!boolValue);
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
            double? numericValue = _viewModel.Value switch
            {
                double doubleValue => doubleValue,
                int intValue => (double)intValue,
                _ => null,
            };

            string initialValue;
            if (DisplayConverter != null && numericValue != null)
            {
                var converted = DisplayConverter(numericValue.Value);
                initialValue =
                    _viewModel.Value is int
                        ? ((int)Math.Round(converted)).ToString()
                        : converted.ToString();
            }
            else
                initialValue = _viewModel.Value?.ToString() ?? "0";

            var allowDecimal = _viewModel.Value is double;
            var inputBox = _viewFactory.Create<NumpadView>(initialValue, allowDecimal);
            if (inputBox!.ShowDialog() is not true)
                return;
            newValue = inputBox.Value;
            newValue = newValue.Replace(",", "");
        }

        if (InputConverter != null && double.TryParse(newValue, out var parsed))
        {
            var converted = InputConverter(parsed);
            _viewModel.ChangeValue(
                _viewModel.Value is int
                    ? ((int)Math.Round(converted)).ToString()
                    : converted.ToString()
            );
        }
        else
            _viewModel.ChangeValue(newValue);
    }
}
