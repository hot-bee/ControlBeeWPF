using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class ActorItemExplorerViewFactory(IActorRegistry actorRegistry, IViewFactory viewFactory)
{
    public ActorItemExplorerView Create(string actorName)
    {
        var viewModel = new ActorItemExplorerViewModel(actorName, actorRegistry);
        var view = new ActorItemExplorerView(actorName, viewModel, actorRegistry, viewFactory);
        return view;
    }
}
