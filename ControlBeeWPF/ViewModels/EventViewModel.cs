using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Constants;
using ControlBee.Interfaces;
using log4net;
using Application = System.Windows.Application;

namespace ControlBeeWPF.ViewModels;

public partial class EventViewModel : ObservableObject
{
    private readonly DialogSeverity[] _severities;
    private const int EventLimitCount = 100;
    private static readonly ILog Logger = LogManager.GetLogger(nameof(EventViewModel));
    [ObservableProperty] private ObservableCollection<string> _events = [];

    public EventViewModel(IEventManager eventManager, DialogSeverity[] severities)
    {
        _severities = severities;
        eventManager.EventOccured += EventManagerOnEventOccured;
    }

    private void EventManagerOnEventOccured(object? sender, EventMessage e)
    {
        if (!_severities.Contains(e.Severity)) return;
        Application.Current.Dispatcher.Invoke(() =>  // TODO: Use IDispatcher
        {
            Events.Add($"[{e.EventTime}] {e.Name}");
            while (EventLimitCount < Events.Count) Events.RemoveAt(0);
        });
    }
}