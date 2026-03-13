using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class BuildManifest
{
    [JsonPropertyName("generated")]
    public string Generated { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("builds")]
    public List<BuildEntry> Builds { get; set; } = new();
}
