using Blazor_Personal_Site.Services.Abstractions;

namespace Blazor_Personal_Site.Services.DataSources;

/// <summary>
/// Development data source: reads the manifest and markdown files directly
/// from a local clone of a CMS repository.
///
/// Asset URLs are returned as root-relative paths under <c>/dev-cdn/{contentFolder}/</c>.
/// Program.cs mounts a PhysicalFileProvider at the content folder inside the repo,
/// so the browser can load images through the running dev server rather than
/// needing file:// access.
///
/// Activate by setting "Cms:Source": "Local" and
/// "Cms:LocalPaths:{key}": "/path/to/repo" in appsettings.Development.json.
/// A leading <c>~/</c> in the path is expanded to the current user's home directory.
/// </summary>
public class LocalCmsDataSource : ICmsDataSource
{
    /// <summary>
    /// The root request-path prefix under which local CMS assets are served.
    /// Each CMS gets its own sub-path: <c>/dev-cdn/{contentFolder}/</c>.
    /// Must match the RequestPath pattern configured in Program.cs.
    /// </summary>
    public const string DevCdnPrefix = "/dev-cdn";

    private readonly string _repoRoot;
    private readonly string _contentFolder;
    private readonly ILogger<LocalCmsDataSource> _logger;

    /// <param name="repoRoot">
    ///     Path to the local repo clone. A leading <c>~/</c> is expanded
    ///     to the current user's home directory.
    /// </param>
    /// <param name="contentFolder">
    ///     The subfolder inside the repo that contains per-slug entry directories,
    ///     e.g. <c>"builds"</c> or <c>"programming"</c>.
    /// </param>
    public LocalCmsDataSource(
        string repoRoot,
        string contentFolder,
        ILogger<LocalCmsDataSource> logger)
    {
        // Expand ~/ so appsettings can use the conventional home-directory shorthand.
        if (repoRoot.StartsWith("~/") || repoRoot == "~")
        {
            repoRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                repoRoot.Length > 2 ? repoRoot[2..] : string.Empty);
        }

        _repoRoot      = repoRoot;
        _contentFolder = contentFolder;
        _logger        = logger;
    }

    /// <summary>The expanded absolute path to the local repo root.</summary>
    public string RepoRoot => _repoRoot;

    /// <summary>
    /// The subfolder inside the repo containing per-slug directories.
    /// Also used as the sub-path under <see cref="DevCdnPrefix"/> when serving assets.
    /// </summary>
    public string ContentFolder => _contentFolder;

    /// <inheritdoc/>
    public async Task<string?> FetchManifestJsonAsync()
    {
        var path = Path.Combine(_repoRoot, "manifest.json");
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read local manifest from '{Path}'.", path);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> FetchMarkdownAsync(string slug, string filename)
    {
        var path = Path.Combine(_repoRoot, _contentFolder, slug, filename);
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Failed to read local markdown from '{Path}'.", path);
            return null;
        }
    }

    /// <inheritdoc/>
    public string ResolveAssetUrl(string slug, string relativePath)
    {
        relativePath = relativePath.TrimStart('.', '/');
        // Served by the PhysicalFileProvider middleware mounted at
        // /dev-cdn/{contentFolder}/ in Program.cs.
        return $"{DevCdnPrefix}/{_contentFolder}/{slug}/{relativePath}";
    }
}
