using PlushFood.Commands;
using PlushFood.Helpers;
using PlushFood.Models;
using PlushFood.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PlushFood.ViewModels
{
    internal class ProfileViewModelAdmin : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private Administrator _currentAdmin;
        private string _password;

        public ICommand NavigateBackCommand { get; private set; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand SaveProfileCommand { get; private set; }

        public Administrator CurrentAdmin
        {
            get => _currentAdmin;
            set
            {
                if (_currentAdmin != value)
                {
                    _currentAdmin = value;
                    OnPropertyChanged(nameof(CurrentAdmin));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public ProfileViewModelAdmin()
        {
            NavigateBackCommand = new RelayCommand(NavigateBack);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SaveProfileCommand = new RelayCommand(SaveProfile);

            LoadCurrentAdmin();
        }

        private void NavigateBack(object parameter)
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Hide();

            var previousView = new Views.AdminView();
            previousView.Show();

            Application.Current.MainWindow = previousView;
        }

        private void SwitchTheme(object obj)
        {
            _isDarkTheme = !_isDarkTheme;
            var themeUri = new Uri($"/Themes/{(_isDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml")}", UriKind.Relative);

            var newTheme = new ResourceDictionary { Source = themeUri };

            var applicationResources = Application.Current.Resources.MergedDictionaries;
            applicationResources.Clear();
            applicationResources.Add(newTheme);
        }

        private void LoadCurrentAdmin()
        {
            try
            {
                if (UserSession.Instance.Role == "Admin")
                {
                    using (var context = new PlushFoodContext())
                    {
                        var admin = context.Administrators
                                           .FirstOrDefault(a => a.AdminID == UserSession.Instance.UserId);

                        if (admin != null)
                        {
                            CurrentAdmin = admin;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных администратора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveProfile(object parameter)
        {
            if (CurrentAdmin == null)
            {
                MessageBox.Show("Не выбран администратор.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentAdmin.UserName) || string.IsNullOrWhiteSpace(CurrentAdmin.Email))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidEmail(CurrentAdmin.Email))
            {
                MessageBox.Show("Неверный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(CurrentAdmin.UserName, @"^[a-zA-Z0-9._]{5,40}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, символы '.' и '_', и должен быть длиной от 5 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                if (!Regex.IsMatch(Password, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$"))
                {
                    MessageBox.Show("Пароль должен содержать минимум 1 букву, 1 цифру и 1 спецсимвол, и быть не короче 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var existingAdminWithSameUserName = context.Administrators
                        .FirstOrDefault(a => a.UserName == CurrentAdmin.UserName && a.AdminID != CurrentAdmin.AdminID);

                    if (existingAdminWithSameUserName != null)
                    {
                        MessageBox.Show("Администратор с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var adminInDb = context.Administrators.Find(CurrentAdmin.AdminID);
                    if (adminInDb != null)
                    {
                        adminInDb.UserName = CurrentAdmin.UserName;
                        adminInDb.Email = CurrentAdmin.Email;

                        if (!string.IsNullOrEmpty(Password))
                        {
                            adminInDb.PasswordHash = ComputeHash(Password);
                        }

                        context.SaveChanges();
                    }
                    else
                    {
                        MessageBox.Show("Администратор не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                Password = null;
                MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string ComputeHash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
