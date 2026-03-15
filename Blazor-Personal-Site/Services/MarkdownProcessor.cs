using System.Text.RegularExpressions;
using Markdig;

namespace Blazor_Personal_Site.Services;

/// <summary>
/// Shared static helpers for the CMS markdown processing pipeline.
///
/// Both <see cref="BuildsService"/> and <see cref="ProgrammingService"/> run the
/// same pipeline (strip front matter → rewrite images → parse markdown → wrap images
/// → optionally rewrite links). Centralising the logic here means pipeline changes
/// only need to be made in one place.
/// </summary>
public static partial class MarkdownProcessor
{
    /// <summary>
    /// A single shared pipeline instance used by all CMS services.
    /// MarkdownPipeline is thread-safe once built.
    /// </summary>
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    // ── Source-generated compiled regexes ─────────────────────────────────────
    //
    // [GeneratedRegex] emits a compile-time Roslyn-generated state machine —
    // faster than RegexOptions.Compiled and zero runtime JIT cost.

    [GeneratedRegex(@"<img([^>]*?)(?:\s*/?)?>")]
    private static partial Regex ImgTagRegex();

    [GeneratedRegex(@"!\[([^\]]*)\]\((?!https?://|//)([^)]+)\)")]
    private static partial Regex RelativeImageRegex();

    [GeneratedRegex(@"href=""(?!https?://|//|[/#])([^""]+)""")]
    private static partial Regex RelativeLinkRegex();

    [GeneratedRegex(@"^title:\s*""?([^""\r\n]+)""?\s*$", RegexOptions.Multiline)]
    private static partial Regex FrontMatterTitleRegex();

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>
    /// Runs the full CMS markdown pipeline:
    ///   1. Strip YAML front matter.
    ///   2. Rewrite relative image paths to resolved asset URLs.
    ///   3. Convert markdown to HTML via Markdig.
    ///   4. Wrap every &lt;img&gt; in a container span for layout/lightbox.
    ///   5. Optionally rewrite relative href values to absolute Blazor routes.
    /// </summary>
    /// <param name="raw">Raw markdown string (may include YAML front matter).</param>
    /// <param name="slug">Entry slug — used for URL resolution and route rewriting.</param>
    /// <param name="rewriteLinks">
    ///     When <c>true</c>, bare relative href values in the rendered HTML are rewritten
    ///     to <c>/{baseRoute}/{slug}/{stem}</c> so sub-page links work inside Blazor routing.
    /// </param>
    /// <param name="resolveAssetUrl">
    ///     Delegate that maps a (slug, relativePath) pair to a usable URL.
    ///     Provided by the active <see cref="Abstractions.ICmsDataSource"/>.
    /// </param>
    /// <param name="imgCssPrefix">
    ///     CSS class prefix for image wrappers, e.g. <c>"build-img"</c> or
    ///     <c>"project-img"</c>. The wrapping span gets <c>{prefix}-wrap</c>
    ///     and the zoom-hint span gets <c>{prefix}-hint</c>.
    /// </param>
    /// <param name="baseRoute">
    ///     The Blazor route segment used when rewriting links (e.g. <c>"builds"</c>
    ///     or <c>"programming"</c>).
    /// </param>
    public static string Process(
        string raw,
        string slug,
        bool rewriteLinks,
        Func<string, string, string> resolveAssetUrl,
        string imgCssPrefix,
        string baseRoute)
    {
        var body = StripFrontMatter(raw);
        body = RewriteRelativeImages(body, slug, resolveAssetUrl);
        var html = Markdown.ToHtml(body, Pipeline);
        html = WrapImages(html, imgCssPrefix);
        if (rewriteLinks)
            html = RewriteRelativeLinks(html, slug, baseRoute);
        return html;
    }

    // ── Front matter ──────────────────────────────────────────────────────────

    // Strips the YAML front matter block (--- … ---) from the top of a markdown string.
    private static string StripFrontMatter(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!TryFindFrontMatterBounds(trimmed, out _, out var bodyStart))
            return markdown;

        return trimmed[bodyStart..];
    }

    /// <summary>
    /// Extracts the <c>title</c> field from a YAML front matter block.
    /// Returns <c>null</c> if the document has no front matter or no title field.
    /// </summary>
    public static string? ExtractFrontMatterTitle(string markdown)
    {
        var trimmed = markdown.TrimStart();
        if (!TryFindFrontMatterBounds(trimmed, out var closingIndex, out _))
            return null;

        var frontMatter = trimmed[..closingIndex];
        var match = FrontMatterTitleRegex().Match(frontMatter);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    // ── Slug helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a kebab-case slug to a human-readable title-cased string.
    /// Used as a fallback when no front matter title is available.
    /// </summary>
    public static string TitleCaseFromSlug(string slug) =>
        string.Join(" ", slug.Split('-')
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w));

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Locates the front matter block boundaries in an already-trimmed markdown string.
    /// Returns <c>false</c> when no valid <c>--- … ---</c> block is present.
    /// </summary>
    /// <param name="trimmed">Markdown with leading whitespace already removed.</param>
    /// <param name="closingIndex">Index of the closing <c>\n---</c> sequence.</param>
    /// <param name="bodyStart">Index of the first character after the closing delimiter.</param>
    private static bool TryFindFrontMatterBounds(
        string trimmed, out int closingIndex, out int bodyStart)
    {
        closingIndex = -1;
        bodyStart    = 0;

        if (!trimmed.StartsWith("---")) return false;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0) return false;

        closingIndex = trimmed.IndexOf("\n---", firstNewline);
        if (closingIndex < 0) return false;

        bodyStart = closingIndex + 4; // length of "\n---"
        if (bodyStart < trimmed.Length && trimmed[bodyStart] == '\r') bodyStart++;
        if (bodyStart < trimmed.Length && trimmed[bodyStart] == '\n') bodyStart++;

        return true;
    }

    /// <summary>
    /// Wraps every &lt;img&gt; in the rendered HTML with a container span for
    /// layout purposes (max-height, centering, shadow) and injects a zoom-hint SVG
    /// icon (hidden via CSS; lightbox behaviour is handled by dedicated components).
    /// The wrapping span gets class <c>{imgCssPrefix}-wrap</c> and the hint span
    /// gets <c>{imgCssPrefix}-hint</c>.
    /// </summary>
    private static string WrapImages(string html, string imgCssPrefix)
    {
        var wrapClass = $"{imgCssPrefix}-wrap";
        var hintClass = $"{imgCssPrefix}-hint";

        const string ZoomSvg =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" " +
            "viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" " +
            "stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
            "<circle cx=\"11\" cy=\"11\" r=\"8\"/>" +
            "<line x1=\"21\" y1=\"21\" x2=\"16.65\" y2=\"16.65\"/>" +
            "<line x1=\"11\" y1=\"8\" x2=\"11\" y2=\"14\"/>" +
            "<line x1=\"8\" y1=\"11\" x2=\"14\" y2=\"11\"/>" +
            "</svg>";

        return ImgTagRegex().Replace(
            html,
            $"<span class=\"{wrapClass}\"><img$1><span class=\"{hintClass}\" aria-hidden=\"true\">{ZoomSvg}</span></span>");
    }

    /// <summary>
    /// Rewrites relative image paths in markdown syntax (![alt](relative))
    /// to resolved asset URLs before the markdown is parsed.
    /// </summary>
    private static string RewriteRelativeImages(
        string markdown,
        string slug,
        Func<string, string, string> resolveAssetUrl)
    {
        return RelativeImageRegex().Replace(
            markdown,
            m =>
            {
                var alt  = m.Groups[1].Value;
                var path = m.Groups[2].Value.TrimStart('.', '/');
                var url  = resolveAssetUrl(slug, path);
                return $"![{alt}]({url})";
            });
    }

    /// <summary>
    /// Rewrites bare relative <c>href</c> values in rendered HTML to absolute
    /// Blazor routes (<c>/{baseRoute}/{slug}/{stem}</c>).
    /// </summary>
    private static string RewriteRelativeLinks(string html, string slug, string baseRoute)
    {
        return RelativeLinkRegex().Replace(
            html,
            m => $"href=\"/{baseRoute}/{Uri.EscapeDataString(slug)}/{m.Groups[1].Value}\"");
    }
}
