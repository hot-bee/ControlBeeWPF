using ControlBee.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class TeachingJogViewFactory(IActorRegistry actorRegistry)
{
    public TeachingJogView Create(string actorName, string positionItemPath, object[] location)
    {
        var viewModel = new TeachingJogViewModel(actorName, positionItemPath, location, actorRegistry);
        var view = new TeachingJogView(viewModel);
        return view;
    }
}
