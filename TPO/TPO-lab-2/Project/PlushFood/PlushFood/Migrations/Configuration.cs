namespace PlushFood.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<PlushFood.Services.PlushFoodContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(PlushFood.Services.PlushFoodContext context)
        {
        }
    }
}
