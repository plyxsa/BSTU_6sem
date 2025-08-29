using PlushFood.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlushFood.Views.UserControls
{
    public partial class AdminEditForm : UserControl
    {
        public AdminEditForm()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            var viewModel = (AdminViewModel)this.DataContext;  // Получаем доступ к ViewModel
            viewModel.Password = passwordBox.Password;  // Устанавливаем пароль в ViewModel
        }

    }
}
