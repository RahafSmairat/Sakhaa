using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class CorporatePartnership
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public DateTime? PartnershipDate { get; set; }
}
