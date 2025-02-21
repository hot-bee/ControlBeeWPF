using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class TeachingViewFactory(
    IActorRegistry actorRegistry,
    TeachingAxisStatusViewFactory teachingAxisStatusViewFactory,
    TeachingJogViewFactory teachingJogViewFactory
)
{
    public TeachingView Create(string actorName)
    {
        var viewModel = new TeachingViewModel(actorName, actorRegistry);
        var view = new TeachingView(
            actorName,
            viewModel,
            this,
            teachingAxisStatusViewFactory,
            teachingJogViewFactory
        );
        return view;
    }

    public TeachingDataView CreateData(string actorName, string itemPath)
    {
        var viewModel = new TeachingDataViewModel(actorName, itemPath, actorRegistry);
        var view = new TeachingDataView(viewModel);
        return view;
    }
}
