using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Interfaces;

public interface IViewFactory
{
    UserControl? Create(Type viewType, params object?[]? args);
    T? Create<T>(params object?[]? args)
        where T : class;
}
