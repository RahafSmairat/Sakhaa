using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Project
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? TargetAmount { get; set; }

    public decimal? CurrentAmount { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
