using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PlushFood.Commands;
using PlushFood.Helpers;
using PlushFood.Models;
using PlushFood.Services;
using PlushFood.Views;
using System.Data.Entity;


namespace PlushFood.ViewModels
{
    internal class ProfileViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private Client _currentClient;
        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;
        private Order _selectedOrder;
        private ObservableCollection<OrderDetail> _selectedOrderDetails;

        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<Return> Returns { get; set; }
        public ObservableCollection<OrderDetail> SelectedOrderDetails
        {
            get => _selectedOrderDetails;
            set
            {
                _selectedOrderDetails = value;
                OnPropertyChanged(nameof(SelectedOrderDetails));
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                LoadOrderDetails(_selectedOrder?.OrderID);
            }
        }

        public string OldPassword
        {
            get => _oldPassword;
            set { _oldPassword = value; OnPropertyChanged(nameof(OldPassword)); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(nameof(ConfirmPassword)); }
        }

        public Client CurrentClient
        {
            get => _currentClient;
            set
            {
                _currentClient = value;
                OnPropertyChanged(nameof(CurrentClient));
            }
        }

        private string _returnReason;
        public string ReturnReason
        {
            get => _returnReason;
            set
            {
                if (_returnReason != value)
                {
                    _returnReason = value;
                    OnPropertyChanged(nameof(ReturnReason));
                }
            }
        }

        public ICommand LogoutCommand { get; set; }
        public ICommand NavigateToCatalogCommand { get; set; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ProcessReturnCommand { get; set; }

        public ProfileViewModel()
        {
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand);
            NavigateToCatalogCommand = new RelayCommand(ExecuteNavigateToCatalogCommand);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SaveProfileCommand = new RelayCommand(SaveProfile);
            ProcessReturnCommand = new RelayCommand(ProcessReturn);

            LoadUserData();
            LoadOrdersAndReturns();
        }

        private void ProcessReturn(object parameter)
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для оформления возврата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(ReturnReason))
            {
                MessageBox.Show("Пожалуйста, укажите причину возврата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedOrder.Status != "Delivered")
            {
                MessageBox.Show("Возврат возможен только для доставленных заказов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    bool hasExistingReturn = context.Returns.Any(r => r.OrderID == SelectedOrder.OrderID);
                    if (hasExistingReturn)
                    {
                        MessageBox.Show("Возврат для этого заказа уже оформлен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var order = context.Orders.Find(SelectedOrder.OrderID);
                    if (order != null)
                    {
                        order.Status = "Cancelled";
                    }

                    var returnRecord = new Return
                    {
                        OrderID = SelectedOrder.OrderID,
                        Reason = ReturnReason,
                        ReturnDate = DateTime.Now,
                        Status = "Pending"
                    };

                    context.Returns.Add(returnRecord);

                    context.SaveChanges();

                    MessageBox.Show("Возврат оформлен успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadOrdersAndReturns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении возврата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrdersAndReturns()
        {
            var session = UserSession.Instance;
            if (!session.IsGuest && session.UserId.HasValue)
            {
                try
                {
                    using (var context = new PlushFoodContext())
                    {
                        Orders = new ObservableCollection<Order>(
                            context.Orders
                                .Where(o => o.ClientID == session.UserId.Value)
                                .ToList()
                        );

                        Returns = new ObservableCollection<Return>(
                            context.Returns
                                .Where(r => r.Order.ClientID == session.UserId.Value)
                                .ToList()
                        );

                        OnPropertyChanged(nameof(Orders));
                        OnPropertyChanged(nameof(Returns)); // Уведомляем об изменении
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadOrderDetails(int? orderId)
        {
            if (orderId == null)
            {
                SelectedOrderDetails = new ObservableCollection<OrderDetail>();
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var details = context.OrderDetails
                                         .Where(od => od.OrderID == orderId)
                                         .Include(od => od.Meal)
                                         .ToList();

                    SelectedOrderDetails = new ObservableCollection<OrderDetail>(details);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserData()
        {
            var session = UserSession.Instance;
            if (!session.IsGuest && session.UserId.HasValue)
            {
                try
                {
                    using (var context = new PlushFoodContext())
                    {
                        var client = context.Clients.Find(session.UserId.Value);
                        if (client != null)
                        {
                            CurrentClient = client;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось загрузить данные клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            LogoutAndNavigateToLogin();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogoutAndNavigateToLogin();
                }
            }
            else
            {
                CurrentClient = new Client
                {
                    FirstName = "Гость",
                    LastName = string.Empty,
                    UserName = "guest",
                    Email = "n/a",
                    PhoneNumber = "n/a",
                    Address = "n/a"
                };
            }
        }

        private void SaveProfile(object parameter)
        {
            if (string.IsNullOrEmpty(CurrentClient.FirstName) ||
                string.IsNullOrEmpty(CurrentClient.LastName) ||
                string.IsNullOrEmpty(CurrentClient.UserName) ||
                string.IsNullOrEmpty(CurrentClient.Email) ||
                string.IsNullOrEmpty(CurrentClient.PhoneNumber) ||
                string.IsNullOrEmpty(CurrentClient.Address))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidEmail(CurrentClient.Email))
            {
                MessageBox.Show("Некорректный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidPhoneNumber(CurrentClient.PhoneNumber))
            {
                MessageBox.Show("Некорректный формат номера телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(CurrentClient.UserName, @"^[a-zA-Z0-9._]{5,40}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, символы '.' и '_', и должен быть длиной от 5 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(OldPassword) ||
                !string.IsNullOrEmpty(NewPassword) ||
                !string.IsNullOrEmpty(ConfirmPassword))
            {
                string currentPasswordHash = CurrentClient.PasswordHash;

                if (ComputeHash(OldPassword) != currentPasswordHash)
                {
                    MessageBox.Show("Неверный текущий пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    MessageBox.Show("Новый пароль и его подтверждение не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Regex.IsMatch(NewPassword, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$"))
                {
                    MessageBox.Show("Пароль должен содержать минимум 1 букву, 1 цифру и 1 спецсимвол, и быть не короче 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentClient.PasswordHash = ComputeHash(NewPassword);
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var existingClientWithSameUserName = context.Clients
                        .FirstOrDefault(c => c.UserName == CurrentClient.UserName && c.ClientID != CurrentClient.ClientID);

                    if (existingClientWithSameUserName != null)
                    {
                        MessageBox.Show("Клиент с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var existingClient = context.Clients.Find(CurrentClient.ClientID);
                    if (existingClient != null)
                    {
                        existingClient.FirstName = CurrentClient.FirstName;
                        existingClient.LastName = CurrentClient.LastName;
                        existingClient.UserName = CurrentClient.UserName;
                        existingClient.Email = CurrentClient.Email;
                        existingClient.PhoneNumber = CurrentClient.PhoneNumber;
                        existingClient.Address = CurrentClient.Address;
                        existingClient.PasswordHash = CurrentClient.PasswordHash;

                        context.SaveChanges();
                        MessageBox.Show("Профиль успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные. Клиент не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OldPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private void ExecuteLogoutCommand(object parameter)
        {
            LogoutAndNavigateToLogin();
        }

        private void LogoutAndNavigateToLogin()
        {
            if (!UserSession.Instance.IsGuest)
            {
                UserSession.Instance.Logout();
            }

            NavigateToLoginPage();
        }

        private void ExecuteNavigateToCatalogCommand(object parameter)
        {
            NavigateToCatalogPage();
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

        private void NavigateToLoginPage()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var loginView = new MainWindow();
            loginView.Show();

            Application.Current.MainWindow = loginView;
        }

        private void NavigateToCatalogPage()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var catalogView = new CatalogView();
            catalogView.Show();

            Application.Current.MainWindow = catalogView;
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

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            var phoneRegex = new Regex(@"^\+375(29|33|44|25)\d{7}$");
            return phoneRegex.IsMatch(phoneNumber);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
