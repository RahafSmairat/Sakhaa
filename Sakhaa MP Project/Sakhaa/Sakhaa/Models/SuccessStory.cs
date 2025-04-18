using System;
using System.Collections.Generic;

namespace Sakhaa.Models;

public partial class SuccessStory
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? ImagePath { get; set; }
}
