using System.ComponentModel.DataAnnotations;

namespace PlushFood.Models
{
    public class Administrator
    {
        [Key]
        public int AdminID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public Administrator Clone()
        {
            return (Administrator)this.MemberwiseClone();
        }
    }

}
