using PlushFood.Services;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using PlushFood.Commands;
using System.Windows.Input;
using System.ComponentModel;
using PlushFood.Helpers;


namespace PlushFood.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private string _username;
        private string _password;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SwitchThemeCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateAsGuestCommand { get; }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public MainViewModel()
        {
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            LoginCommand = new RelayCommand(Login);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            NavigateAsGuestCommand = new RelayCommand(NavigateAsGuest);
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

        private void Login(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string hashedPassword = ComputeHash(Password);

                using (var context = new PlushFoodContext())
                {
                    var client = context.Clients.FirstOrDefault(c => c.UserName == Username);
                    if (client != null && client.PasswordHash == hashedPassword)
                    {
                        UserSession.Instance.LoginClient(client.ClientID, client.UserName);
                        NavigateToCatalog();
                        return;
                    }

                    var admin = context.Administrators.FirstOrDefault(a => a.UserName == Username);
                    if (admin != null && admin.PasswordHash == hashedPassword)
                    {
                        UserSession.Instance.LoginAdmin(admin.AdminID, admin.UserName);
                        NavigateToAdmin();
                        return;
                    }
                }
                MessageBox.Show("Неверно введены данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при попытке входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ComputeHash(string password)
        {
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(password);
                    var hash = sha256.ComputeHash(bytes);

                    string hashedPassword = BitConverter.ToString(hash).Replace("-", "").ToUpper();
                    Console.WriteLine($"Сгенерированный хэш: {hashedPassword}");
                    return hashedPassword;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при хэшировании пароля: {ex.Message}");
                throw;
            }
        }

        private void NavigateToCatalog()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Hide();

            var catalogView = new Views.CatalogView();
            catalogView.Show();

            Application.Current.MainWindow = catalogView;
        }

        private void NavigateToAdmin()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Hide();

            var adminView = new Views.AdminView();
            adminView.Show();

            Application.Current.MainWindow = adminView;
        }

        private void NavigateToRegister(object obj)
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Hide();

            var registerView = new Views.RegisterView();
            registerView.Show();

            Application.Current.MainWindow = registerView;
        }

        private void NavigateAsGuest(object obj)
        {
            UserSession.Instance.LoginAsGuest();
            NavigateToCatalog();
        }
    }
}