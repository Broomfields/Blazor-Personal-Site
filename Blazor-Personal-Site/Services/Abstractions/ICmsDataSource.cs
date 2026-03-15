namespace Blazor_Personal_Site.Services.Abstractions;

/// <summary>
/// Abstracts the transport layer for CMS data — how the raw manifest JSON and
/// markdown content are fetched, and how asset URLs are constructed.
///
/// Two implementations exist:
///   <see cref="DataSources.GitHubCmsDataSource"/> — production, fetches from GitHub.
///   <see cref="DataSources.LocalCmsDataSource"/>  — development, reads from a local repo clone.
///
/// The active implementation for each CMS is selected at startup via the
/// "Cms:Source" configuration key ("GitHub" | "Local").
/// </summary>
public interface ICmsDataSource
{
    /// <summary>
    /// Fetches the raw manifest JSON string.
    /// Returns <c>null</c> if the fetch fails for any reason.
    /// </summary>
    Task<string?> FetchManifestJsonAsync();

    /// <summary>
    /// Fetches the raw markdown content for a file inside an entry's directory.
    /// <paramref name="filename"/> is the bare filename including extension
    /// (e.g. <c>"rocket-booster-bracket.md"</c> or <c>"print-settings.md"</c>).
    /// Returns <c>null</c> if the fetch fails.
    /// </summary>
    Task<string?> FetchMarkdownAsync(string slug, string filename);

    /// <summary>
    /// Resolves a relative asset path (as stored in the manifest or markdown)
    /// to a usable URL. May be absolute (GitHub CDN) or root-relative (/dev-cdn/).
    /// </summary>
    string ResolveAssetUrl(string slug, string relativePath);
}
