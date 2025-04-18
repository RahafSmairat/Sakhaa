using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class NewsEvent
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public DateTime? CreatedAt { get; set; }
}
