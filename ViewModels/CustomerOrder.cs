using SQLBenchmark.Models;
using System;
using System.Collections.Generic;

namespace SQLBenchmark.ViewModels;

public class CustomerOrder
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual List<Order> Orders { get; set; } = new List<Order>();
    
    public class Order
    {
        public int OrderId { get; set; }


        public DateTime OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public string? ShippingAddress { get; set; }

        public string? Status { get; set; }

        public int? TotalOrder { get; set; }
        public decimal? TotalOrderValue { get; set; }
    }
}
