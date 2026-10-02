using ControlBeeAbstract.Constants;
using log4net;
using Button = System.Windows.Controls.Button;

namespace ControlBeeWPF.Utils;

public static class HoldToJogBehavior
{
    private static readonly ILog Logger = LogManager.GetLogger(nameof(HoldToJogBehavior));

    public static void Attach(
        Button button,
        AxisDirection direction,
        Action<AxisDirection> startJog,
        Action stopJog
    )
    {
        var jogging = false;

        button.PreviewMouseLeftButtonDown += (_, _) => StartJog(direction);
        button.PreviewMouseRightButtonDown += (_, _) =>
            StartJog((AxisDirection)((int)direction * -1));
        button.PreviewMouseLeftButtonUp += (_, _) => button.ReleaseMouseCapture();
        button.PreviewMouseRightButtonUp += (_, _) => button.ReleaseMouseCapture();
        button.LostMouseCapture += (_, _) => StopJog();

        void StartJog(AxisDirection jogDirection)
        {
            // Button captures the mouse by itself for the left button only.
            var captured = button.CaptureMouse();
            if (!captured)
            {
                Logger.Warn($"Jog not started: {button.Name} could not capture the mouse.");
                return;
            }

            jogging = true;
            startJog(jogDirection);
        }

        // Button also captures the mouse for the Space key, and _jogStop stops the axis
        // even when no jog is running.
        void StopJog()
        {
            if (jogging)
            {
                jogging = false;
                stopJog();
            }
        }
    }
}
