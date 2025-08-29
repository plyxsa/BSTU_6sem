using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using PlushFood.Commands;
using PlushFood.Services;
using PlushFood.Models;
using System.Text.RegularExpressions;

namespace PlushFood.ViewModels
{
    internal class RegisterViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private string _username;
        private string _password;
        private string _email;
        private string _firstName;
        private string _lastName;
        private string _address;
        private string _phoneNumber;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SwitchThemeCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public RegisterViewModel()
        {
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            RegisterCommand = new RelayCommand(Register);
            BackCommand = new RelayCommand(BackToMainWindow);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void Register(object obj)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Address) ||
                string.IsNullOrWhiteSpace(PhoneNumber))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный адрес электронной почты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(PhoneNumber, @"^\+375(29|33|44|25)\d{7}$"))
            {
                MessageBox.Show("Введите номер телефона в формате +375XXXXXXXXX!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка логина (только латинские буквы, цифры, спецсимволы '.' и '_', длина от 5 до 40 символов)
            if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9._]{5,40}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, символы '.' и '_', и должен быть длиной от 5 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка пароля (латинские буквы, минимум 1 буква, цифра и спецсимволы)
            if (!Regex.IsMatch(Password, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$"))
            {
                MessageBox.Show("Пароль должен содержать минимум 1 букву, 1 цифру и 1 спецсимвол!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка имени и фамилии (латинские или русские буквы, длина от 2 до 40 символов)
            if (!Regex.IsMatch(FirstName, @"^[a-zA-Zа-яА-Я]{2,40}$") || !Regex.IsMatch(LastName, @"^[a-zA-Zа-яА-Я]{2,40}$"))
            {
                MessageBox.Show("Имя и фамилия могут содержать только латинские и русские буквы и должны быть длиной от 2 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка адреса (формат "ул. Название, число для дома, число для квартиры")
            if (!Regex.IsMatch(Address, @"^ул\.\s+[а-яА-Яa-zA-Z]+\s*,\s*\d+\s*,\s*\d+$"))
            {
                MessageBox.Show("Адрес должен быть в формате: 'ул. Название, число для дома, число для квартиры'!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var existingUser = context.Clients.FirstOrDefault(c => c.UserName == Username);
                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке логина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var hashedPassword = ComputeHash(Password);

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var newClient = new Client
                    {
                        UserName = Username,
                        PasswordHash = hashedPassword,
                        Email = Email,
                        FirstName = FirstName,
                        LastName = LastName,
                        Address = Address,
                        PhoneNumber = PhoneNumber
                    };

                    context.Clients.Add(newClient);
                    context.SaveChanges();
                }

                MessageBox.Show("Регистрация успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToMainWindow(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BackToMainWindow(object obj)
        {
            var currentWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            if (currentWindow != null)
            {
                currentWindow.Close();
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
