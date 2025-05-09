using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Product
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public string? ProductImage { get; set; }

    public decimal? Price { get; set; }

    public bool? IsAvailable { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? SponsorId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Sponsor? Sponsor { get; set; }
}
