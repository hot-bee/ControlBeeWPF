using System.Threading;
using System.Windows;
using ControlBee.Interfaces;
using ControlBee.Models;
using ControlBeeWPF.Models;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace ControlBeeWPF.Tests.Models;

[TestSubject(typeof(UiActorMessageHandler))]
public class UiActorMessageHandlerTest
{
    [Fact]
    public void CallbackIntoMainThreadTest()
    {
        var systemConfigurations = new SystemConfigurations();
        var ui = new UiActor(
            new ActorConfig(
                "ui",
                systemConfigurations,
                Mock.Of<IAxisFactory>(),
                Mock.Of<IDigitalInputFactory>(),
                Mock.Of<IDigitalOutputFactory>(),
                Mock.Of<IInitializeSequenceFactory>(),
                Mock.Of<IBinaryActuatorFactory>(),
                Mock.Of<IVariableManager>(),
                Mock.Of<ITimeManager>(),
                Mock.Of<IScenarioFlowTester>(),
                Mock.Of<IActorItemInjectionDataSource>(),
                null
            )
        );

        var called = false;
        var sync1 = new AutoResetEvent(false);
        var mainThread = new Thread(() =>
        {
            var application = new Application();
            var handler = new UiActorMessageHandler(application.Dispatcher);
            ui.SetHandler(handler);
            ui.MessageArrived += (sender, message) =>
            {
                Assert.Equal("_hello", message.Name);
                called = true;
                application.Shutdown();
            };
            sync1.Set();
            application.Run();
        });
        mainThread.Start();

        sync1.WaitOne();
        ui.Send(new Message(EmptyActor.Instance, "_hello"));
        mainThread.Join();
        Assert.True(called);
    }
}
