using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;

namespace ControlBeeWPF.ViewModels;

public partial class EditableFrameViewModel : ObservableObject
{
    private readonly IVariableManager _variableManager;

    [ObservableProperty] private bool _modified;

    public EditableFrameViewModel(IVariableManager variableManager)
    {
        _variableManager = variableManager;

        Modified = _variableManager.Modified;
        _variableManager.PropertyChanged += VariableManagerOnPropertyChanged;
    }

    private void VariableManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IVariableManager.Modified):
                Modified = _variableManager.Modified;
                break;
        }
    }

    [RelayCommand]
    private void Save()
    {
        _variableManager.Save();
    }

    [RelayCommand]
    private void Discard()
    {
        _variableManager.DiscardChanges();
    }
}