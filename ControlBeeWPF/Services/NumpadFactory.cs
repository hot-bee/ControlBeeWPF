using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class NumpadFactory
{
    public NumpadView Create(string initialValue)
    {
        var viewModel = new NumpadViewModel(initialValue);
        var view = new NumpadView(viewModel);
        return view;
    }
}
