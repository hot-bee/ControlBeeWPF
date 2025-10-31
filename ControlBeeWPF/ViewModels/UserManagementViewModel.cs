using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlBee.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Dict = System.Collections.Generic.Dictionary<string, object?>;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace ControlBeeWPF.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly IAuthorityLevels _authorityLevels;
    private readonly IUserManager _userManager;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _userId = string.Empty;
    [ObservableProperty] private int _userLevel;

    public ObservableCollection<UserRow> Users { get; } = new();
    [ObservableProperty] private UserRow? _selectedUser;

    public IReadOnlyList<KeyValuePair<int, string>> LevelItems { get; }

    public UserManagementViewModel(IUserManager userManager, IAuthorityLevels authorityLevels)
    {
        _userManager = userManager;
        _authorityLevels = authorityLevels;

        LevelItems = _authorityLevels.LevelMap
            .Where(keyValuePair => keyValuePair.Key < _userManager.CurrentUser!.Level)
            .OrderBy(keyValuePair => keyValuePair.Key)
            .ToList();
        if (LevelItems.Count > 0)
            UserLevel = LevelItems[0].Key;

        LoadUsers();
    }

    private void LoadUsers()
    {
        Users.Clear();
        foreach (var user in _userManager.GetUserBelowCurrentLevel())
        {
            var row = new UserRow(user, null);
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(UserRow.Level))
                    row.LevelName = _authorityLevels.GetLevelName(row.Level);
            };
            Users.Add(row);
        }
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

        var registerSucceeded = _userManager.Register(UserId, Password, Name, UserLevel);
        if (registerSucceeded)
            MessageBox.Show("Registration successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Registration failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        Name = string.Empty;
        Password = string.Empty;
        UserId = string.Empty;
        if (LevelItems.Count > 0)
            UserLevel = LevelItems[0].Key;

        LoadUsers();
    }

    [RelayCommand]
    private void UpdateUsers()
    {
        var view = CollectionViewSource.GetDefaultView(Users) as IEditableCollectionView;
        view?.CommitEdit();
        view?.CommitNew();

        var changes = Users
            .Where(userRow => userRow.IsDirty)
            .Select(userRow => new Dict
            {
                ["Id"] = userRow.Id,
                ["Name"] = userRow.Name,
                ["RawPassword"] = string.IsNullOrWhiteSpace(userRow.PasswordInput) ? null : userRow.PasswordInput,
                ["Level"] = userRow.Level
            })
            .ToList();

        if (changes.Count == 0)
        {
            MessageBox.Show("No changes to update.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = _userManager.UpdateUsers(changes);
        if (result)
        {
            foreach (var user in Users.Where(userRow => userRow.IsDirty))
            {
                user.PasswordInput = null;
                user.IsDirty = false;
                user.LevelName = _authorityLevels.GetLevelName(user.Level);
            }
            MessageBox.Show($"Successfully updated user(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadUsers();
        }
        else
        {
            MessageBox.Show("Failed to update users.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Delete(int id)
    {
        if (MessageBox.Show($"Delete user ID {id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        var deleteSucceeded = _userManager.Delete(id);
        if (!deleteSucceeded) return;

        LoadUsers();
    }

    public partial class UserRow : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _userId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string? _passwordInput;
        [ObservableProperty] private int _level;
        [ObservableProperty] private string _levelName;

        [ObservableProperty] private bool _isDirty;

        public UserRow(IUserInfo user, string? passwordInput)
        {
            _id = user.Id;
            _userId = user.UserId;
            _name = user.Name;
            _passwordInput = passwordInput;
            _level = user.Level;
            _levelName = user.LevelName;
        }

        partial void OnNameChanged(string value) => IsDirty = true;
        partial void OnLevelChanged(int value) => IsDirty = true;
        partial void OnPasswordInputChanged(string? value) => IsDirty = true;
    }
}