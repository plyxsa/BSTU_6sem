using System.ComponentModel.DataAnnotations;

namespace PlushFood.Models
{
    public class MealDescription
    {
        [Key]
        public int DescriptionID { get; set; }
        public int MealID { get; set; }
        public string Description { get; set; }
        public decimal Calories { get; set; }
        public decimal Proteins { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbohydrates { get; set; }

        public Meal Meal { get; set; }
    }
}
