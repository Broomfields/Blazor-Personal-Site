using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

/// <summary>
/// A third-party asset or design referenced by a build.
/// <c>License</c> is optional — only present when the source specifies one.
/// </summary>
public class CreditItem
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// SPDX or Creative Commons identifier for the credited asset.
    /// Optional — may be absent even when the parent build has a <c>license</c> field.
    /// </summary>
    [JsonPropertyName("license")]
    public string? License { get; set; }
}
