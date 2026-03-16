using ForgeAndFable.Models;

namespace ForgeAndFable.Services.Abstractions;

/// <summary>
/// Abstraction over <see cref="ProgrammingService"/> consumed by Razor components.
///
/// Decouples component code from the concrete service implementation, making
/// it straightforward to substitute a test double in unit/component tests.
/// </summary>
public interface IProgrammingService
{
    /// <summary>
    /// Resolves a relative asset path to a usable URL via the active data source.
    /// </summary>
    string ResolveCdnUrl(string slug, string relativePath);

    /// <summary>
    /// Fetches (and caches) the project manifest. Returns null on failure.
    /// </summary>
    Task<ProjectManifest?> FetchManifestAsync();

    /// <summary>
    /// Fetches, processes, and caches the rendered HTML for a project's main page.
    /// Returns null on failure.
    /// </summary>
    Task<string?> FetchProjectHtmlAsync(string slug);

    /// <summary>
    /// Fetches, processes, and caches the rendered HTML for a sub-page.
    /// Returns null on failure.
    /// </summary>
    Task<string?> FetchSubPageHtmlAsync(string slug, string subpage);

    /// <summary>
    /// Fetches and caches the title from a sub-page's YAML front matter.
    /// Returns null if the fetch fails or no title is present.
    /// </summary>
    Task<string?> FetchSubPageTitleAsync(string slug, string subpage);
}
