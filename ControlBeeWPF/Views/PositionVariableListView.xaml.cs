using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Models;
using ControlBeeWPF.ViewModels;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

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

    private Grid CreateRowGrid()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "Indicator" }
        );
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "Name" }
        );
        for (var i = 0; i < _axisLabels.Length; i++)
            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = $"Axis_{i}" }
            );
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = GridLength.Auto, SharedSizeGroup = "SetButton" }
        );
        return grid;
    }

    private int AxisColumnOffset => 2;
    private int SetButtonColumn => 2 + _axisLabels.Length + 1;

    private void RenderHeader()
    {
        if (_axisLabels.Length == 0)
            return;

        var grid = CreateRowGrid();
        grid.Margin = new Thickness(0, 0, 0, 5);

        for (var i = 0; i < _axisLabels.Length; i++)
        {
            var headerLabel = new Label
            {
                Content = _axisLabels[i],
                Style = (Style)FindResource("MainLabelStyle"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(headerLabel, AxisColumnOffset + i);
            grid.Children.Add(headerLabel);
        }

        RowsControl.Items.Add(grid);
    }

    private void RenderRows()
    {
        foreach (var row in _viewModel.Rows)
        {
            var grid = CreateRowGrid();
            grid.Margin = new Thickness(0, 0, 0, 10);

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

            var map = row
                .AxisItemPaths.Zip(
                    row.VariableViewModels,
                    (axisPath, variableViewModel) => (axisPath, variableViewModel)
                )
                .ToDictionary(x => x.axisPath, x => x.variableViewModel);
            for (var i = 0; i < _axisLabels.Length; i++)
            {
                if (!map.TryGetValue(_axisLabels[i], out var variableViewModel))
                    continue;

                var view = _viewFactory.Create<VariableItemView>(variableViewModel)!;
                view.DisplayConverter = valueObject =>
                    valueObject is double doubleValue ? doubleValue.ToString("F3") : valueObject;
                view.Refresh();
                Grid.SetColumn(view, AxisColumnOffset + i);
                grid.Children.Add(view);
            }

            var button = new Button
            {
                Content = "Set",
                Style = (Style)FindResource("RoundButtonStyle"),
            };
            button.Click += (_, _) => SetPosition(row);
            Grid.SetColumn(button, SetButtonColumn);
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
