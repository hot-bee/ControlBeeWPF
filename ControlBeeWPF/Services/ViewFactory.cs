using System.Windows;
using ControlBee.Interfaces;
using ControlBeeAbstract.Exceptions;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using UserControl = System.Windows.Controls.UserControl;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

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

        if (viewType == typeof(VariableStatusBarView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var subItemPath = args.Length > 2 ? (object[]?)args[2] : null;
            var viewFactory = serviceProvider.GetRequiredService<IViewFactory>();
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new VariableViewModel(actorRegistry, actorName, itemPath, subItemPath);
            var view = new VariableStatusBarView(viewFactory, viewModel);
            return view;
        }

        if (viewType == typeof(AxisStatusView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var index = (int)args![2]!;
            var shortMode = args.Length > 3 && (bool)args[3]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new AxisStatusViewModel(actorRegistry, actorName, itemPath, index);
            var view = new AxisStatusView(viewModel, shortMode);
            return view;
        }

        if (viewType == typeof(TeachingAxisStatusView))
        {
            var actorName = (string)args![0]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var view = new TeachingAxisStatusView(actorName, actorRegistry, this);
            return view;
        }

        if (viewType == typeof(TeachingJogView))
        {
            var actorName = (string)args![0]!;
            var positionItemPath = (string)args[1]!;
            var location = (object[])args[2]!;
            var jogView = args.Length > 3 ? (UserControl)args[3]! : null;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            var viewModel =
                new TeachingJogViewModel(actorName, positionItemPath, location, actorRegistry, dialogService);
            if (jogView == null)
            {
                var jogViewModel = new JogViewModel(actorName, actorRegistry);
                jogView = new JogView(jogViewModel);
            }

            var view = new TeachingJogView(viewModel, jogView);
            return view;
        }

        if (viewType == typeof(InspectionContainerView))
        {
            var visionDeviceName = (string)args![0]!;
            var options = (Dict)args[1]!;
            var deviceManager = serviceProvider.GetRequiredService<IDeviceManager>();
            var systemConfigurations = serviceProvider.GetRequiredService<ISystemConfigurations>();
            var viewModel = new VisionStatusViewModel(visionDeviceName, deviceManager);
            var view = new InspectionContainerView(viewModel, systemConfigurations, options);
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