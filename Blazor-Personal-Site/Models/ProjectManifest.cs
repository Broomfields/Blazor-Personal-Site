using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class ProjectManifest
{
    [JsonPropertyName("generated")]
    public string Generated { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("projects")]
    public List<ProjectEntry> Projects { get; set; } = new();
}
