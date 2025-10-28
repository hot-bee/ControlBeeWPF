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
    private readonly IUserInfo _userInfo;
    private readonly IUserManager _userManager;

    public LoginViewModel(IUserInfo userInfo, IUserManager userManager)
    {
        _userInfo = userInfo;
        _userManager = userManager;
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
            MessageBox.Show("Please enter both your ID and password.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_userManager.Login(UserId, UserPassword))
        {
            MessageBox.Show($"Welcome, {_userInfo.Name}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
            MessageBox.Show("Login failed: Invalid ID or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
