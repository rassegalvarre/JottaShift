using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;

public record FileTransferJob : FileExportJob
{
    [JsonPropertyName("target_directory_path")]
    public required string TargetDirectoryPath { get; init; }
}
