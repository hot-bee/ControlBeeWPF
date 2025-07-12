using System.Windows;
using System.Windows.Controls;

namespace ControlBeeWPF.Interfaces;

public interface IViewFactory
{
    UserControl Create(Type viewType, params object?[]? args);
    Window CreateWindow(Type viewType, params object?[]? args);
}