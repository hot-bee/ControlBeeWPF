using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ControlBee.Constants;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.ViewModels;
using log4net;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for TeachingJogView.xaml
/// </summary>
public partial class TeachingJogView : UserControl, IDisposable
{
    private static readonly ILog Logger = LogManager.GetLogger(
        MethodBase.GetCurrentMethod()!.DeclaringType!
    );
    private readonly Dictionary<Button, string> _jogButtons = new();
    private readonly TeachingJogViewModel _viewModel;

    public TeachingJogView(TeachingJogViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        viewModel.Loaded += ViewModelOnLoaded;
    }

    private void ViewModelOnLoaded(object? sender, EventArgs e)
    {
        ContinuousAxesContent.Content = UpdateAxesButtons(JogMode.Continuous);
        DiscreteAxesContent.Content = UpdateAxesButtons(JogMode.Discrete);
        SetupDiscreteStepOptions();
    }

    public void Dispose()
    {
        foreach (var button in _jogButtons.Keys)
        {
            button.MouseLeftButtonDown -= NegButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonDown -= PosButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonUp -= ButtonOnMouseLeftButtonUp;
            button.MouseRightButtonDown -= PosButtonOnMouseLeftButtonDown;
            button.MouseRightButtonDown -= NegButtonOnMouseLeftButtonDown;
            button.MouseRightButtonUp -= ButtonOnMouseLeftButtonUp;
        }
    }

    private void SetupDiscreteStepOptions()
    {
        DiscreteStepOptions.Children.Clear();
        var stepSizes = (string[])["Small", "Medium", "Large"];
        foreach (var step in stepSizes)
        {
            if (DiscreteStepOptions.Children.Count > 0)
                DiscreteStepOptions.Children.Add(new Rectangle { Width = 60 });
            var radioButton = new RadioButton
            {
                Content = $"{step}",
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            radioButton.Checked += RadioButtonOnChecked;
            _stepRadios.Add(radioButton);
            DiscreteStepOptions.Children.Add(radioButton);
        }

        _stepRadios[1].IsChecked = true;
    }

    private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
    {
        var stepIndex = _stepRadios.IndexOf((RadioButton)sender);
        foreach (var (itemPath, label) in _axisLabels)
        {
            var step = _viewModel.StepJogSizes[itemPath][stepIndex];
            label.Content = $"{itemPath}\n({step})";
        }
    }

    private readonly Dictionary<string, Label> _axisLabels = [];
    private readonly List<RadioButton> _stepRadios = [];

    private UIElement UpdateAxesButtons(JogMode jogMode)
    {
        var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        foreach (var itemPath in _viewModel.AxisItemPaths)
        {
            var negButton = new Button
            {
                Content = "- Neg",
                Width = 120,
                Height = 60,
                Margin = new Thickness(10),
            };
            var posButton = new Button
            {
                Content = "Pos +",
                Width = 120,
                Height = 60,
                Margin = new Thickness(10),
            };
            var label = new Label
            {
                Content = itemPath,
                Width = 80,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            _axisLabels[itemPath] = label;
            stackPanel.Children.Add(
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { negButton, label, posButton },
                }
            );
            negButton.PreviewMouseLeftButtonDown += NegButtonOnMouseLeftButtonDown;
            negButton.PreviewMouseLeftButtonUp += ButtonOnMouseLeftButtonUp;
            posButton.PreviewMouseLeftButtonDown += PosButtonOnMouseLeftButtonDown;
            posButton.PreviewMouseLeftButtonUp += ButtonOnMouseLeftButtonUp;
            negButton.PreviewMouseRightButtonDown += PosButtonOnMouseLeftButtonDown;
            negButton.PreviewMouseRightButtonUp += ButtonOnMouseLeftButtonUp;
            posButton.PreviewMouseRightButtonDown += NegButtonOnMouseLeftButtonDown;
            posButton.PreviewMouseRightButtonUp += ButtonOnMouseLeftButtonUp;
            _jogButtons[negButton] = itemPath;
            _jogButtons[posButton] = itemPath;
        }

        return stackPanel;
    }

    private void NegButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Down.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStart(itemPath, AxisDirection.Negative, GetSpeedIndex());
    }

    private void PosButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Down.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStart(itemPath, AxisDirection.Positive, GetSpeedIndex());
    }

    private void ButtonOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Up.");
        var itemPath = _jogButtons[(Button)sender];
        _viewModel.ContinuousMoveStop(itemPath);
    }

    private int GetSpeedIndex()
    {
        int speedIndex;
        if (SpeedLowRadio.IsChecked is true)
            speedIndex = 0;
        else if (SpeedMidRadio.IsChecked is true)
            speedIndex = 1;
        else if (SpeedHighRadio.IsChecked is true)
            speedIndex = 2;
        else
            throw new ValueError();
        return speedIndex;
    }

    private enum JogMode
    {
        Continuous,
        Discrete,
    }
}
