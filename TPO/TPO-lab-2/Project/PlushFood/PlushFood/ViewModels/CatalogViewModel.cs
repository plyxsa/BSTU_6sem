using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PlushFood.Commands;
using PlushFood.Helpers;
using PlushFood.Models;
using PlushFood.Services;
using PlushFood.Views;

namespace PlushFood.ViewModels
{
    public class CatalogViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private ObservableCollection<Meal> _meals;
        private ObservableCollection<Meal> _allMeals;
        private ObservableCollection<MealCategory> _categories;
        private ObservableCollection<CartItem> _cartMeals;
        private string _searchText;
        private MealCategory _selectedCategory;
        private decimal _totalPrice;

        public ICommand OrderCommand { get; set; }
        public ICommand ProfileCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand AddToCartCommand { get; set; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand IncreaseQuantityCommand { get; set; }
        public ICommand DecreaseQuantityCommand { get; set; }

        public ObservableCollection<Meal> Meals
        {
            get { return _meals; }
            set
            {
                _meals = value;
                OnPropertyChanged(nameof(Meals));
            }
        }

        public ObservableCollection<MealCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ObservableCollection<CartItem> CartMeals
        {
            get { return _cartMeals; }
            set
            {
                _cartMeals = value;
                OnPropertyChanged(nameof(CartMeals));
            }
        }

        public decimal TotalPrice
        {
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilter();
            }
        }

        public MealCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                ApplyFilter();
            }
        }

        public CatalogViewModel()
        {
            Meals = new ObservableCollection<Meal>();
            _allMeals = new ObservableCollection<Meal>();
            Categories = new ObservableCollection<MealCategory>();
            CartMeals = new ObservableCollection<CartItem>();
            LoadMeals();
            LoadCategories();

            OrderCommand = new RelayCommand(ExecuteOrderCommand);
            ProfileCommand = new RelayCommand(ExecuteProfileCommand);
            LogoutCommand = new RelayCommand(ExecuteLogoutCommand);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            AddToCartCommand = new RelayCommand(ExecuteAddToCartCommand);
            ApplyFilterCommand = new RelayCommand(obj => ClearFilters());
            IncreaseQuantityCommand = new RelayCommand(ExecuteIncreaseQuantityCommand);
            DecreaseQuantityCommand = new RelayCommand(ExecuteDecreaseQuantityCommand);
        }

        private void LoadMeals()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    var meals = context.Meals
                        .Include(m => m.Category)
                        .ToList();

                    foreach (var meal in meals)
                    {
                        meal.CategoryName = meal.Category?.CategoryName;
                    }

                    _allMeals = new ObservableCollection<Meal>(meals);
                    Meals = new ObservableCollection<Meal>(_allMeals);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке блюд: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    var categories = context.MealCategories.ToList();
                    Categories = new ObservableCollection<MealCategory>(categories);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var filteredMeals = _allMeals.AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                filteredMeals = filteredMeals.Where(m => m.Name.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0);
            }

            if (SelectedCategory != null)
            {
                filteredMeals = filteredMeals.Where(m => m.CategoryID == SelectedCategory.CategoryID);
            }

            Meals = new ObservableCollection<Meal>(filteredMeals.ToList());
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = null;
            Meals = new ObservableCollection<Meal>(_allMeals);
        }

        private void ExecuteAddToCartCommand(object parameter)
        {
            var meal = parameter as Meal;
            if (meal == null)
            {
                return;
            }

            var existingItem = CartMeals.FirstOrDefault(ci => ci.Meal.MealID == meal.MealID);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                CartMeals.Add(new CartItem { Meal = meal, Quantity = 1 });
            }

            TotalPrice = CartMeals.Sum(ci => ci.TotalPrice);
        }

        private void ExecuteIncreaseQuantityCommand(object parameter)
        {
            var cartItem = parameter as CartItem;
            if (cartItem != null)
            {
                cartItem.Quantity++;
                UpdateTotalPrice();
            }
        }

        private void ExecuteDecreaseQuantityCommand(object parameter)
        {
            var cartItem = parameter as CartItem;
            if (cartItem != null)
            {
                cartItem.Quantity--;
                if (cartItem.Quantity <= 0)
                {
                    CartMeals.Remove(cartItem);
                }
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = CartMeals.Sum(ci => ci.TotalPrice); // Перерасчет общей цены корзины
        }

        private void ExecuteProfileCommand(object parameter)
        {
            if (UserSession.Instance.IsGuest)
            {
                MessageBox.Show("Вы должны быть зарегистрированы как клиент для просмотра профиля заказа.");
                NavigateToRegistrationPage();
            }
            else
            {
                NavigateToProfilePage();
            }
        }

        private void ExecuteOrderCommand(object parameter)
        {
            if (UserSession.Instance.IsGuest)
            {
                MessageBox.Show("Вы должны быть зарегистрированы как клиент для просмотра профиля.");
                NavigateToRegistrationPage();
            }
            else
            {
                ProcessOrder();
            }
        }

        private void ExecuteLogoutCommand(object parameter)
        {
            if (!UserSession.Instance.IsGuest)
            {
                UserSession.Instance.Logout();
            }

            NavigateToLoginPage();
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

        private void NavigateToProfilePage()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var profileView = new ProfileView();
            profileView.Show();

            Application.Current.MainWindow = profileView;
        }

        private void NavigateToRegistrationPage()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var registerView = new RegisterView();
            registerView.Show();

            Application.Current.MainWindow = registerView;
        }

        private void NavigateToLoginPage()
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var loginView = new MainWindow();
            loginView.Show();
        }

        private async void ProcessOrder()
        {
            try
            {
                var currentClientId = UserSession.Instance.UserId;

                if (currentClientId == null || UserSession.Instance.Role != "Client")
                {
                    MessageBox.Show("Вы должны быть авторизованы как клиент для оформления заказа.");
                    return;
                }

                if (CartMeals == null || !CartMeals.Any())
                {
                    MessageBox.Show("Ваша корзина пуста. Добавьте товары в корзину.");
                    return;
                }

                var order = new Order
                {
                    ClientID = currentClientId.Value,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    TotalAmount = CartMeals.Sum(ci => ci.TotalPrice)
                };

                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }

                foreach (var cartItem in CartMeals)
                {
                    if (cartItem?.Meal == null)
                    {
                        MessageBox.Show("Ошибка при добавлении товара в заказ.");
                        return;
                    }

                    var orderDetail = new OrderDetail
                    {
                        MealID = cartItem.Meal.MealID,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Meal.Price,
                        Order = order
                    };

                    order.OrderDetails.Add(orderDetail);
                }

                using (var context = new PlushFoodContext())
                {
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();
                }

                CartMeals.Clear();

                MessageBox.Show("Ваш заказ оформлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
