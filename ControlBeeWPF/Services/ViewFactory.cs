using ControlBee.Constants;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using ControlBeeWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Services;

public class ViewFactory(IServiceProvider serviceProvider) : IViewFactory
{
    [Obsolete]
    public virtual UserControl? Create(Type viewType, params object?[]? args) // TODO: Migrate to Create<T>().
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
            var properties = args.Length > 3 ? (Dict?)args[3] : null;
            var viewFactory = serviceProvider.GetRequiredService<IViewFactory>();
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new VariableViewModel(actorRegistry, actorName, itemPath, subItemPath);
            var view = new VariableStatusBarView(viewFactory, viewModel);
            foreach (var (propertyName, value) in properties ?? [])
            {
                var property = view.GetType().GetProperty(propertyName);
                property?.SetValue(view, value);
            }

            return view;
        }

        if (viewType == typeof(AxisStatusView))
        {
            var actorName = (string)args![0]!;
            var itemPath = (string)args![1]!;
            var index = (int)args![2]!;
            var shortMode = args.Length > 3 && (bool)args[3]!;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new AxisStatusViewModel(actorRegistry, actorName, itemPath);
            var view = new AxisStatusView(viewModel, shortMode, index);
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
            var viewModel = new TeachingJogViewModel(
                actorName,
                positionItemPath,
                location,
                actorRegistry,
                dialogService
            );
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

        if (viewType == typeof(EventView))
        {
            var severities = (DialogSeverity[])args![0]!;
            var eventManager = serviceProvider.GetRequiredService<IEventManager>();
            var viewModel = new EventViewModel(eventManager, severities);
            var view = new EventView(viewModel);
            return view;
        }

        if (viewType == typeof(EditableFrameView))
        {
            var content = (UserControl)args![0]!;
            var systemConfigurations = serviceProvider.GetRequiredService<ISystemConfigurations>();
            var variableManager = serviceProvider.GetRequiredService<IVariableManager>();
            var viewModel = new EditableFrameViewModel(variableManager);
            var view = new EditableFrameView(systemConfigurations, viewModel, content);
            return view;
        }

        if (viewType == typeof(DigitalInputVariableBoolRectView))
        {
            var actorName = (string)args![0]!;
            var digitalInputItemPath = (string)args![1]!;
            var variableItemPath = (string)args![2]!;
            var variableSubItem = args.Length > 3 ? (object[]?)args[3] : null;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var digitalInputViewModel = new DigitalInputViewModel(
                actorRegistry,
                actorName,
                digitalInputItemPath
            );
            var variableViewModel = new VariableViewModel(
                actorRegistry,
                actorName,
                variableItemPath,
                variableSubItem
            );
            var view = new DigitalInputVariableBoolRectView(
                digitalInputViewModel,
                variableViewModel
            );
            return view;
        }

        if (viewType == typeof(IoView))
        {
            var actorName = (string)args![0]!;
            var columns = (int)args![1]!;
            var pageSize = args.Length > 2 ? (int?)args[2]! : null;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var view = new IoView(actorName, columns, actorRegistry, this, pageSize);
            return view;
        }

        if (viewType == typeof(InitializationView))
        {
            var excludedActors = args!.Length > 0 ? (List<string>)args[0]! : null;
            var actorRegistry = serviceProvider.GetRequiredService<IActorRegistry>();
            var viewModel = new InitializationViewModel(actorRegistry, excludedActors);
            var view = new InitializationView(actorRegistry, viewModel);
            return view;
        }

        return null;
    }

    public virtual T? Create<T>(params object?[]? args)
        where T : class
    {
        if (typeof(T) == typeof(NumpadView))
        {
            var initialValue = (string)args![0]!;
            var allowDecimal = args![1] is true;
            var minValue = args.Length > 2 ? (string?)args[2] : null;
            var maxValue = args.Length > 3 ? (string?)args[3] : null;
            var viewModel = new NumpadViewModel(initialValue, allowDecimal, minValue!, maxValue!);
            var view = new NumpadView(viewModel);
            return (view as T)!;
        }

        if (typeof(T) == typeof(LoginView))
        {
            var userManager = serviceProvider.GetRequiredService<IUserManager>();
            var viewModel = new LoginViewModel(userManager);
            var view = new LoginView(viewModel);
            return (view as T)!;
        }

        if (typeof(T) == typeof(UserManagementView))
        {
            var userManager = serviceProvider.GetRequiredService<IUserManager>();
            var authorityLevels = serviceProvider.GetRequiredService<IAuthorityLevels>();
            var viewModel = new UserManagementViewModel(userManager, authorityLevels);
            var view = new UserManagementView(viewModel);
            return (view as T)!;
        }

        return null;
    }
}
