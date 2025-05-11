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

    public decimal Amount { get; set; }

    public bool ShowAmount { get; set; }

    public string? PersonalMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }

    public string? GiftCardUrl { get; set; }

    public virtual GiftCardCustomization? GiftCardCustomization { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
