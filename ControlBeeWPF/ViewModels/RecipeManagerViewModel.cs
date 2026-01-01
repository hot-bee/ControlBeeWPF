using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlBee.Interfaces;
using ControlBee.Services;

namespace ControlBeeWPF.ViewModels;

public partial class RecipeManagerViewModel : ObservableObject
{
    private readonly IVariableManager _variableManager;

    [ObservableProperty]
    private string _localName;

    [ObservableProperty]
    private string[] _localNames;

    public RecipeManagerViewModel(IVariableManager variableManager)
    {
        _variableManager = variableManager;
        LocalName = variableManager.LocalName;
        LocalNames = variableManager.LocalNames;
        variableManager.PropertyChanged += VariableManagerOnPropertyChanged;
    }

    private void VariableManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IVariableManager.LocalName):
                LocalName = _variableManager.LocalName;
                break;
            case nameof(IVariableManager.LocalNames):
                LocalNames = _variableManager.LocalNames;
                break;
        }
    }
}
