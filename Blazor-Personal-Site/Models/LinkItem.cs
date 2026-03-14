using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

/// <summary>
/// An external appearance of a build (e.g. on Printables, MakerWorld, Thingiverse).
/// <c>Label</c> is the display name of the platform or link.
/// <c>Url</c> is the full URL to the listing.
/// </summary>
public class LinkItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
