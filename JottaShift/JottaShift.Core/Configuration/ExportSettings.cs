using System.Text.Json.Serialization;

public class ExportSettings
{
    [JsonPropertyName("jottacloud_timeline")]
    public required JottacloudTimeline JottacloudTimeline { get; init; }

    [JsonPropertyName("steam_screenshots")]
    public required SteamScreenshots SteamScreenshots { get; init; }

    [JsonPropertyName("chromecast_photos")]
    public required ChromecastPhotos ChromecastPhotos { get; init; }

    [JsonPropertyName("desktop_wallpapers_4k")]
    public required DesktopWallpapers4k DesktopWallpapers4k { get; init; }

    [JsonPropertyName("desktop_wallpapers_1440p")]
    public required DesktopWallpapers1440p DesktopWallpapers1440p { get; init; }
}

/* ---------- Individual sections ---------- */

public class JottacloudTimeline
{
    [JsonPropertyName("staging_directory_path")]
    public required string StagingDirectoryPath { get; init; }

    [JsonPropertyName("target_directory")]
    public required string TargetDirectory { get; init; }
}

public class SteamScreenshots
{
    [JsonPropertyName("staging_directory_path")]
    public required string StagingDirectoryPath { get; init; }

    [JsonPropertyName("target_directory")]
    public required string TargetDirectory { get; init; }
}

public class ChromecastPhotos
{
    [JsonPropertyName("staging_directory_path")]
    public required string StagingDirectoryPath { get; init; }

    [JsonPropertyName("album_name")]
    public required string AlbumName { get; init; }
}

public class DesktopWallpapers4k
{
    [JsonPropertyName("staging_directory_path")]
    public required string StagingDirectoryPath { get; init; }

    [JsonPropertyName("target_directory")]
    public required string TargetDirectory { get; init; }
}

public class DesktopWallpapers1440p
{
    [JsonPropertyName("staging_directory_path")]
    public required string StagingDirectoryPath { get; init; }

    [JsonPropertyName("target_directory")]
    public required string TargetDirectory { get; init; }
}