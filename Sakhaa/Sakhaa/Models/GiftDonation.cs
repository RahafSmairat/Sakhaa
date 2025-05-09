using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class GiftDonation
{
    public int Id { get; set; }

    public string? GiverName { get; set; }

    public string? GiverEmail { get; set; }

    public string? GiverPhone { get; set; }

    public string? ReceiverName { get; set; }

    public string? ReceiverEmail { get; set; }

    public string? ReceiverPhone { get; set; }

    public string? DonationType { get; set; }

    public decimal Amount { get; set; }

    public bool ShowAmount { get; set; }

    public string? PersonalMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
