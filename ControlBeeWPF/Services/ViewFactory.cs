using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ControlBeeWPF.Services;

public class ViewFactory(IServiceProvider serviceProvider) : IViewFactory
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

        if (viewType == typeof(DigitalOutputStatusBarView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new DigitalOutputViewModel(actorRegistry, actorName, itemPath);
            var view = new DigitalOutputStatusBarView(viewModel);
            return view;
        }

        if (viewType == typeof(AnalogInputStatusBarView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new AnalogInputViewModel(actorRegistry, actorName, itemPath);
            var view = new AnalogInputStatusBarView(viewModel);
            return view;
        }

        throw new ValueError();
    }

    public virtual Window CreateWindow(Type viewType, params object?[]? args)
    {
        if (viewType == typeof(NumpadView))
        {
            var initialValue = (string)args![0]!;
            var allowDecimal = args![1] is true;
            var viewModel = new NumpadViewModel(initialValue, allowDecimal);
            var view = new NumpadView(viewModel);
            return view;
        }

        throw new ValueError();
    }
}