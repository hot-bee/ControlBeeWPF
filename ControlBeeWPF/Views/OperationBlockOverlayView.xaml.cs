using System.Windows;
using System.Windows.Input;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using Message = ControlBee.Models.Message;
using MessageBox = System.Windows.MessageBox;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for OperationBlockOverlayView.xaml
/// </summary>
public partial class OperationBlockOverlayView
{
    private readonly string _password;
    private readonly IActor _syncer;
    private readonly IActor _ui;
    private readonly IViewFactory _viewFactory;

    public OperationBlockOverlayView(
        IActorRegistry actorRegistry,
        IViewFactory viewFactory,
        Dict arguments
    )
    {
        _viewFactory = viewFactory;
        _syncer = actorRegistry.Get("Syncer")!;
        _ui = actorRegistry.Get("Ui")!;

        InitializeComponent();

        Owner = (Window)arguments["Owner"]!;
        JobNameLabel.Text = arguments["Job"]!.ToString();
        _password = (string)arguments["Password"]!;

        Loaded += (_, __) => ApplyOverlayBounds();
        Closed += (_, _) => Owner.Activate();
    }

    private void ApplyOverlayBounds()
    {
        if (Owner == null)
            return;

        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = Owner.Left;
        Top = Owner.Top;
        Width = Owner.ActualWidth;
        Height = Owner.ActualHeight;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender.Equals(CloseButton))
        {
            if (Password.Password == _password)
            {
                _syncer.Send(new Message(_ui, "UnblockOperation"));
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Incorrect password. Please try again.",
                    "Password Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                Password.Clear();
            }
        }
    }

    private void Password_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var keyboardView = _viewFactory.Create<KeyboardView>()!;
        if (keyboardView.ShowDialog() is true)
            Password.Password = keyboardView.Value;
    }
}
