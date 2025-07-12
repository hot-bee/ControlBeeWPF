using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;
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

        if (viewType == typeof(DigitalInputStatusBarView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new DigitalInputViewModel(actorRegistry, actorName, itemPath);
            var view = new DigitalInputStatusBarView(viewModel);
            return view;
        }

        throw new ValueError();
    }
}