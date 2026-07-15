#nullable enable
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBee.Variables;
using ControlBeeWPF.ViewModels;
using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra.Double;
using Moq;
using Xunit;
using Dict = System.Collections.Generic.Dictionary<string, object?>;

namespace ControlBeeWPF.Tests.ViewModels;

[TestSubject(typeof(TeachingDataViewModel))]
public class TeachingDataViewModelTest
{
    private static (TeachingDataViewModel vm, IUiActor ui, IActor actor) Setup(object[] location)
    {
        var uiActor = Mock.Of<IUiActor>();
        Mock.Get(uiActor).Setup(m => m.Name).Returns("Ui");

        var actor = Mock.Of<IActor>();
        Mock.Get(actor).Setup(m => m.Name).Returns("MyActor");
        Mock.Get(actor).Setup(m => m.GetAxisItemPaths("/WorkPosX")).Returns(["/X"]);

        var registry = Mock.Of<IActorRegistry>();
        Mock.Get(registry).Setup(m => m.Get("Ui")).Returns(uiActor);
        Mock.Get(registry).Setup(m => m.Get("MyActor")).Returns(actor);

        var vm = new TeachingDataViewModel("MyActor", "/WorkPosX", location, registry);
        return (vm, uiActor, actor);
    }

    private static void RaiseDataChanged(IUiActor uiActor, IActor actor, ValueChangedArgs args)
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(
                    actor,
                    "/WorkPosX",
                    "_itemDataChanged",
                    new Dict { [nameof(ValueChangedArgs)] = args }
                )
            );
    }

    private static void RaiseAxisMetaDataChanged(IUiActor uiActor, IActor actor, Dict payload)
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(actor, "/X", "_itemMetaDataChanged", payload)
            );
    }

    private static void RaiseAxisDataChanged(IUiActor uiActor, IActor actor, double commandPosition)
    {
        Mock.Get(uiActor)
            .Raise(
                m => m.MessageArrived += null,
                uiActor,
                new ActorItemMessage(
                    actor,
                    "/X",
                    "_itemDataChanged",
                    new Dict { ["CommandPosition"] = commandPosition }
                )
            );
    }

    private static Array1D<Position1D> MakeArray(params double[] values)
    {
        var array = new Array1D<Position1D>(values.Length);
        for (var i = 0; i < values.Length; i++)
            array[i] = new Position1D(DenseVector.OfArray([values[i]]));
        return array;
    }

    [Fact]
    public void InitialReadDisplaysOwnIndexValue()
    {
        var array = MakeArray(10.0, 20.0, 30.0);

        var (vm0, ui0, actor0) = Setup([0]);
        RaiseDataChanged(ui0, actor0, new ValueChangedArgs([], null, array));
        Assert.Equal(10.0, vm0.TableData.Rows[1][1]);

        var (vm1, ui1, actor1) = Setup([1]);
        RaiseDataChanged(ui1, actor1, new ValueChangedArgs([], null, array));
        Assert.Equal(20.0, vm1.TableData.Rows[1][1]);

        var (vm2, ui2, actor2) = Setup([2]);
        RaiseDataChanged(ui2, actor2, new ValueChangedArgs([], null, array));
        Assert.Equal(30.0, vm2.TableData.Rows[1][1]);
    }

    [Fact]
    public void UpdateForOtherIndexIsIgnored()
    {
        var (vm, ui, actor) = Setup([1]);
        RaiseDataChanged(ui, actor, new ValueChangedArgs([], null, MakeArray(10.0, 20.0, 30.0)));
        Assert.Equal(20.0, vm.TableData.Rows[1][1]);

        // A coordinate change for index 0 must not touch index 1's view.
        RaiseDataChanged(ui, actor, new ValueChangedArgs([0, 0], 10.0, 99.0));
        Assert.Equal(20.0, vm.TableData.Rows[1][1]);

        // A coordinate change for index 1 updates it.
        RaiseDataChanged(ui, actor, new ValueChangedArgs([1, 0], 20.0, 77.0));
        Assert.Equal(77.0, vm.TableData.Rows[1][1]);
    }

    [Fact]
    public void UpdatePerCoordinate()
    {
        var (vm, ui, actor) = Setup([0]);
        RaiseDataChanged(ui, actor, new ValueChangedArgs([0, 0], 0.0, 33.0));
        Assert.Equal(33.0, vm.TableData.Rows[1][1]);
    }

    [Fact]
    public void UpdateWholeElement()
    {
        var (vm, ui, actor) = Setup([0]);
        var pos = new Position1D(DenseVector.OfArray([44.0]));
        RaiseDataChanged(ui, actor, new ValueChangedArgs([0], null, pos));
        Assert.Equal(44.0, vm.TableData.Rows[1][1]);
    }

    [Fact]
    public void CurrentPositionIsScaledByDisplayResolution()
    {
        var (vm, ui, actor) = Setup([0]);
        RaiseAxisMetaDataChanged(
            ui,
            actor,
            new Dict { ["Name"] = "X", ["DisplayResolution"] = 0.001 }
        );
        RaiseAxisDataChanged(ui, actor, 1000.0);
        Assert.Equal(1.0, vm.TableData.Rows[0][1]);
    }

    [Fact]
    public void ItemValueIsScaledByDisplayResolution()
    {
        var (vm, ui, actor) = Setup([0]);
        RaiseAxisMetaDataChanged(
            ui,
            actor,
            new Dict { ["Name"] = "X", ["DisplayResolution"] = 0.001 }
        );
        RaiseDataChanged(ui, actor, new ValueChangedArgs([], null, MakeArray(2000.0)));
        Assert.Equal(2.0, vm.TableData.Rows[1][1]);
    }

    [Fact]
    public void LateMetaDataRescalesExistingValues()
    {
        var (vm, ui, actor) = Setup([0]);
        RaiseAxisDataChanged(ui, actor, 1000.0);
        RaiseDataChanged(ui, actor, new ValueChangedArgs([], null, MakeArray(2000.0)));
        Assert.Equal(1000.0, vm.TableData.Rows[0][1]);
        Assert.Equal(2000.0, vm.TableData.Rows[1][1]);

        RaiseAxisMetaDataChanged(
            ui,
            actor,
            new Dict { ["Name"] = "X", ["DisplayResolution"] = 0.001 }
        );
        Assert.Equal(1.0, vm.TableData.Rows[0][1]);
        Assert.Equal(2.0, vm.TableData.Rows[1][1]);
    }
}
