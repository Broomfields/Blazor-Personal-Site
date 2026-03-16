using System.Text.Json.Serialization;

namespace ForgeAndFable.Models;

public class BuildManifest
{
    [JsonPropertyName("builds")]
    public List<BuildEntry> Builds { get; set; } = new();
}
