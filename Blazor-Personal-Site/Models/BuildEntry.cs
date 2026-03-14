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

    // ── New fields (manifest v2+) ─────────────────────────────────────────────

    /// <summary>
    /// Descriptive alt text for the cover image. Suitable for screen readers
    /// without being redundant with the title. Falls back to <see cref="Title"/>
    /// when absent.
    /// </summary>
    [JsonPropertyName("cover_alt")]
    public string? CoverAlt { get; set; }

    /// <summary>
    /// Ordered gallery images. Each entry has a resolved relative <c>Src</c>
    /// path and a descriptive <c>Label</c> (suitable as alt text / caption).
    /// Absent when the build has no gallery.
    /// </summary>
    [JsonPropertyName("gallery")]
    public List<GalleryItem>? Gallery { get; set; }

    /// <summary>
    /// When <c>true</c> the build should be pinned / highlighted in the UI.
    /// The field is only present in the manifest when its value is <c>true</c>;
    /// absent entries deserialise to the default <c>false</c>.
    /// </summary>
    [JsonPropertyName("featured")]
    public bool Featured { get; set; }

    /// <summary>
    /// SPDX or Creative Commons license identifier for the design
    /// (e.g. "CC BY-SA 4.0", "CC0"). Intended for display alongside
    /// metadata or downloadable files.
    /// </summary>
    [JsonPropertyName("license")]
    public string? License { get; set; }

    /// <summary>
    /// External appearances of the build (Printables, MakerWorld, etc.).
    /// Each entry has a <c>Label</c> (display name) and a <c>Url</c>.
    /// Absent when the build has no external links.
    /// </summary>
    [JsonPropertyName("links")]
    public List<LinkItem>? Links { get; set; }

    /// <summary>
    /// Third-party assets or designs used in this build.
    /// Positioned at the bottom of the detail page as attribution.
    /// Absent when no credits are declared.
    /// </summary>
    [JsonPropertyName("credits")]
    public List<CreditItem>? Credits { get; set; }
}
