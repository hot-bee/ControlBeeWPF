using System.Windows;
using System.Windows.Controls;
using ControlBeeWPF.Models;
using ControlBeeWPF.ViewModels;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ControlBeeWPF.Views;

/// <summary>
///     Interaction logic for DigitalOutputGridView.xaml
/// </summary>
public partial class DigitalOutputGridView
{
    private readonly DigitalOutputGridViewModel _viewModel;
    private readonly int _rowCount;
    private readonly int _colCount;
    private bool _rendered;

    public DigitalOutputGridView(DigitalOutputGridViewModel viewModel, int rowCount, int colCount)
    {
        InitializeComponent();
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

        RenderCells();
        _rendered = true;
    }

    public void AddVariable(int row, int col, ActorItemKey keys)
    {
        _viewModel.AddVariable(row, col, keys);
    }

    private void RenderCells()
    {
        for (var row = 0; row < _rowCount; row++)
            CellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (var col = 0; col < _colCount; col++)
            CellGrid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );

        foreach (var cell in _viewModel.Cells)
        {
            var view = new DigitalOutputStatusBarView(cell.ViewModel)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                NameColumnWidth = new GridLength(3, GridUnitType.Star),
            };

            Grid.SetRow(view, cell.Row);
            Grid.SetColumn(view, cell.Col);
            CellGrid.Children.Add(view);
        }
    }
}
