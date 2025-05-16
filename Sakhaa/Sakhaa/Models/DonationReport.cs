using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class DonationReport
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? SubscriptionId { get; set; }

    public int? DonationId { get; set; }

    public DateTime? ReportDate { get; set; }

    public string? ReportName { get; set; }

    public string? ReportDescription { get; set; }

    public string? FilePath { get; set; }

    public virtual Donation? Donation { get; set; }

    public virtual Subscription? Subscription { get; set; }

    public virtual User User { get; set; } = null!;
}
