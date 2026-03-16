using System.Text.Json.Serialization;

namespace ForgeAndFable.Models;

/// <summary>
/// A single entry in a build's gallery array.
/// <c>Src</c> is a resolved relative path (e.g. "images/01-model-study.png").
/// <c>Label</c> is descriptive text suitable for use as <c>alt</c> text or a caption.
/// </summary>
public class GalleryItem
{
    [JsonPropertyName("src")]
    public string Src { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
