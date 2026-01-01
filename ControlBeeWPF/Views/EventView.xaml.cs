using System.Collections.Specialized;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for EventView.xaml
/// </summary>
public partial class EventView
{
    public EventView(EventViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        viewModel.Events.CollectionChanged += EventsOnCollectionChanged;
    }

    private void EventsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            EventListBox.ScrollIntoView(EventListBox.Items[^1]!);
    }
}
