using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class PaymentMethod
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? CardHolderName { get; set; }

    public string? CardNumber { get; set; }

    public string? ExpiryDate { get; set; }

    public string? Cvv { get; set; }

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
