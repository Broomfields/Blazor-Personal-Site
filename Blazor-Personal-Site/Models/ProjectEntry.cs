using System.Text.Json.Serialization;

namespace Blazor_Personal_Site.Models;

public class ProjectEntry
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

    /// <summary>
    /// Descriptive alt text for the cover image. Falls back to <see cref="Title"/>
    /// when absent.
    /// </summary>
    [JsonPropertyName("cover_alt")]
    public string? CoverAlt { get; set; }

    /// <summary>
    /// Ordered gallery images. Each entry has a resolved relative <c>Src</c>
    /// path and a descriptive <c>Label</c>. Absent when the project has no gallery.
    /// </summary>
    [JsonPropertyName("gallery")]
    public List<GalleryItem>? Gallery { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When <c>true</c> the project should be pinned / highlighted in the UI.
    /// </summary>
    [JsonPropertyName("featured")]
    public bool IsFeatured { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// GitHub repository URL, e.g. "https://github.com/Broomfields/Love2D-Chess".
    /// Used to build the GitHub link button and fetch live repo stats.
    /// Absent when the project has no public repo.
    /// </summary>
    [JsonPropertyName("repo")]
    public string? Repo { get; set; }

    /// <summary>
    /// Programming languages used (e.g. ["Lua", "C#"]). Displayed as coloured
    /// language badges on both the card and the detail page.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = new();

    /// <summary>
    /// Runtime or build-time dependencies (e.g. ["LÖVE 11+", "busted"]).
    /// Shown as a simple list on the detail page.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// SPDX or Creative Commons license identifier (e.g. "MIT", "CC BY-SA 4.0").
    /// </summary>
    [JsonPropertyName("license")]
    public string? License { get; set; }

    /// <summary>
    /// Supported platforms (e.g. ["Windows", "macOS", "Linux"]).
    /// Shown as small platform badges on the detail page.
    /// </summary>
    [JsonPropertyName("platform")]
    public List<string> Platform { get; set; } = new();

    /// <summary>
    /// URL to a live demo or hosted version of the project.
    /// Shown as a "Try it" link button on the detail page.
    /// </summary>
    [JsonPropertyName("demo_url")]
    public string? DemoUrl { get; set; }

    /// <summary>
    /// Embeddable URL (e.g. an iframe src) for an interactive demo.
    /// When present, rendered as an embedded preview on the detail page.
    /// </summary>
    [JsonPropertyName("demo_embed")]
    public string? DemoEmbed { get; set; }

    /// <summary>
    /// External appearances of the project (itch.io, portfolio sites, etc.).
    /// Each entry has a <c>Label</c> (display name) and a <c>Url</c>.
    /// </summary>
    [JsonPropertyName("links")]
    public List<LinkItem>? Links { get; set; }

    /// <summary>
    /// Third-party assets or libraries credited on the detail page.
    /// </summary>
    [JsonPropertyName("credits")]
    public List<CreditItem>? Credits { get; set; }

    /// <summary>
    /// Downloadable files (binaries, source archives, etc.).
    /// Each entry has a <c>Src</c> path and a descriptive <c>Label</c>.
    /// </summary>
    [JsonPropertyName("files")]
    public List<FileItem>? Files { get; set; }

    /// <summary>
    /// Bare filename stems of sub-pages for this project.
    /// Absent (null) when the project has no sub-pages.
    /// </summary>
    [JsonPropertyName("subpages")]
    public List<string>? Subpages { get; set; }
}
