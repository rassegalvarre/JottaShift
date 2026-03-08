using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.Jottacloud;

public record AlbumItem
{
    public required string ImageName { get; init; }
    public DateTime CapturedDate { get; init; } // Needs to be converted from Long
    public string? LocalFilePath { get; init; }
}
