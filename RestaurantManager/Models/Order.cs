using System;
using System.Collections.Generic;

namespace RestaurantManager.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table? Table { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Open"; // Open | Paid | Canceled

        public List<OrderItem> Items { get; set; } = new();
    }
}
