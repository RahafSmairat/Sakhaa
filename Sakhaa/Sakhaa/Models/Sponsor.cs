using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Sponsor
{
    public int SponsorId { get; set; }

    public string? Name { get; set; }

    public string? LogoPath { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
