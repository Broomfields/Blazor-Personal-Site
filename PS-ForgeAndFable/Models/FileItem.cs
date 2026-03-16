using System.Text.Json.Serialization;

namespace ForgeAndFable.Models;

/// <summary>
/// A single downloadable file attached to a build.
/// <c>Src</c> is a relative path or bare stem (e.g. "files/01-bracket.stl" or "01-bracket").
/// <c>Label</c> is the display name shown in the UI (e.g. "Bracket Cap — STL").
/// </summary>
public class FileItem
{
    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
