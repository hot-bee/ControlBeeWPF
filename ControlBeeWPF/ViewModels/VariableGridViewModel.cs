using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBeeWPF.Models;

namespace ControlBeeWPF.ViewModels;

public sealed partial class VariableGridViewModel(IActorRegistry actorRegistry)
{
    public ObservableCollection<Cell> Cells { get; } = [];

    public void AddVariable(int row, int col, ActorItemKey actorItemKey, string? cellName)
    {
        var variableViewModel = new VariableViewModel(
            actorRegistry,
            actorItemKey.ActorName,
            actorItemKey.ItemPath,
            actorItemKey.SubItemPath
        );
        var cell = new Cell(row, col, variableViewModel, cellName ?? variableViewModel.Name);
        Cells.Add(cell);

        if (!string.IsNullOrWhiteSpace(cellName))
            return;
        cell.CellName = variableViewModel.Name;
        variableViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(VariableViewModel.Name))
                cell.CellName = variableViewModel.Name;
        };
    }

    public sealed partial class Cell(int row, int col, VariableViewModel viewModel, string cellName)
        : ObservableObject
    {
        public int Row { get; } = row;
        public int Col { get; } = col;
        public VariableViewModel ViewModel { get; } = viewModel;

        [ObservableProperty]
        private string _cellName = cellName;
    }
}
