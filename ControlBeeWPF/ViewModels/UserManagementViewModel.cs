using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace ControlBeeWPF.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly IUserManager _userManager;
    private readonly IAuthorityLevels _authorityLevels;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _userId = string.Empty;
    [ObservableProperty] private int _userLevel;

    public IReadOnlyList<KeyValuePair<int, string>> LevelItems { get; }

    public UserManagementViewModel(IUserManager userManager, IAuthorityLevels authorityLevels)
    {
        _userManager = userManager;
        _authorityLevels = authorityLevels;

        LevelItems = _authorityLevels.LevelMap.OrderBy(kv => kv.Key).ToList();

        if (LevelItems.Count > 0)
            UserLevel = LevelItems[0].Key;
    }

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

        Name = string.Empty;
        Password = string.Empty;
        UserId = string.Empty;
        if (LevelItems.Count > 0)
            UserLevel = LevelItems[0].Key;
    }
}