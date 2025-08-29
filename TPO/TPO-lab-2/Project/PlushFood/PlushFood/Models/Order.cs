using System;
using System.Collections.Generic;

namespace PlushFood.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int ClientID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public Client Client { get; set; }

        public ICollection<Return> Returns { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

