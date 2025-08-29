namespace PlushFood.Models
{
    public class MealWithDescription
    {
        public int MealID { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; } 
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }
        public decimal Calories { get; set; }
        public decimal Proteins { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbohydrates { get; set; }
    }
}
