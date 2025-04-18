using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? DonationId { get; set; }

    public int? SubscriptionId { get; set; }

    public int? GiftDonationId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual Donation? Donation { get; set; }

    public virtual GiftDonation? GiftDonation { get; set; }

    public virtual Subscription? Subscription { get; set; }

    public virtual User User { get; set; } = null!;
}
