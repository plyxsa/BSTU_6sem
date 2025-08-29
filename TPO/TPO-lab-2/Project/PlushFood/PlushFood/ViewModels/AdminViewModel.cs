using PlushFood.Commands;
using PlushFood.Helpers;
using PlushFood.Models;
using PlushFood.Services;
using PlushFood.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PlushFood.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        private string _password;
        private bool _isAddingNewAdmin;
        public ObservableCollection<Client> Clients { get; set; }

        public ObservableCollection<Administrator> Admins { get; set; }

        private Administrator _selectedAdmin;
        private Administrator _originalAdmin;

        public Administrator SelectedAdmin
        {
            get => _selectedAdmin;
            set
            {
                if (_selectedAdmin != value)
                {
                    _selectedAdmin = value;
                    _originalAdmin = _selectedAdmin?.Clone();

                    OnSelectedAdminChanged();
                    OnPropertyChanged(nameof(SelectedAdmin));
                }
            }
        }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnSelectedClientChanged();
                OnPropertyChanged(nameof(SelectedClient));
            }
        }

        private Meal _selectedMeal;
        public Meal SelectedMeal
        {
            get { return _selectedMeal; }
            set
            {
                _selectedMeal = value;
                OnSelectedMealChanged();
                OnPropertyChanged(nameof(SelectedMeal));
            }
        }

        private bool _isAddingNewMeal;
        public bool IsAddingNewMeal
        {
            get => _isAddingNewMeal;
            set
            {
                _isAddingNewMeal = value;
                OnPropertyChanged(nameof(IsAddingNewMeal));
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

        public bool IsAddingNewAdmin
        {
            get => _isAddingNewAdmin;
            set
            {
                _isAddingNewAdmin = value;
                OnPropertyChanged(nameof(IsAddingNewAdmin));
            }
        }

        public ObservableCollection<MealCategory> Categories { get; set; }
        private MealCategory _selectedCategory;

        public MealCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private string categoryName;
        public string CategoryName
        {
            get => categoryName;
            set
            {
                if (categoryName != value)
                {
                    categoryName = value;
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }


        private object _leftContent;
        public object LeftContent
        {
            get => _leftContent;
            set
            {
                _leftContent = value;
                OnPropertyChanged(nameof(LeftContent));
            }
        }

        private object _rightContent;
        public object RightContent
        {
            get => _rightContent;
            set
            {
                _rightContent = value;
                OnPropertyChanged(nameof(RightContent));
            }
        }

        private ObservableCollection<Meal> _mealsWithDescription;
        public ObservableCollection<Meal> MealsWithDescription
        {
            get => _mealsWithDescription;
            set
            {
                _mealsWithDescription = value;
                OnPropertyChanged(nameof(MealsWithDescription));
            }
        }
        public ObservableCollection<Order> Orders { get; set; }
        public ObservableCollection<OrderDetail> OrderDetails { get; set; }
        public ObservableCollection<string> Statuses { get; set; } 

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnSelectedOrderChanged();
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        private OrderDetail _selectedOrderDetail;
        public OrderDetail SelectedOrderDetail
        {
            get => _selectedOrderDetail;
            set
            {
                _selectedOrderDetail = value;
                OnPropertyChanged(nameof(SelectedOrderDetail));
            }
        }
        public ObservableCollection<Return> Returns { get; set; } = new ObservableCollection<Return>();
        public ObservableCollection<string> ReturnStatuses { get; set; }

        private Return _selectedReturn;
        public Return SelectedReturn
        {
            get => _selectedReturn;
            set
            {
                _selectedReturn = value;
                OnSelectedReturnChanged();
                OnPropertyChanged(nameof(SelectedReturn));
            }
        }

        public ICommand NavigateToProfileCommand { get; private set; }
        public ICommand ShowReturnsCommand { get; }
        public ICommand SaveReturnCommand { get; }
        public ICommand ShowOrdersCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand ShowMealsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CreateNewCommand { get; }
        public ICommand ShowAdminsCommand { get; }
        public ICommand ShowClientsCommand { get; }
        public ICommand SaveClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand ShowCategoriesCommand { get; }
        public ICommand SaveCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand CreateCategoryCommand { get; }
        public ICommand SaveMealCommand { get; }
        public ICommand DeleteMealCommand { get; }
        public ICommand CreateMealCommand { get; }
        public ICommand ChooseImageCommand { get; }

        public AdminViewModel()
        {
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SwitchThemeCommand = new RelayCommand(SwitchTheme);
            SaveCommand = new RelayCommand(SaveAdmin, CanModifyAdmin);
            DeleteCommand = new RelayCommand(DeleteAdmin, CanModifyAdmin);
            CreateNewCommand = new RelayCommand(CreateNewAdmin);
            ShowAdminsCommand = new RelayCommand(ShowAdmins);
            ShowClientsCommand = new RelayCommand(ShowClients);
            SaveClientCommand = new RelayCommand(SaveClient, CanModifyClient);
            DeleteClientCommand = new RelayCommand(DeleteClient, CanModifyClient);
            ShowCategoriesCommand = new RelayCommand(ShowCategories);
            SaveCategoryCommand = new RelayCommand(SaveCategory, CanModifyCategory);
            DeleteCategoryCommand = new RelayCommand(DeleteCategory, CanModifyCategory);
            CreateCategoryCommand = new RelayCommand(CreateCategory);
            ShowMealsCommand = new RelayCommand(ShowMeals);
            SaveMealCommand = new RelayCommand(SaveMeal, CanModifyMeal);
            DeleteMealCommand = new RelayCommand(DeleteMeal, CanModifyMeal);
            CreateMealCommand = new RelayCommand(CreateNewMeal);
            Orders = new ObservableCollection<Order>();
            OrderDetails = new ObservableCollection<OrderDetail>();
            Statuses = new ObservableCollection<string>();
            ShowOrdersCommand = new RelayCommand(ShowOrders);
            SaveOrderCommand = new RelayCommand(SaveOrder, CanModifyOrder);
            ShowReturnsCommand = new RelayCommand(ShowReturns);
            SaveReturnCommand = new RelayCommand(SaveReturn, CanModifyReturn);
            NavigateToProfileCommand = new RelayCommand(NavigateToProfile);
            ChooseImageCommand = new RelayCommand(ChooseImage);

            LoadStatuses();
            LoadCategories();
            LoadAdmins();
            LoadClients();
            LoadAdminControls();
            LoadReturnStatuses();
        }

        private void LoadReturns()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    var returns = context.Returns
                        .Include(r => r.Order)
                        .ToList();

                    Returns.Clear();
                    foreach (var returnItem in returns)
                    {
                        Returns.Add(returnItem);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Ошибка при загрузке возвратов: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВложенная ошибка: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }

                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReturnStatuses()
        {
            // Загрузка возможных статусов возвратов
            ReturnStatuses = new ObservableCollection<string>
            {
                "Pending",
                "Approved",
                "Rejected"
            };
        }

        private void ShowReturns(object parameter)
        {
            try
            {
                LoadReturns();

                LeftContent = new ReturnTable { DataContext = this };
                RightContent = new ReturnEditControl { DataContext = this };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении возвратов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSelectedReturnChanged()
        {
            RightContent = new ReturnEditControl { DataContext = this };
        }

        private void SaveReturn(object parameter)
        {
            if (SelectedReturn == null)
                return;

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var returnItem = context.Returns.FirstOrDefault(r => r.ReturnID == SelectedReturn.ReturnID);
                    if (returnItem != null)
                    {
                        returnItem.Status = SelectedReturn.Status;
                        context.SaveChanges();
                        MessageBox.Show("Статус возврата успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении статуса возврата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModifyReturn(object parameter)
        {
            return SelectedReturn != null;
        }

        private void LoadStatuses()
        {
            try
            {
                var predefinedStatuses = new List<string> { "Pending", "In Progress", "Delivered", "Cancelled" };

                Statuses.Clear();
                foreach (var status in predefinedStatuses)
                {
                    Statuses.Add(status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    var orders = context.Orders.Include(o => o.Client).ToList();
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderDetails()
        {
            if (SelectedOrder == null)
            {
                OrderDetails.Clear();
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var details = context.OrderDetails
                        .Include(od => od.Meal)
                        .Where(od => od.OrderID == SelectedOrder.OrderID)
                        .ToList();

                    if (details == null || !details.Any())
                    {
                        MessageBox.Show($"Для заказа с ID {SelectedOrder.OrderID} не найдены детали.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    OrderDetails.Clear();
                    foreach (var detail in details)
                    {
                        OrderDetails.Add(detail);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowOrders(object parameter)
        {
            try
            {
                LoadOrders();

                LeftContent = new OrderTable { DataContext = this };
                RightContent = new OrderEditControl { DataContext = this };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSelectedOrderChanged()
        {
            LoadOrderDetails();
        }

        private void SaveOrder(object parameter)
        {
            if (SelectedOrder == null)
                return;

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var order = context.Orders.FirstOrDefault(o => o.OrderID == SelectedOrder.OrderID);
                    if (order != null)
                    {
                        order.Status = SelectedOrder.Status;
                        context.SaveChanges();
                        MessageBox.Show("Изменения успешно сохранены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModifyOrder(object parameter)
        {
            return SelectedOrder != null;
        }

        private void LoadMeals()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    var meals = context.Meals.ToList();
                    var categories = context.MealCategories.ToDictionary(c => c.CategoryID, c => c.CategoryName);

                    MealsWithDescription = new ObservableCollection<Meal>(meals.Select(meal => new Meal
                    {
                        MealID = meal.MealID,
                        Name = meal.Name,
                        CategoryName = categories.ContainsKey(meal.CategoryID) ? categories[meal.CategoryID] : "Неизвестная категория",
                        Price = meal.Price,
                        IsAvailable = meal.IsAvailable,
                        Description = meal.Description,
                        Calories = meal.Calories,
                        Proteins = meal.Proteins,
                        Fats = meal.Fats,
                        Carbohydrates = meal.Carbohydrates,
                        ImagePath = meal.ImagePath
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке блюд: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowMeals(object parameter)
        {
            try
            {
                LoadMeals();

                LeftContent = new MealTableControl();

                RightContent = new MealEditControl { DataContext = this };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении блюд: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMeal(object parameter)
        {
            if (SelectedMeal == null)
            {
                MessageBox.Show("Не выбрано блюдо.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedMeal.Name) || SelectedMeal.CategoryID == 0)
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(SelectedMeal.Price.ToString(), out _) ||
                !decimal.TryParse(SelectedMeal.Calories.ToString(), out _) ||
                !decimal.TryParse(SelectedMeal.Proteins.ToString(), out _) ||
                !decimal.TryParse(SelectedMeal.Fats.ToString(), out _) ||
                !decimal.TryParse(SelectedMeal.Carbohydrates.ToString(), out _))
            {
                MessageBox.Show("Некоторые числовые значения имеют неверный формат. Убедитесь, что вы ввели корректные числа.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    if (IsAddingNewMeal)
                    {
                        var newMeal = new Meal
                        {
                            Name = SelectedMeal.Name,
                            CategoryID = SelectedMeal.CategoryID,
                            Price = SelectedMeal.Price,
                            IsAvailable = SelectedMeal.IsAvailable,
                            Description = SelectedMeal.Description,
                            Calories = SelectedMeal.Calories,
                            Proteins = SelectedMeal.Proteins,
                            Fats = SelectedMeal.Fats,
                            Carbohydrates = SelectedMeal.Carbohydrates,
                            ImagePath = SelectedMeal.ImagePath
                        };

                        context.Meals.Add(newMeal);
                    }
                    else
                    {
                        var mealInDb = context.Meals.Find(SelectedMeal.MealID);

                        if (mealInDb != null)
                        {
                            mealInDb.Name = SelectedMeal.Name;
                            mealInDb.CategoryID = SelectedMeal.CategoryID;
                            mealInDb.Price = SelectedMeal.Price;
                            mealInDb.IsAvailable = SelectedMeal.IsAvailable;
                            mealInDb.Description = SelectedMeal.Description;
                            mealInDb.Calories = SelectedMeal.Calories;
                            mealInDb.Proteins = SelectedMeal.Proteins;
                            mealInDb.Fats = SelectedMeal.Fats;
                            mealInDb.Carbohydrates = SelectedMeal.Carbohydrates;

                            if (!string.IsNullOrEmpty(SelectedMeal.ImagePath))
                                mealInDb.ImagePath = SelectedMeal.ImagePath;
                        }
                    }

                    context.SaveChanges();
                }

                LoadMeals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении блюда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsDecimalValid(decimal value)
        {
            return value >= 0;
        }

        private void DeleteMeal(object parameter)
        {
            if (SelectedMeal == null)
                return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить блюдо \"{SelectedMeal.Name}\"?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var meal = context.Meals.FirstOrDefault(m => m.MealID == SelectedMeal.MealID);
                    if (meal == null)
                    {
                        MessageBox.Show("Блюдо не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    bool isUsedInOrders = context.OrderDetails.Any(od => od.MealID == SelectedMeal.MealID);
                    if (isUsedInOrders)
                    {
                        MessageBox.Show("Невозможно удалить блюдо, так как оно используется в заказах.",
                                        "Ошибка",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        return;
                    }

                    context.Meals.Remove(meal);
                    context.SaveChanges();

                    MessageBox.Show("Блюдо успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadMeals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении блюда: {ex.Message}\n\nСтек вызовов:\n{ex.StackTrace}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

        }

        private bool CanModifyMeal(object parameter)
        {
            return SelectedMeal != null;
        }

        private void CreateNewMeal(object parameter)
        {
            SelectedMeal = new Meal
            {
                ImagePath = string.Empty
            };
            IsAddingNewMeal = true;
            OnPropertyChanged(nameof(SelectedMeal));
        }

        private void ChooseImage(object parameter)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите изображение блюда",
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(openFileDialog.FileName)}";
                    string filePath = Path.Combine(imagesDirectory, fileName);

                    File.Copy(openFileDialog.FileName, filePath);

                    SelectedMeal.ImagePath = filePath;
                    OnPropertyChanged(nameof(SelectedMeal));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnSelectedMealChanged()
        {
            IsAddingNewMeal = SelectedMeal != null && SelectedMeal.MealID == 0;

            OnPropertyChanged(nameof(SelectedMeal));
            OnPropertyChanged(nameof(IsAddingNewMeal));
        }

        private void LoadAdmins()
        {
            using (var context = new PlushFoodContext())
            {
                Admins = new ObservableCollection<Administrator>(context.Administrators.ToList());
            }
            OnPropertyChanged(nameof(Admins));
        }

        private void LoadClients()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    Clients = new ObservableCollection<Client>(context.Clients.ToList());
                }
                OnPropertyChanged(nameof(Clients));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAdminControls()
        {
            ShowAdmins(null);
        }

        private void ShowAdmins(object parameter)
        {
            try
            {
                LeftContent = new AdminsTable();
                RightContent = new AdminEditForm { DataContext = this };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении администраторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowClients(object parameter)
        {
            try
            {
                LeftContent = new ClientsTable();
                RightContent = new ClientEditForm { DataContext = this };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveAdmin(object parameter)
        {
            if (SelectedAdmin == null)
            {
                MessageBox.Show("Не выбран администратор.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedAdmin.UserName) || string.IsNullOrWhiteSpace(SelectedAdmin.Email))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidEmail(SelectedAdmin.Email))
            {
                MessageBox.Show("Неверный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(SelectedAdmin.UserName, @"^[a-zA-Z0-9._]{5,40}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, символы '.' и '_', и должен быть длиной от 5 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(Password) && !Regex.IsMatch(Password, @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$"))
            {
                MessageBox.Show("Пароль должен содержать минимум 1 букву, 1 цифру и 1 спецсимвол, и быть не пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    var existingAdmin = context.Administrators
                        .FirstOrDefault(a => a.UserName == SelectedAdmin.UserName && a.AdminID != SelectedAdmin.AdminID);

                    if (existingAdmin != null)
                    {
                        MessageBox.Show("Администратор с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoadAdmins();
                        return;
                    }

                    if (SelectedAdmin.AdminID == 0)
                    {
                        var newAdmin = new Administrator
                        {
                            UserName = SelectedAdmin.UserName,
                            Email = SelectedAdmin.Email,
                            PasswordHash = string.IsNullOrWhiteSpace(Password) ? null : ComputeHash(Password)
                        };
                        context.Administrators.Add(newAdmin);
                        context.SaveChanges();
                    }
                    else
                    {
                        var adminInDb = context.Administrators.Find(SelectedAdmin.AdminID);
                        if (adminInDb != null)
                        {
                            adminInDb.UserName = SelectedAdmin.UserName;
                            adminInDb.Email = SelectedAdmin.Email;

                            if (!string.IsNullOrWhiteSpace(Password))
                            {
                                adminInDb.PasswordHash = ComputeHash(Password);
                            }
                        }
                        
                    }
                    MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveClient(object parameter)
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Не выбран клиент.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedClient.UserName) || string.IsNullOrWhiteSpace(SelectedClient.Email) || string.IsNullOrWhiteSpace(SelectedClient.PhoneNumber))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidEmail(SelectedClient.Email))
            {
                MessageBox.Show("Неверный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidPhoneNumber(SelectedClient.PhoneNumber))
            {
                MessageBox.Show("Неверный формат номера телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка логина клиента (только латинские буквы, цифры, спецсимволы '.' и '_', длина от 5 до 40 символов)
            if (!Regex.IsMatch(SelectedClient.UserName, @"^[a-zA-Z0-9._]{5,40}$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, символы '.' и '_', и должен быть длиной от 5 до 40 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new PlushFoodContext())
            {
                var existingClient = context.Clients.FirstOrDefault(c => c.UserName == SelectedClient.UserName);
                if (existingClient != null && existingClient.ClientID != SelectedClient.ClientID)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadClients();
                    return;
                }

                var clientInDb = context.Clients.Find(SelectedClient.ClientID);
                if (clientInDb != null)
                {
                    clientInDb.UserName = SelectedClient.UserName;
                    clientInDb.Email = SelectedClient.Email;
                    clientInDb.PhoneNumber = SelectedClient.PhoneNumber;
                }

                context.SaveChanges();
            }

            MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteClient(object parameter)
        {
            try
            {
                if (SelectedClient == null)
                {
                    MessageBox.Show("Не выбран клиент.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить клиента?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var context = new PlushFoodContext())
                    {
                        var clientInDb = context.Clients
                            .FirstOrDefault(c => c.ClientID == SelectedClient.ClientID);

                        if (clientInDb != null)
                        {
                            context.Clients.Remove(clientInDb);
                            context.SaveChanges();
                        }
                    }

                    LoadClients();
                    SelectedClient = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanModifyClient(object parameter)
        {
            return SelectedClient != null;
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

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            try
            {
                var phoneRegex = new System.Text.RegularExpressions.Regex(
                    @"^\+375[0-9]{9}$"); // Белорусский формат номера телефона
                return phoneRegex.IsMatch(phoneNumber);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DeleteAdmin(object parameter)
        {
            try
            {
                if (SelectedAdmin == null)
                {
                    MessageBox.Show("Не выбран администратор.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить администратора?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var context = new PlushFoodContext())
                    {
                        var adminInDb = context.Administrators.Find(SelectedAdmin.AdminID);
                        if (adminInDb != null)
                        {
                            context.Administrators.Remove(adminInDb);
                            context.SaveChanges();
                        }
                    }
                    LoadAdmins();
                    SelectedAdmin = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении администратора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewAdmin(object parameter)
        {
            SelectedAdmin = new Administrator();
            Password = null;
            IsAddingNewAdmin = true;
            OnPropertyChanged(nameof(SelectedAdmin));
        }

        private void OnSelectedAdminChanged()
        {
            Password = null;
            IsAddingNewAdmin = SelectedAdmin != null && SelectedAdmin.AdminID == 0;
            OnPropertyChanged(nameof(IsAddingNewAdmin));
        }

        private void OnSelectedClientChanged()
        {
            OnPropertyChanged(nameof(SelectedClient));
        }

        private bool CanModifyAdmin(object parameter)
        {
            return SelectedAdmin != null;
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new PlushFoodContext())
                {
                    Categories = new ObservableCollection<MealCategory>(context.MealCategories.ToList());
                }
                OnPropertyChanged(nameof(Categories));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowCategories(object parameter)
        {
            LeftContent = new CategoryTableControl { DataContext = this };
            RightContent = new CategoryEditControl { DataContext = this };
        }

        private void SaveCategory(object parameter)
        {
            if (SelectedCategory == null)
            {
                SelectedCategory = new MealCategory();
            }

            if (string.IsNullOrWhiteSpace(SelectedCategory.CategoryName))
            {
                MessageBox.Show("Укажите название категории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new PlushFoodContext())
                {
                    if (SelectedCategory.CategoryID == 0)
                    {
                        context.MealCategories.Add(SelectedCategory);
                    }
                    else
                    {
                        var categoryInDb = context.MealCategories.Find(SelectedCategory.CategoryID);
                        if (categoryInDb != null)
                        {
                            categoryInDb.CategoryName = SelectedCategory.CategoryName;
                        }
                    }

                    context.SaveChanges();
                }

                LoadCategories();
                SelectedCategory = null;
                MessageBox.Show("Категория успешно сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategory(object parameter)
        {
            try
            {
                if (SelectedCategory == null)
                {
                    MessageBox.Show("Не выбрана категория.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить категорию?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var context = new PlushFoodContext())
                    {
                        // Проверяем, не используется ли категория в других сущностях (например, блюда)
                        var mealsWithCategory = context.Meals
                            .Where(m => m.CategoryID == SelectedCategory.CategoryID)
                            .ToList();

                        if (mealsWithCategory.Any())
                        {
                            MessageBox.Show("Невозможно удалить категорию, так как она используется в блюдах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var categoryInDb = context.MealCategories
                            .FirstOrDefault(c => c.CategoryID == SelectedCategory.CategoryID);

                        if (categoryInDb != null)
                        {
                            context.MealCategories.Remove(categoryInDb);
                            context.SaveChanges();
                        }
                    }
                    LoadCategories();
                    SelectedCategory = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении категории: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateCategory(object parameter)
        {
            SelectedCategory = new MealCategory();
        }

        private bool CanModifyCategory(object parameter) => SelectedCategory != null;

        private string ComputeHash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        private void ExecuteLogout(object parameter)
        {
            UserSession.Instance.Logout();

            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var loginView = new MainWindow();
            loginView.Show();

            Application.Current.MainWindow = loginView;
        }

        private void NavigateToProfile(object parameter)
        {
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();

            var profileView = new Views.ProfileViewAdmin();
            profileView.Show();

            Application.Current.MainWindow = profileView;
        }

        private void SwitchTheme(object parameter)
        {
            _isDarkTheme = !_isDarkTheme;
            var theme = _isDarkTheme ? "DarkTheme" : "LightTheme";
            var uri = new Uri($"Themes/{theme}.xaml", UriKind.Relative);
            var resourceDictionary = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
