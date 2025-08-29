using PlushFood.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlushFood
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализация базы данных
            Database.SetInitializer(new CreateDatabaseIfNotExists<PlushFoodContext>());
        }
    }
}
