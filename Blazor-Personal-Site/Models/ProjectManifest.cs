using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class ProjectManifest
{
    [JsonPropertyName("projects")]
    public List<ProjectEntry> Projects { get; set; } = new();
}
