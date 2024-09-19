using System;
using System.Collections.Generic;

namespace SQLBenchmark.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Status { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    //public virtual ICollection<Paymenttransaction> Paymenttransactions { get; set; } = new List<Paymenttransaction>();
}
