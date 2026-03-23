using System.Text.Json.Serialization;

namespace JottaShift.Core.Jottacloud.Models.Domain;

public record ShareInfo
{
    [JsonPropertyName("admin")]
    public bool Admin { get; set; }

    [JsonPropertyName("authorization")]
    public string Authorization { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("ownerFullName")]
    public string OwnerFullName { get; set; } = string.Empty;

    [JsonPropertyName("ownerAvatar")]
    public OwnerAvatar OwnerAvatar { get; set; } = new();

    [JsonPropertyName("shareDate")]
    public long ShareDate { get; set; }

    [JsonPropertyName("subscribers")]
    public IEnumerable<object> Subscribers { get; set; } = Enumerable.Empty<object>();

    [JsonPropertyName("subscriptionDate")]
    public long SubscriptionDate { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}
