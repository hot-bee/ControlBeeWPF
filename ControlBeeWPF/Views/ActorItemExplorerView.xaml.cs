using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ControlBee.Interfaces;
using ControlBee.Variables;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.ViewModels;
using String = ControlBee.Variables.String;

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
                MyContentControl.Content = _viewFactory.Create(typeof(VariableStatusBarView), _actorName,
                    nodeModel.ItemPath, (object[]) [0]);
            }
            else if (type.IsAssignableTo(typeof(Variable<Position2D>)))
            {
                var panel = new StackPanel
                {
                    Children =
                    {
                        _viewFactory.Create(typeof(VariableStatusBarView),
                            _actorName,
                            nodeModel.ItemPath,
                            (object[]) [0]),
                        _viewFactory.Create(typeof(VariableStatusBarView), _actorName, nodeModel.ItemPath, (object[]) [1]
                        )
                    }
                };
                MyContentControl.Content = panel;
            }
            else if (value is ArrayBase and IIndex1D { Size: > 0 } index1D)
            {
                var stackPanel = new StackPanel();
                var firstItem = index1D.GetValue(0);
                if (firstItem is Position1D)
                    for (var i = 0; i < index1D.Size; i++)
                    {
                        var groupBox = new GroupBox
                        {
                            Header = $"Index: {i}",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    _viewFactory.Create(typeof(VariableStatusBarView),
                                        _actorName,
                                        nodeModel.ItemPath,
                                        (object[]) [i, 0]
                                    )
                                }
                            }
                        };
                        stackPanel.Children.Add(groupBox);
                    }
                else if (firstItem is Position2D)
                    for (var i = 0; i < index1D.Size; i++)
                    {
                        var groupBox = new GroupBox
                        {
                            Header = $"Index: {i}",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    _viewFactory.Create(typeof(VariableStatusBarView),
                                        _actorName,
                                        nodeModel.ItemPath,
                                        (object[]) [i, 0]
                                    ),
                                    _viewFactory.Create(typeof(VariableStatusBarView),
                                        _actorName,
                                        nodeModel.ItemPath,
                                        (object[]) [i, 1]
                                    )
                                }
                            }
                        };
                        stackPanel.Children.Add(groupBox);
                    }
                else // double, int, ...
                    for (var i = 0; i < index1D.Size; i++)
                    {
                        var groupBox = new GroupBox
                        {
                            Header = $"Index: {i}",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    _viewFactory.Create(typeof(VariableStatusBarView),
                                        _actorName,
                                        nodeModel.ItemPath,
                                        (object[]) [i]
                                    )
                                }
                            }
                        };
                        stackPanel.Children.Add(groupBox);
                    }

                MyContentControl.Content = stackPanel;
            }
            else if (type.IsAssignableTo(typeof(Variable<SpeedProfile>)))
            {
                var velocityView = (VariableStatusBarView)_viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) [nameof(SpeedProfile.Velocity)]
                );
                velocityView.OverrideName = "Velocity";
                var accelView = (VariableStatusBarView)_viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) [nameof(SpeedProfile.Accel)]
                );
                accelView.OverrideName = "Acceleration";
                var decelView = (VariableStatusBarView)_viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) [nameof(SpeedProfile.Decel)]
                );
                decelView.OverrideName = "Deceleration";
                var accelJerkRatioView = (VariableStatusBarView)_viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) [nameof(SpeedProfile.AccelJerkRatio)]
                );
                accelJerkRatioView.OverrideName = "AccelJerkRatio";
                var decelJerkRatioView = (VariableStatusBarView)_viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) [nameof(SpeedProfile.DecelJerkRatio)]
                );
                decelJerkRatioView.OverrideName = "DecelJerkRatio";
                var panel = new StackPanel
                {
                    Children = { velocityView, accelView, decelView, accelJerkRatioView, decelJerkRatioView }
                };
                MyContentControl.Content = panel;
            }
            else if (type.IsAssignableTo(typeof(Variable<String>)))
            {
                MyContentControl.Content = _viewFactory.Create(typeof(VariableStatusBarView),
                    _actorName,
                    nodeModel.ItemPath,
                    (object[]) ["Value"]
                );
            }
            else
            {
                MyContentControl.Content = _viewFactory.Create(typeof(VariableStatusBarView),
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