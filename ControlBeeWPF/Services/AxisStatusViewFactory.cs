using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class AxisStatusViewFactory(IActorRegistry actorRegistry)
{
    public AxisStatusView Create(string actorName, string itemPath, int index)
    {
        var viewModel = new AxisStatusViewModel(actorRegistry, actorName, itemPath, index);
        var view = new AxisStatusView(viewModel);
        return view;
    }
}
