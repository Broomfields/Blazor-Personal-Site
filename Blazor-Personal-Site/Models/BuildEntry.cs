using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class BuildEntry
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("cover")]
    public string Cover { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("cad_tool")]
    public string CadTool { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Bare filename stems of sub-pages for this build.
    /// Absent (null) when the build has no sub-pages.
    /// Never an empty list — the manifest simply omits the field.
    /// </summary>
    [JsonPropertyName("subpages")]
    public List<string>? Subpages { get; set; }
}
