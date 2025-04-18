using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class GiftDonation
{
    public int Id { get; set; }

    public string GiverName { get; set; } = null!;

    public string GiverEmail { get; set; } = null!;

    public string? GiverPhone { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string ReceiverEmail { get; set; } = null!;

    public string? ReceiverPhone { get; set; }

    public string DonationType { get; set; } = null!;

    public decimal Amount { get; set; }

    public bool IncludeReport { get; set; }

    public bool ShowAmount { get; set; }

    public string? PersonalMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
