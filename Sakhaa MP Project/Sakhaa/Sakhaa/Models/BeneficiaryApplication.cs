using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class BeneficiaryApplication
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? FamilyBookUrl { get; set; }

    public string? AidVerificationDocument { get; set; }

    public string? InsuranceStatusDocument { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }
}
