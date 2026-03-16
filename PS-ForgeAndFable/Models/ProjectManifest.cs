using System.Text.Json.Serialization;

namespace ForgeAndFable.Models;

public class ProjectManifest
{
    [JsonPropertyName("projects")]
    public List<ProjectEntry> Projects { get; set; } = new();
}
