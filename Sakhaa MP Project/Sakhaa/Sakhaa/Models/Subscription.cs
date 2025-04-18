using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProgramId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<DonationReport> DonationReports { get; set; } = new List<DonationReport>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual DonationProgram Program { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
