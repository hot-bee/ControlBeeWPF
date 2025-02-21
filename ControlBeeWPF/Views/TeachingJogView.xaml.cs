using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ControlBee.Constants;
using ControlBee.Exceptions;
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
    private readonly Dictionary<Button, string> _buttons = new();
    private readonly TeachingJogViewModel _viewModel;

    public TeachingJogView(TeachingJogViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        ContinuousAxesContent.Content = UpdateAxesButtons(JogMode.Continuous);
        DiscreteAxesContent.Content = UpdateAxesButtons(JogMode.Discrete);
        SetupDiscreteStepOptions();
    }

    public void Dispose()
    {
        foreach (var button in _buttons.Keys)
        {
            button.MouseLeftButtonDown -= NegButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonDown -= PosButtonOnMouseLeftButtonDown;
            button.MouseLeftButtonUp -= ButtonOnMouseLeftButtonUp;
        }
    }

    private void SetupDiscreteStepOptions()
    {
        DiscreteStepOptions.Children.Clear();
        List<RadioButton> radios = [];
        foreach (var step in _viewModel.StepJogSizes)
        {
            if (DiscreteStepOptions.Children.Count > 0)
                DiscreteStepOptions.Children.Add(new Rectangle { Width = 100 });
            var radioButton = new RadioButton
            {
                Content = $"{step}",
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            radios.Add(radioButton);
            DiscreteStepOptions.Children.Add(radioButton);
        }

        radios[1].IsChecked = true;
    }

    private UIElement UpdateAxesButtons(JogMode jogMode)
    {
        var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        foreach (var itemPath in _viewModel.AxisItemPaths)
        {
            if (stackPanel.Children.Count > 0)
                stackPanel.Children.Add(new Rectangle { Height = 20 });

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
            stackPanel.Children.Add(
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        negButton,
                        new Label
                        {
                            Content = itemPath,
                            Width = 80,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        },
                        posButton,
                    },
                }
            );
            negButton.PreviewMouseLeftButtonDown += NegButtonOnMouseLeftButtonDown;
            negButton.PreviewMouseLeftButtonUp += ButtonOnMouseLeftButtonUp;
            posButton.PreviewMouseLeftButtonDown += PosButtonOnMouseLeftButtonDown;
            posButton.PreviewMouseLeftButtonUp += ButtonOnMouseLeftButtonUp;
            _buttons[negButton] = itemPath;
            _buttons[posButton] = itemPath;
        }

        return stackPanel;
    }

    private void NegButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Down.");
        var itemPath = _buttons[(Button)sender];
        _viewModel.ContinuousMoveStart(itemPath, AxisDirection.Negative, GetSpeedIndex());
    }

    private void PosButtonOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Down.");
        var itemPath = _buttons[(Button)sender];
        _viewModel.ContinuousMoveStart(itemPath, AxisDirection.Positive, GetSpeedIndex());
    }

    private void ButtonOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Logger.Debug("Left Button Up.");
        var itemPath = _buttons[(Button)sender];
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
