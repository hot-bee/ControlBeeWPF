using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class ActorItemExplorerViewFactory(IActorRegistry actorRegistry)
{
    public ActorItemExplorerView Create(string actorName)
    {
        var viewModel = new ActorItemExplorerViewModel(actorName, actorRegistry);
        var view = new ActorItemExplorerView(actorName, viewModel, actorRegistry);
        return view;
    }
}
