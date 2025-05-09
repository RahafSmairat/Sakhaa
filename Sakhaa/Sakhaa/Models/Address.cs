using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public int? UserId { get; set; }

    public string? Title { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
