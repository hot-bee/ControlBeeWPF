using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;

namespace ControlBeeWPF.Services;

public class TeachingJogViewFactory(IActorRegistry actorRegistry, IDialogService dialogService)
{
    public TeachingJogView Create(string actorName, string positionItemPath, object[] location)
    {
        var viewModel = new TeachingJogViewModel(actorName, positionItemPath, location, actorRegistry, dialogService);
        var view = new TeachingJogView(viewModel);
        return view;
    }
}
