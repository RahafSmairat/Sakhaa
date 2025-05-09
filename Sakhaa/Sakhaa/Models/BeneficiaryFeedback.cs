using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class BeneficiaryFeedback
{
    public int Id { get; set; }

    public string? BeneficiaryName { get; set; }

    public string? Feedback { get; set; }

    public DateTime? CreatedAt { get; set; }
}
