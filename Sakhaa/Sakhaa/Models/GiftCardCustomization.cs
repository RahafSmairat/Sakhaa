using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class GiftCardCustomization
{
    public int Id { get; set; }

    public int? GiftDonationId { get; set; }

    public string? DecorationImageUrl { get; set; }

    public string? OccasionImageUrl { get; set; }

    public string? TextColor { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GiftDonation? GiftDonation { get; set; }
}
