using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Interfaces;

public interface IViewFactory
{
    UserControl Create(Type viewType, params object?[]? args);
    Window CreateWindow(Type viewType, params object?[]? args);
}