using System.Collections.ObjectModel;
using ControlBee.Interfaces;
using ControlBeeWPF.Models;

namespace ControlBeeWPF.ViewModels;

public sealed class DigitalOutputGridViewModel(IActorRegistry actorRegistry)
{
    public sealed class Cell(int row, int col, DigitalOutputViewModel viewModel)
    {
        public int Row { get; } = row;
        public int Col { get; } = col;
        public DigitalOutputViewModel ViewModel { get; } = viewModel;
    }

    public ObservableCollection<Cell> Cells { get; } = [];

    public void AddVariable(int row, int col, ActorItemKey key)
    {
        var cell = new Cell(
            row,
            col,
            new DigitalOutputViewModel(actorRegistry, key.ActorName, key.ItemPath)
        );
        Cells.Add(cell);
    }
}
