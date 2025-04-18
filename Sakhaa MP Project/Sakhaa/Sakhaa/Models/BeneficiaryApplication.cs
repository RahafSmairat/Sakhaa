using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class BeneficiaryApplication
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string FamilyBookUrl { get; set; } = null!;

    public string AidVerificationDocument { get; set; } = null!;

    public string InsuranceStatusDocument { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
}
