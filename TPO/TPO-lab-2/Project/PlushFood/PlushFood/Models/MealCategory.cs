using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlushFood.Models
{
    public class MealCategory
    {
        [Key]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public virtual ICollection<Meal> Meals { get; set; }
    }
}
