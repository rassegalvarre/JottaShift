using System.Text.Json.Serialization;

namespace JottaShift.Core.Configuration;

public class ExportSettings
{
    [JsonPropertyName("jottacloud_timeline")]
    public required FileExportMapping JottacloudTimeline { get; init; }

    [JsonPropertyName("steam_screenshots")]
    public required FileExportMapping SteamScreenshots { get; init; }

    [JsonPropertyName("desktop_wallpapers_4k")]
    public required FileExportMapping DesktopWallpapers4k { get; init; }

    [JsonPropertyName("desktop_wallpapers_1440p")]
    public required FileExportMapping DesktopWallpapers1440p { get; init; }

    [JsonPropertyName("chromecast_photos")]
    public required GooglePhotosUploadMapping ChromecastPhotos { get; init; }
}

public abstract record BaseFileExportMapping()
{
    [JsonPropertyName("source_directory_path")]
    public required string SourceDirectoryPath { get; init; }
}

public record FileExportMapping
{
    [JsonPropertyName("target_directory_path")]
    public required string TargetDirectoryPath { get; init; }
}

public record GooglePhotosUploadMapping : BaseFileExportMapping
{
    [JsonPropertyName("album_name")]
    public required string AlbumName { get; init; }
}