using ForgeAndFable.Services.Abstractions;

namespace ForgeAndFable.Services.DataSources;

/// <summary>
/// Production data source: fetches manifest and markdown from a GitHub repository
/// over HTTP.
///
/// The repo owner, repo name, and content folder are supplied at construction time,
/// so the same class serves every CMS (Builds, Programming, Writing, …) — just
/// registered under different keyed-service keys in Program.cs.
///
/// This is the default implementation used when "Cms:Source" is "GitHub"
/// (or when the key is absent).
/// </summary>
public class GitHubCmsDataSource : ICmsDataSource
{
    private const string RepoOwner = "Broomfields";
    private const string Branch    = "main";

    private readonly string _rawBase;
    private readonly string _manifestUrl;
    private readonly string _contentFolder;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubCmsDataSource> _logger;

    /// <param name="repoName">
    ///     The GitHub repository name, e.g. <c>"PS-CMS-Builds"</c>.
    /// </param>
    /// <param name="contentFolder">
    ///     The subfolder inside the repo that contains per-slug entry directories,
    ///     e.g. <c>"builds"</c> or <c>"programming"</c>.
    /// </param>
    public GitHubCmsDataSource(
        string repoName,
        string contentFolder,
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubCmsDataSource> logger)
    {
        _contentFolder     = contentFolder;
        _httpClientFactory = httpClientFactory;
        _logger            = logger;

        _rawBase     = $"https://raw.githubusercontent.com/{RepoOwner}/{repoName}/{Branch}";
        _manifestUrl = $"https://github.com/{RepoOwner}/{repoName}/releases/latest/download/manifest.json";
    }

    /// <inheritdoc/>
    public async Task<string?> FetchManifestJsonAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var http = _httpClientFactory.CreateClient();
            var json = await http.GetStringAsync(_manifestUrl, cts.Token);
            return string.IsNullOrWhiteSpace(json) ? null : json;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch manifest from GitHub ({ManifestUrl}).", _manifestUrl);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> FetchMarkdownAsync(string slug, string filename)
    {
        var url = $"{_rawBase}/{_contentFolder}/{Uri.EscapeDataString(slug)}/{filename}";
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var http = _httpClientFactory.CreateClient();
            return await http.GetStringAsync(url, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Failed to fetch markdown '{Slug}/{Filename}' from GitHub.",
                slug, filename);
            return null;
        }
    }

    /// <inheritdoc/>
    public string ResolveAssetUrl(string slug, string relativePath)
    {
        relativePath = relativePath.TrimStart('.', '/');
        return $"{_rawBase}/{_contentFolder}/{Uri.EscapeDataString(slug)}/{relativePath}";
    }
}
