using System.Text.Json;
using System.Text.RegularExpressions;
using Blazor_Personal_Site.Models;
using Markdig;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor_Personal_Site.Services;

/// <summary>
/// Processes and caches content sourced from the Programming CMS (<c>PS-CMS-Programming</c>).
///
/// This service owns all deserialization, markdown processing, and in-memory caching.
/// It is intentionally agnostic about <em>where</em> data comes from — that is the
/// responsibility of the injected <see cref="ICmsDataSource"/> (GitHub or local filesystem).
/// </summary>
public class ProgrammingService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private static readonly MarkdownPipeline MarkdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private readonly ICmsDataSource _dataSource;
    private readonly ILogger<ProgrammingService> _logger;

    // ── Manifest cache ────────────────────────────────────────────────────────

    private ProjectManifest? _manifestCache;
    private DateTime _manifestCachedAt = DateTime.MinValue;
    private static readonly TimeSpan ManifestTtl = TimeSpan.FromMinutes(5);

    // ── Per-project / per-subpage HTML cache ──────────────────────────────────
    //
    // Key: slug  or  "slug/subpage"
    // Version token: bump when the HTML post-processing pipeline changes so that
    // stale cached HTML is never served.

    private const int HtmlCacheVersion = 1;
    private readonly Dictionary<string, (string Html, DateTime CachedAt, int Version)>
        _htmlCache = new();
    private static readonly TimeSpan HtmlTtl = TimeSpan.FromMinutes(10);

    // ── Sub-page title cache ──────────────────────────────────────────────────

    private readonly Dictionary<string, (string Title, DateTime CachedAt)>
        _subpageTitleCache = new();

    public ProgrammingService(
        [FromKeyedServices("programming")] ICmsDataSource dataSource,
        ILogger<ProgrammingService> logger)
    {
        _dataSource = dataSource;
        _logger     = logger;
    }

    // ── URL helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves a relative asset path (as stored in the manifest or markdown) to
    /// a usable URL. Delegates to the active <see cref="ICmsDataSource"/>.
    /// </summary>
    public string ResolveCdnUrl(string slug, string relativePath) =>
        _dataSource.ResolveAssetUrl(slug, relativePath);

    // ── Public fetch methods ──────────────────────────────────────────────────

    /// <summary>
    /// Fetches (and caches) the manifest. Returns null if the fetch or
    /// deserialisation fails.
    /// </summary>
    public async Task<ProjectManifest?> FetchManifestAsync()
    {
        if (_manifestCache is not null && DateTime.UtcNow - _manifestCachedAt < ManifestTtl)
            return _manifestCache;

        var json = await _dataSource.FetchManifestJsonAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("Programming manifest was empty or unavailable.");
            return null;
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<ProjectManifest>(json, JsonOptions);
            if (manifest is null)
            {
                _logger.LogWarning("Programming manifest deserialised as null.");
                return null;
            }

            _manifestCache    = manifest;
            _manifestCachedAt = DateTime.UtcNow;
            return _manifestCache;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialise programming manifest.");
            return null;
        }
    }

    /// <summary>
    /// Fetches, processes, and caches the rendered HTML for a project's main
    /// markdown file. Front matter is stripped, relative image paths are
    /// rewritten to resolved URLs, and the markdown is converted to safe HTML.
    /// Returns null if the fetch fails.
    /// </summary>
    public async Task<string?> FetchProjectHtmlAsync(string slug)
    {
        if (_htmlCache.TryGetValue(slug, out var cached) &&
            cached.Version == HtmlCacheVersion &&
            DateTime.UtcNow - cached.CachedAt < HtmlTtl)
            return cached.Html;

        var raw = await _dataSource.FetchMarkdownAsync(slug, $"{slug}.md");
        if (raw is null) return null;

        var html = ProcessMarkdown(raw, slug, rewriteLinks: true);
        _htmlCache[slug] = (html, DateTime.UtcNow, HtmlCacheVersion);
        return html;
    }

    /// <summary>
    /// Fetches, processes, and caches the rendered HTML for a sub-page.
    /// Returns null if the fetch fails.
    /// </summary>
    public async Task<string?> FetchSubPageHtmlAsync(string slug, string subpage)
    {
        var cacheKey = $"{slug}/{subpage}";

        if (_htmlCache.TryGetValue(cacheKey, out var cached) &&
            cached.Version == HtmlCacheVersion &&
            DateTime.UtcNow - cached.CachedAt < HtmlTtl)
            return cached.Html;

        var raw = await _dataSource.FetchMarkdownAsync(slug, $"{subpage}.md");
        if (raw is null) return null;

        var html = ProcessMarkdown(raw, slug, rewriteLinks: false);
        _htmlCache[cacheKey] = (html, DateTime.UtcNow, HtmlCacheVersion);
        return html;
    }

    /// <summary>
    /// Fetches and caches the <c>title</c> field from a sub-page's YAML front matter.
    /// Returns null if the fetch fails or no title is found.
    /// </summary>
    public async Task<string?> FetchSubPageTitleAsync(string slug, string subpage)
    {
        var cacheKey = $"{slug}/{subpage}";

        if (_subpageTitleCache.TryGetValue(cacheKey, out var cached) &&
            DateTime.UtcNow - cached.CachedAt < HtmlTtl)
            return cached.Title;

        var raw = await _dataSource.FetchMarkdownAsync(slug, $"{subpage}.md");
        if (raw is null) return null;

        var title = ExtractFrontMatterTitle(raw) ?? TitleCaseFromSlug(subpage);
        _subpageTitleCache[cacheKey] = (title, DateTime.UtcNow);
        return title;
    }

    // ── Markdown processing pipeline ──────────────────────────────────────────

    private string ProcessMarkdown(string raw, string slug, bool rewriteLinks)
    {
        var body = StripFrontMatter(raw);
        body = RewriteRelativeImages(body, slug);
        var html = Markdown.ToHtml(body, MarkdownPipeline);
        html = WrapImages(html);
        if (rewriteLinks)
            html = RewriteRelativeLinks(html, slug);
        return html;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string StripFrontMatter(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!trimmed.StartsWith("---")) return markdown;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0) return markdown;

        var closingIndex = trimmed.IndexOf("\n---", firstNewline);
        if (closingIndex < 0) return markdown;

        var bodyStart = closingIndex + 4;
        if (bodyStart < trimmed.Length && trimmed[bodyStart] == '\r') bodyStart++;
        if (bodyStart < trimmed.Length && trimmed[bodyStart] == '\n') bodyStart++;

        return trimmed[bodyStart..];
    }

    private static string WrapImages(string html)
    {
        const string ZoomSvg =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" " +
            "viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
            "stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
            "<circle cx=\"11\" cy=\"11\" r=\"8\"/>" +
            "<line x1=\"21\" y1=\"21\" x2=\"16.65\" y2=\"16.65\"/>" +
            "<line x1=\"11\" y1=\"8\" x2=\"11\" y2=\"14\"/>" +
            "<line x1=\"8\" y1=\"11\" x2=\"14\" y2=\"11\"/>" +
            "</svg>";

        return Regex.Replace(
            html,
            @"<img([^>]*?)(?:\s*/?)?>",
            $"<span class=\"project-img-wrap\"><img$1><span class=\"project-img-hint\" aria-hidden=\"true\">{ZoomSvg}</span></span>");
    }

    private string RewriteRelativeImages(string markdown, string slug)
    {
        return Regex.Replace(
            markdown,
            @"!\[([^\]]*)\]\((?!https?://|//)([^)]+)\)",
            m =>
            {
                var alt  = m.Groups[1].Value;
                var path = m.Groups[2].Value.TrimStart('.', '/');
                var url  = _dataSource.ResolveAssetUrl(slug, path);
                return $"![{alt}]({url})";
            });
    }

    /// <summary>
    /// Rewrites bare relative <c>href</c> values in rendered HTML to absolute
    /// Blazor routes (<c>/programming/{slug}/{stem}</c>).
    /// </summary>
    private static string RewriteRelativeLinks(string html, string slug)
    {
        return Regex.Replace(
            html,
            @"href=""(?!https?://|//|[/#])([^""]+)""",
            m => $"href=\"/programming/{Uri.EscapeDataString(slug)}/{m.Groups[1].Value}\"");
    }

    private static string? ExtractFrontMatterTitle(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!trimmed.StartsWith("---")) return null;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0) return null;

        var closingIndex = trimmed.IndexOf("\n---", firstNewline);
        if (closingIndex < 0) return null;

        var frontMatter = trimmed[..closingIndex];
        var match = Regex.Match(
            frontMatter,
            @"^title:\s*""?([^""\r\n]+)""?\s*$",
            RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string TitleCaseFromSlug(string slug) =>
        string.Join(" ", slug.Split('-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));
}
