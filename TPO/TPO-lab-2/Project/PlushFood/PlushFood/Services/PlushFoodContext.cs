using PlushFood.Models;
using System.Data.Entity;

namespace PlushFood.Services
{
    internal class PlushFoodContext : DbContext
    {
        public PlushFoodContext() : base("name=PlushFoodDb")
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<MealCategory> MealCategories { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MealDescription> MealDescriptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Return> Returns { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Meal)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(od => od.MealID);

            modelBuilder.Entity<Return>()
                .HasRequired(r => r.Order)
                .WithMany(o => o.Returns)
                .HasForeignKey(r => r.OrderID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<MealCategory>()
                .HasMany(mc => mc.Meals)
                .WithRequired(m => m.Category)
                .HasForeignKey(m => m.CategoryID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Meal>()
                .HasMany(m => m.OrderDetails)
                .WithRequired(od => od.Meal)
                .HasForeignKey(od => od.MealID)
                .WillCascadeOnDelete(true); 
        }
    }
}
