using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class DonationProgram
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? MinimumDonationAmount { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
