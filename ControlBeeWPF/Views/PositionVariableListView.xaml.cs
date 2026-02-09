using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Models;
using ControlBeeWPF.ViewModels;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for PositionVariableListView.xaml
/// </summary>
public partial class PositionVariableListView
{
    private readonly IViewFactory _viewFactory;
    private readonly PositionVariableListViewModel _viewModel;
    private bool _rendered;
    private readonly string[] _axisLabels;
    private readonly double _tolerance;

    public PositionVariableListView(
        IViewFactory viewFactory,
        PositionVariableListViewModel viewModel,
        string[] axisLabels,
        double? tolerance
    )
    {
        InitializeComponent();
        _viewFactory = viewFactory;
        DataContext = viewModel;
        _viewModel = viewModel;
        _axisLabels = axisLabels;
        _tolerance = tolerance ?? 0.001;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_rendered)
            return;

        RenderHeader();
        RenderRows();
        _rendered = true;
    }

    public void AddVariable(
        string rowName,
        IEnumerable<PositionVariableItemKey> positionVariableItemKeys
    )
    {
        _viewModel.AddVariable(rowName, positionVariableItemKeys);
    }

    private void RenderHeader()
    {
        if (_axisLabels.Length == 0)
            return;

        var grid = new Grid { Margin = new Thickness(0, 0, 0, 5) };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(175) });
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );

        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };

        foreach (var axis in _axisLabels)
        {
            headerPanel.Children.Add(
                new Label
                {
                    Content = axis,
                    Style = (Style)FindResource("MainLabelStyle"),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Width = 86,
                }
            );
        }

        Grid.SetColumn(headerPanel, 2);
        grid.Children.Add(headerPanel);

        RowsControl.Items.Add(grid);
    }

    private void RenderRows()
    {
        foreach (var row in _viewModel.Rows)
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(175) });
            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var inPositionView = _viewFactory.Create<InPositionIndicatorView>(
                row.AxisStatusViewModels,
                row.VariableViewModels,
                _tolerance
            )!;

            Grid.SetColumn(inPositionView, 0);
            grid.Children.Add(inPositionView);

            var label = new Label
            {
                Content = row.RowName,
                Style = (Style)FindResource("MainLabelStyle"),
            };
            Grid.SetColumn(label, 1);
            grid.Children.Add(label);

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(panel, 2);
            var map = row
                .AxisItemPaths.Zip(
                    row.VariableViewModels,
                    (axisPath, variableViewModel) => (axisPath, variableViewModel)
                )
                .ToDictionary(x => x.axisPath, x => x.variableViewModel);
            foreach (var axisLabel in _axisLabels)
            {
                if (!map.TryGetValue(axisLabel, out var variableViewModel))
                {
                    panel.Children.Add(new Border { Width = 90, Margin = new Thickness(2) });
                    continue;
                }

                var view = _viewFactory.Create<VariableItemView>(variableViewModel);
                panel.Children.Add(new ContentControl { Content = view });
            }
            grid.Children.Add(panel);

            var button = new Button
            {
                Content = "Set",
                Style = (Style)FindResource("RoundButtonStyle"),
            };
            button.Click += (_, _) => SetPosition(row);
            Grid.SetColumn(button, 3);
            grid.Children.Add(button);

            RowsControl.Items.Add(grid);
        }
    }

    private void SetPosition(PositionVariableListViewModel.Row row)
    {
        if (
            MessageBox.Show(
                "Do you want to save the current position?",
                "Set position",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.No
        )
            return;

        _viewModel.SetPosition(row);
    }
}
