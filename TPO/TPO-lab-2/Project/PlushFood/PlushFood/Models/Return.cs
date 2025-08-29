using System;

namespace PlushFood.Models
{
    public class Return
    {
        public int ReturnID { get; set; }
        public int OrderID { get; set; }
        public string Reason { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Status { get; set; }
        public Order Order { get; set; }
    }
}
