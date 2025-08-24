using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class TeachingViewFactory(
    IActorRegistry actorRegistry,
    TeachingJogViewFactory teachingJogViewFactory,
    IViewFactory viewFactory
)
{
    public TeachingView Create(string actorName)
    {
        var viewModel = new TeachingViewModel(actorName, actorRegistry);
        var view = new TeachingView(
            actorName,
            viewModel,
            this,
            teachingJogViewFactory,
            viewFactory
        );
        return view;
    }

    public TeachingDataView CreateData(string actorName, string itemPath, object[] location)
    {
        var viewModel = new TeachingDataViewModel(actorName, itemPath, location, actorRegistry);
        var view = new TeachingDataView(viewModel);
        return view;
    }
}
