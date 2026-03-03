using System.Text.Json.Serialization;

namespace JottaShift.Core.FileExportOrchestrator.Jobs;

public abstract record FileExportJob()
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("source_directory_path")]
    public required string SourceDirectoryPath { get; init; }

    [JsonPropertyName("delete_source_files")]
    public required bool DeleteSourceFiles { get; init; }

    [JsonPropertyName("enabled")]
    public required bool Enabled { get; init; }
}
