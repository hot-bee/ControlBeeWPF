#nullable enable
using System.Windows.Media;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeWPF.ViewModels;
using JetBrains.Annotations;
using Moq;
using Xunit;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.Tests.ViewModels;

[TestSubject(typeof(InPositionIndicatorViewModel))]
public class InPositionIndicatorViewModelTest
{
    private static (IUiActor ui, IActor actor, IActorRegistry registry) SetupActors()
    {
        var uiActor = Mock.Of<IUiActor>();
        Mock.Get(uiActor).Setup(m => m.Name).Returns("Ui");

        var actor = Mock.Of<IActor>();
        Mock.Get(actor).Setup(m => m.Name).Returns("MyActor");

        var registry = Mock.Of<IActorRegistry>();
        Mock.Get(registry).Setup(m => m.Get("Ui")).Returns(uiActor);
        Mock.Get(registry).Setup(m => m.Get("MyActor")).Returns(actor);

        return (uiActor, actor, registry);
    }

    private static void RaiseMessage(
        IUiActor uiActor,
        IActor actor,
        string itemPath,
        string messageName,
        Dict payload
    )
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(actor, itemPath, messageName, payload)
            );
    }

    private static Dict AxisData(double position)
    {
        return new Dict
        {
            ["CommandPosition"] = position,
            ["ActualPosition"] = position,
            ["IsMoving"] = false,
            ["IsAlarmed"] = false,
            ["IsEnabled"] = true,
            ["IsInitializing"] = false,
            ["IsInitialized"] = true,
            ["IsHomeDet"] = false,
            ["IsNegativeLimitDet"] = false,
            ["IsPositiveLimitDet"] = false,
        };
    }

    [Fact]
    public void AlignmentComparesRawCommandPosition()
    {
        var (ui, actor, registry) = SetupActors();
        using var axisViewModel = new AxisStatusViewModel(registry, "MyActor", "/X");
        using var variableViewModel = new VariableViewModel(registry, "MyActor", "/TargetX", null);
        var viewModel = new InPositionIndicatorViewModel([axisViewModel], [variableViewModel], 0.1);

        // DisplayResolution scales what's shown, not what's compared.
        RaiseMessage(
            ui,
            actor,
            "/X",
            "_itemMetaDataChanged",
            new Dict
            {
                ["Name"] = "X",
                ["Desc"] = "",
                ["DisplayResolution"] = 0.001,
            }
        );
        RaiseMessage(
            ui,
            actor,
            "/TargetX",
            "_itemDataChanged",
            new Dict { [nameof(ValueChangedArgs)] = new ValueChangedArgs([], null, 1000.0) }
        );
        RaiseMessage(ui, actor, "/X", "_itemDataChanged", AxisData(1000.0));

        Assert.Equal(1.0, axisViewModel.CommandPosition);
        Assert.Equal(Brushes.LawnGreen, viewModel.BackgroundBrush);
    }

    [Fact]
    public void OutOfTolerancePositionIsNotAligned()
    {
        var (ui, actor, registry) = SetupActors();
        using var axisViewModel = new AxisStatusViewModel(registry, "MyActor", "/X");
        using var variableViewModel = new VariableViewModel(registry, "MyActor", "/TargetX", null);
        var viewModel = new InPositionIndicatorViewModel([axisViewModel], [variableViewModel], 0.1);

        RaiseMessage(
            ui,
            actor,
            "/TargetX",
            "_itemDataChanged",
            new Dict { [nameof(ValueChangedArgs)] = new ValueChangedArgs([], null, 1000.0) }
        );
        RaiseMessage(ui, actor, "/X", "_itemDataChanged", AxisData(500.0));

        Assert.Equal(Brushes.LightGray, viewModel.BackgroundBrush);
    }

    [Fact]
    public void MissingTargetIsNotAligned()
    {
        var (ui, actor, registry) = SetupActors();
        using var axisViewModel = new AxisStatusViewModel(registry, "MyActor", "/X");
        using var variableViewModel = new VariableViewModel(registry, "MyActor", "/TargetX", null);
        var viewModel = new InPositionIndicatorViewModel([axisViewModel], [variableViewModel], 0.1);

        RaiseMessage(ui, actor, "/X", "_itemDataChanged", AxisData(1000.0));

        Assert.Equal(Brushes.LightGray, viewModel.BackgroundBrush);
    }
}
