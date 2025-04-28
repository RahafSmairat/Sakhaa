using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class NewsEvent
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime EventDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Image { get; set; }
}
