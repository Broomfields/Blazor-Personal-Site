using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class BuildManifest
{
    [JsonPropertyName("builds")]
    public List<BuildEntry> Builds { get; set; } = new();
}
