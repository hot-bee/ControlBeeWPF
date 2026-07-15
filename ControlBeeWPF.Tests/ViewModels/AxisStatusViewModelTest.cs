#nullable enable
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBeeWPF.ViewModels;
using JetBrains.Annotations;
using Moq;
using Xunit;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.Tests.ViewModels;

[TestSubject(typeof(AxisStatusViewModel))]
public class AxisStatusViewModelTest
{
    private static (AxisStatusViewModel vm, IUiActor ui, IActor actor) Setup()
    {
        var uiActor = Mock.Of<IUiActor>();
        Mock.Get(uiActor).Setup(m => m.Name).Returns("Ui");

        var actor = Mock.Of<IActor>();
        Mock.Get(actor).Setup(m => m.Name).Returns("MyActor");

        var registry = Mock.Of<IActorRegistry>();
        Mock.Get(registry).Setup(m => m.Get("Ui")).Returns(uiActor);
        Mock.Get(registry).Setup(m => m.Get("MyActor")).Returns(actor);

        var vm = new AxisStatusViewModel(registry, "MyActor", "/X");
        return (vm, uiActor, actor);
    }

    private static void RaiseMetaDataChanged(IUiActor uiActor, IActor actor, Dict payload)
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(actor, "/X", "_itemMetaDataChanged", payload)
            );
    }

    private static void RaiseDataChanged(IUiActor uiActor, IActor actor, Dict payload)
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(actor, "/X", "_itemDataChanged", payload)
            );
    }

    private static Dict AxisData(double commandPosition, double actualPosition)
    {
        return new Dict
        {
            ["CommandPosition"] = commandPosition,
            ["ActualPosition"] = actualPosition,
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
    public void PositionsUseRawValuesWithoutDisplayResolution()
    {
        var (vm, ui, actor) = Setup();

        RaiseDataChanged(ui, actor, AxisData(100.0, 50.0));

        Assert.Equal(100.0, vm.RawCommandPosition);
        Assert.Equal(50.0, vm.RawActualPosition);
        Assert.Equal(100.0, vm.CommandPosition);
        Assert.Equal(50.0, vm.ActualPosition);
    }

    [Fact]
    public void PositionsAreScaledByDisplayResolution()
    {
        var (vm, ui, actor) = Setup();

        RaiseMetaDataChanged(
            ui,
            actor,
            new Dict
            {
                ["Name"] = "X",
                ["Desc"] = "",
                ["DisplayResolution"] = 0.001,
            }
        );
        RaiseDataChanged(ui, actor, AxisData(1000.0, 2000.0));

        Assert.Equal(1000.0, vm.RawCommandPosition);
        Assert.Equal(2000.0, vm.RawActualPosition);
        Assert.Equal(1.0, vm.CommandPosition);
        Assert.Equal(2.0, vm.ActualPosition);
    }

    [Fact]
    public void LateMetaDataRescalesExistingPositions()
    {
        var (vm, ui, actor) = Setup();

        RaiseDataChanged(ui, actor, AxisData(1000.0, 2000.0));
        Assert.Equal(1000.0, vm.CommandPosition);
        Assert.Equal(2000.0, vm.ActualPosition);

        RaiseMetaDataChanged(
            ui,
            actor,
            new Dict
            {
                ["Name"] = "X",
                ["Desc"] = "",
                ["DisplayResolution"] = 0.001,
            }
        );

        Assert.Equal(1000.0, vm.RawCommandPosition);
        Assert.Equal(2000.0, vm.RawActualPosition);
        Assert.Equal(1.0, vm.CommandPosition);
        Assert.Equal(2.0, vm.ActualPosition);
    }

    [Fact]
    public void MissingDisplayResolutionDefaultsToOne()
    {
        var (vm, ui, actor) = Setup();

        RaiseDataChanged(ui, actor, AxisData(100.0, 50.0));
        RaiseMetaDataChanged(ui, actor, new Dict { ["Name"] = "X", ["Desc"] = "" });

        Assert.Equal(1.0, vm.DisplayResolution);
        Assert.Equal(100.0, vm.CommandPosition);
        Assert.Equal(50.0, vm.ActualPosition);
    }
}
