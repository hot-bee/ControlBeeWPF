using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBee.Variables;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for ActorItemExplorerView.xaml
/// </summary>
public partial class ActorItemExplorerView : UserControl, IDisposable
{
    private readonly string _actorName;
    private readonly IActorRegistry _actorRegistry;
    private readonly ActorItemExplorerViewModel _viewModel;

    public ActorItemExplorerView(
        string actorName,
        ActorItemExplorerViewModel viewModel,
        IActorRegistry actorRegistry
    )
    {
        InitializeComponent();
        DataContext = viewModel;
        _actorName = actorName;
        _viewModel = viewModel;
        _actorRegistry = actorRegistry;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    public void Dispose()
    {
        _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        DetachContent();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.SelectedItem))
            UpdateContent();
    }

    private void DetachContent()
    {
        if (MyContentControl.Content is IDisposable disposable)
            disposable.Dispose();
        MyContentControl.Content = null;
    }

    private void UpdateContent()
    {
        if (_viewModel.SelectedItem == null)
            return;
        var nodeModel = _viewModel.SelectedItem.Data;
        var type = nodeModel.Type;
        MyContentLabel.Content = type?.Name;

        DetachContent();
        if (type != null && type.IsAssignableTo(typeof(IVariable)))
        {
            MyContentControl.VerticalAlignment = VerticalAlignment.Top;
            MyContentControl.Margin = new Thickness(0, 10, 0, 0);

            if (type.IsAssignableTo(typeof(Variable<Position1D>)))
            {
                MyContentControl.Content = new VariableStatusBarView(
                    _actorRegistry,
                    _actorName,
                    nodeModel.ItemPath,
                    [0]
                );
            }
            else if (type.IsAssignableTo(typeof(Variable<Position2D>)))
            {
                var panel = new StackPanel
                {
                    Children =
                    {
                        new VariableStatusBarView(
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [0]
                        ),
                        new VariableStatusBarView(
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [1]
                        ),
                    },
                };
                MyContentControl.Content = panel;
            }
            else
            {
                MyContentControl.Content = new VariableStatusBarView(
                    _actorRegistry,
                    _actorName,
                    nodeModel.ItemPath
                );
            }
        }
    }

    private void TreeView_OnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e
    )
    {
        _viewModel.SelectedItem = e.NewValue as ActorItemTreeNode;
    }
}
