using ControlBeeWPF.Views;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ControlBeeWPF.Services;

public class ViewFactory(IServiceProvider serviceProvider)
{
    public virtual UserControl Create(Type viewType, params object?[]? args)
    {
        if (viewType == typeof(JogView))
        {
            var actorName = (string)args![0]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new JogViewModel(actorName, actorRegistry);
            var view = new JogView(viewModel);
            return view;
        }

        throw new ValueError();
    }
}