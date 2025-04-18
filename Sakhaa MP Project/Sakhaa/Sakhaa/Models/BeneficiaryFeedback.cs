using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class BeneficiaryFeedback
{
    public int Id { get; set; }

    public string BeneficiaryName { get; set; } = null!;

    public string Feedback { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
