using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.Interfaces;
using ControlBeeWPF.Models;
using ControlBeeWPF.ViewModels;
using Binding = System.Windows.Data.Binding;
using BindingMode = System.Windows.Data.BindingMode;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for VariableGridView.xaml
/// </summary>
public partial class VariableGridView
{
    private readonly VariableGridViewModel _viewModel;
    private readonly IViewFactory _viewFactory;
    private readonly int _rowCount;
    private readonly int _colCount;
    private bool _rendered;

    public VariableGridView(
        IViewFactory viewFactory,
        VariableGridViewModel viewModel,
        int rowCount,
        int colCount
    )
    {
        InitializeComponent();
        _viewFactory = viewFactory;
        DataContext = viewModel;
        _viewModel = viewModel;
        _rowCount = rowCount;
        _colCount = colCount;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_rendered)
            return;

        RenderRows();
        _rendered = true;
    }

    public void AddVariable(int row, int col, ActorItemKey actorItemKey, string? cellName = null)
    {
        _viewModel.AddVariable(row, col, actorItemKey, cellName);
    }

    private void RenderRows()
    {
        for (var row = 0; row < _rowCount; row++)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var col = 0; col < _colCount; col++)
            CellGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );

        foreach (var cell in _viewModel.Cells)
        {
            var container = new Grid
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            container.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var label = new Label
            {
                Content = cell.CellName,
                Style = (Style)FindResource("MainLabelStyle"),
                MinWidth = 120,
            };
            label.SetBinding(
                ContentProperty,
                new Binding(nameof(VariableGridViewModel.Cell.CellName))
                {
                    Source = cell,
                    Mode = BindingMode.OneWay,
                }
            );
            Grid.SetColumn(label, 0);
            container.Children.Add(label);

            var view = _viewFactory.Create<VariableItemView>(cell.ViewModel);
            if (view != null)
            {
                Grid.SetColumn(view, 1);
                container.Children.Add(view);
            }

            Grid.SetRow(container, cell.Row);
            Grid.SetColumn(container, cell.Col);
            CellGrid.Children.Add(container);
        }
    }
}
