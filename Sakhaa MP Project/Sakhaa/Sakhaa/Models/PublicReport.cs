using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class PublicReport
{
    public int Id { get; set; }

    public int ReportYear { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string FilePath { get; set; } = null!;

    public string ReportFileName { get; set; } = null!;
}
