﻿using System.Threading;
using System.Windows;
using ControlBee.Models;
using ControlBee.Services;
using ControlBee.Variables;
using ControlBeeWPF.Models;
using JetBrains.Annotations;
using Xunit;

namespace ControlBeeWPF.Tests.Models;

[TestSubject(typeof(UiActorMessageHandler))]
public class UiActorMessageHandlerTest
{
    [Fact]
    public void CallbackIntoMainThreadTest()
    {
        var ui = new UiActor(
            new ActorConfig(
                "ui",
                EmptyAxisFactory.Instance,
                EmptyDigitalInputFactory.Instance,
                EmptyDigitalOutputFactory.Instance,
                EmptyVariableManager.Instance,
                EmptyTimeManager.Instance,
                EmptyActorItemInjectionDataSource.Instance
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
