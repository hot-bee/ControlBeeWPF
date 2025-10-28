using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace ControlBeeWPF.ViewModels;

public record LevelOption(int Level, string Name);

public partial class UserManagementViewModel : ObservableObject
{
    private readonly IUserManager _userManager;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _userId = string.Empty;
    [ObservableProperty] private int _userLevel;

    public UserManagementViewModel(IUserManager userManager)
    {
        _userManager = userManager;

        UserLevel = LevelOptions[0].Level;
    }

    public LevelOption[] LevelOptions { get; } =
    {
        new(0, "Guest"),
        new(1, "Operator"),
        new(3, "Maintenance"),
        new(5, "Manager"),
        new(7, "Manufacturer Engineer"),
        new(9, "Software Engineer")
    };

    [RelayCommand]
    private void Register()
    {
        if (string.IsNullOrWhiteSpace(UserId) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Please fill in all fields.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var ok = _userManager.Register(UserId, Password, Name, UserLevel);
        if (ok)
            MessageBox.Show("Registration successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Registration failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}