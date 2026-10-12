#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlBeeAbstract.Constants;
using ControlBeeWPF.Utils;
using JetBrains.Annotations;
using Xunit;

namespace ControlBeeWPF.Tests.Utils;

[TestSubject(typeof(HoldToJogBehavior))]
public class HoldToJogBehaviorTest
{
    [Theory]
    [InlineData(MouseButton.Left, AxisDirection.Positive, false)]
    [InlineData(MouseButton.Right, AxisDirection.Negative, false)]
    [InlineData(MouseButton.Left, AxisDirection.Positive, true)]
    [InlineData(MouseButton.Right, AxisDirection.Negative, true)]
    public void HoldEndStopsJogOnce(
        MouseButton pressedButton,
        AxisDirection expectedDirection,
        bool captureTakenAway
    )
    {
        RunOnSta(() =>
        {
            var (window, jogButton, otherButton) = ShowButtons();
            var startedDirections = new List<AxisDirection>();
            var stopCount = 0;
            HoldToJogBehavior.Attach(
                jogButton,
                AxisDirection.Positive,
                startedDirections.Add,
                () => stopCount++
            );

            Press(jogButton, pressedButton, isDown: true);
            Assert.True(jogButton.IsMouseCaptured);
            if (captureTakenAway)
            {
                otherButton.CaptureMouse();
                Assert.Equal(1, stopCount);
            }
            Press(jogButton, pressedButton, isDown: false);

            Assert.Equal([expectedDirection], startedDirections);
            Assert.Equal(1, stopCount);
            window.Close();
        });
    }

    [Fact]
    public void CaptureWithoutPressSendsNoStop()
    {
        RunOnSta(() =>
        {
            var (window, jogButton, _) = ShowButtons();
            var stopCount = 0;
            HoldToJogBehavior.Attach(
                jogButton,
                AxisDirection.Positive,
                _ => { },
                () => stopCount++
            );

            jogButton.CaptureMouse();
            jogButton.ReleaseMouseCapture();

            Assert.Equal(0, stopCount);
            window.Close();
        });
    }

    private static (Window window, Button jogButton, Button otherButton) ShowButtons()
    {
        var jogButton = new Button();
        var otherButton = new Button();
        var window = new Window
        {
            Content = new StackPanel { Children = { jogButton, otherButton } },
            ShowActivated = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -10000,
            Top = -10000,
        };
        window.Show();
        return (window, jogButton, otherButton);
    }

    private static void Press(Button button, MouseButton mouseButton, bool isDown)
    {
        var routedEvent = (mouseButton, isDown) switch
        {
            (MouseButton.Left, true) => UIElement.PreviewMouseLeftButtonDownEvent,
            (MouseButton.Left, false) => UIElement.PreviewMouseLeftButtonUpEvent,
            (MouseButton.Right, true) => UIElement.PreviewMouseRightButtonDownEvent,
            (MouseButton.Right, false) => UIElement.PreviewMouseRightButtonUpEvent,
            _ => throw new ArgumentOutOfRangeException(nameof(mouseButton)),
        };
        button.RaiseEvent(
            new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, mouseButton)
            {
                RoutedEvent = routedEvent,
            }
        );
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
