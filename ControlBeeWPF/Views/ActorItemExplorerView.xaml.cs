using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBee.Variables;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Services;
using ControlBeeWPF.ViewModels;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for ActorItemExplorerView.xaml
/// </summary>
public partial class ActorItemExplorerView : UserControl, IDisposable
{
    private readonly string _actorName;
    private readonly IActorRegistry _actorRegistry;
    private readonly IViewFactory _viewFactory;
    private readonly ActorItemExplorerViewModel _viewModel;

    public ActorItemExplorerView(
        string actorName,
        ActorItemExplorerViewModel viewModel,
        IActorRegistry actorRegistry,
        IViewFactory viewFactory
    )
    {
        InitializeComponent();
        DataContext = viewModel;
        _actorName = actorName;
        _viewModel = viewModel;
        _actorRegistry = actorRegistry;
        _viewFactory = viewFactory;
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
        var value = nodeModel.Value;
        MyContentLabel.Content = type?.Name;

        DetachContent();
        if (type != null && type.IsAssignableTo(typeof(IVariable)))
        {
            MyContentControl.VerticalAlignment = VerticalAlignment.Top;
            MyContentControl.Margin = new Thickness(0, 10, 0, 0);

            if (type.IsAssignableTo(typeof(Variable<Position1D>)))
            {
                MyContentControl.Content = new VariableStatusBarView(
                    _viewFactory,
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
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [0]
                        ),
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [1]
                        ),
                    },
                };
                MyContentControl.Content = panel;
            }
            else if (value is ArrayBase and IIndex1D { Size: > 0 } index1D)
            {
                var stackPanel = new StackPanel();
                var firstItem = index1D.GetValue(0);
                if(firstItem is Position2D)
                {
                    for (var i = 0; i < index1D.Size; i++)
                    {
                        var groupBox = new GroupBox()
                        {
                            Header = $"Index: {i}",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    new VariableStatusBarView(
                                        _viewFactory,
                                        _actorRegistry,
                                        _actorName,
                                        nodeModel.ItemPath,
                                        [i, 0]
                                    ),
                                    new VariableStatusBarView(
                                        _viewFactory,
                                        _actorRegistry,
                                        _actorName,
                                        nodeModel.ItemPath,
                                        [i, 1]
                                    ),
                                },
                            }
                        };
                        stackPanel.Children.Add(groupBox);
                    }
                }
                else // double, int, ...
                {
                    for (var i = 0; i < index1D.Size; i++)
                    {
                        var groupBox = new GroupBox()
                        {
                            Header = $"Index: {i}",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    new VariableStatusBarView(
                                        _viewFactory,
                                        _actorRegistry,
                                        _actorName,
                                        nodeModel.ItemPath,
                                        [i]
                                    ),
                                },
                            }
                        };
                        stackPanel.Children.Add(groupBox);
                    }
                }
                MyContentControl.Content = stackPanel;
            }
            else if (type.IsAssignableTo(typeof(Variable<SpeedProfile>)))
            {
                var panel = new StackPanel
                {
                    Children =
                    {
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [nameof(SpeedProfile.Velocity)]
                        ),
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [nameof(SpeedProfile.Accel)]
                        ),
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [nameof(SpeedProfile.Decel)]
                        ),
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [nameof(SpeedProfile.AccelJerkRatio)]
                        ),
                        new VariableStatusBarView(
                            _viewFactory,
                            _actorRegistry,
                            _actorName,
                            nodeModel.ItemPath,
                            [nameof(SpeedProfile.DecelJerkRatio)]
                        ),
                    },
                };
                MyContentControl.Content = panel;
            }
            else
            {
                MyContentControl.Content = new VariableStatusBarView(
                    _viewFactory,
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
