using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public int? AddressId { get; set; }

    public int? PaymentMethodId { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual User? User { get; set; }
}
