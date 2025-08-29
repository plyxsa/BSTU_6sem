using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlushFood.Models
{
    public class Meal
    {
        [Key]
        public int MealID { get; set; }

        public string Name { get; set; }
        public int CategoryID { get; set; }

        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public string Description { get; set; }

        public decimal Calories { get; set; }

        public decimal Proteins { get; set; }

        public decimal Fats { get; set; }

        public decimal Carbohydrates { get; set; }

        [NotMapped] 
        public string CategoryName { get; set; }

        public string ImagePath { get; set; }

        public virtual MealCategory Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
