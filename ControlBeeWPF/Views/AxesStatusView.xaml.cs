using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Services;
using ControlBeeWPF.ViewModels;
using Button = System.Windows.Controls.Button;
using GroupBox = System.Windows.Controls.GroupBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlBeeWPF.Views;

public partial class AxesStatusView : UserControl
{
    private readonly IViewFactory _viewFactory;
    private readonly List<AxisStatusView> _axisStatusViews = [];
    private readonly AxesStatusViewModel _viewModel;

    public AxesStatusView(AxesStatusViewModel viewModel, IViewFactory viewFactory)
    {
        _viewFactory = viewFactory;
        DataContext = _viewModel = viewModel;
        InitializeComponent();

        var nameTitleParis = (((string, string)[])[("", "All")]).Concat(
            viewModel.GetActorNameTitlePairs()
        );

        foreach (var (name, title) in nameTitleParis)
        {
            var button = new Button
            {
                Content = title,
                Height = 40,
                Margin = new Thickness(1),
            };
            button.Click += (sender, args) =>
            {
                viewModel.SelectActor(name);
            };
            ActorPanel.Children.Add(button);
        }

        viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        viewModel.SelectActor("");
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.SelectedActors):
                UpdateContent();
                break;
        }
    }

    private void ClearContent()
    {
        _axisStatusViews.ForEach(x => x.Dispose());
        _axisStatusViews.Clear();
        AxisStatusListPanel.Children.Clear();
    }

    private void UpdateContent()
    {
        ClearContent();
        var index = 1;
        foreach (var actor in _viewModel.SelectedActors)
        {
            var panel = new StackPanel { Margin = new Thickness(5) };
            var groupBox = new GroupBox
            {
                Header = actor.Title,
                Content = panel,
                Margin = new Thickness(0, AxisStatusListPanel.Children.Count > 0 ? 10 : 0, 0, 0),
            };
            AxisStatusListPanel.Children.Add(groupBox);
            foreach (var (itemPath, type) in actor.GetItems())
                if (type.IsAssignableTo(typeof(IAxis)))
                {
                    if (panel.Children.Count > 0)
                        panel.Children.Add(new Separator());
                    var statusView = (AxisStatusView)
                        _viewFactory.Create(typeof(AxisStatusView), actor.Name, itemPath, index++);
                    _axisStatusViews.Add(statusView);
                    panel.Children.Add(statusView);
                }
        }
    }
}
