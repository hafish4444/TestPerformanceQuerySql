using System;
using System.Collections.Generic;

namespace SQLBenchmark.Models;

public partial class Paymenttransaction
{
    public int TransactionId { get; set; }

    public int? OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal? PaymentAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }
}
