using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlBee.Interfaces;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace ControlBeeWPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUserManager _userManager;

    public LoginViewModel(IUserManager userManager)
    {
        _userManager = userManager;

        _userId = string.Empty;
        _userPassword = string.Empty;
    }

    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    private string _userId;

    [ObservableProperty]
    private string _userPassword;

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(UserPassword))
        {
            MessageBox.Show(
                "Please enter both your ID and password.",
                "Notice",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        if (_userManager.Login(UserId, UserPassword))
        {
            var user = _userManager.CurrentUser;
            var name = user?.Name ?? "User";
            MessageBox.Show(
                $"Welcome, {name}!",
                "Login Successful",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
            MessageBox.Show(
                "Login failed: Invalid ID or password.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
    }
}
