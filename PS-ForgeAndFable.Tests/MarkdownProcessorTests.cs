using ForgeAndFable.Services;

namespace ForgeAndFable.Tests;

public class MarkdownProcessorTests
{
    // ── TitleCaseFromSlug ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("my-project",          "My Project")]
    [InlineData("rocket-booster-v2",   "Rocket Booster V2")]
    [InlineData("hello",               "Hello")]
    [InlineData("a-b-c",               "A B C")]
    public void TitleCaseFromSlug_ConvertsKebabCaseToTitleCase(string slug, string expected)
    {
        var result = MarkdownProcessor.TitleCaseFromSlug(slug);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TitleCaseFromSlug_EmptyString_ReturnsEmpty()
    {
        var result = MarkdownProcessor.TitleCaseFromSlug("");

        Assert.Equal("", result);
    }

    // ── ExtractFrontMatterTitle ───────────────────────────────────────────────

    [Fact]
    public void ExtractFrontMatterTitle_QuotedTitle_ReturnsTitle()
    {
        var markdown = """
            ---
            title: "My Build Title"
            date: 2024-01-01
            ---
            # Content
            """;

        var result = MarkdownProcessor.ExtractFrontMatterTitle(markdown);

        Assert.Equal("My Build Title", result);
    }

    [Fact]
    public void ExtractFrontMatterTitle_UnquotedTitle_ReturnsTitle()
    {
        var markdown = """
            ---
            title: Unquoted Title Here
            ---
            Body text.
            """;

        var result = MarkdownProcessor.ExtractFrontMatterTitle(markdown);

        Assert.Equal("Unquoted Title Here", result);
    }

    [Fact]
    public void ExtractFrontMatterTitle_NoFrontMatter_ReturnsNull()
    {
        var markdown = "# Just a heading\n\nSome content.";

        var result = MarkdownProcessor.ExtractFrontMatterTitle(markdown);

        Assert.Null(result);
    }

    [Fact]
    public void ExtractFrontMatterTitle_FrontMatterWithoutTitleField_ReturnsNull()
    {
        var markdown = """
            ---
            date: 2024-01-01
            tags: ["3d-printing"]
            ---
            Body.
            """;

        var result = MarkdownProcessor.ExtractFrontMatterTitle(markdown);

        Assert.Null(result);
    }

    [Fact]
    public void ExtractFrontMatterTitle_EmptyString_ReturnsNull()
    {
        var result = MarkdownProcessor.ExtractFrontMatterTitle("");

        Assert.Null(result);
    }

    [Fact]
    public void ExtractFrontMatterTitle_LeadingWhitespace_StillFindsTitle()
    {
        var markdown = "\n\n---\ntitle: \"Padded Title\"\n---\nBody.";

        var result = MarkdownProcessor.ExtractFrontMatterTitle(markdown);

        Assert.Equal("Padded Title", result);
    }

    // ── Process — front matter stripping ─────────────────────────────────────

    [Fact]
    public void Process_StripsYamlFrontMatter_FromOutput()
    {
        var raw = """
            ---
            title: "Test"
            ---
            Hello world.
            """;

        var result = Process(raw, "my-slug");

        Assert.DoesNotContain("title:", result);
        Assert.DoesNotContain("---\n", result);
        Assert.Contains("Hello world", result);
    }

    [Fact]
    public void Process_NoFrontMatter_RendersContentAsIs()
    {
        var raw = "Hello world.";

        var result = Process(raw, "slug");

        Assert.Contains("Hello world", result);
    }

    // ── Process — markdown → HTML conversion ─────────────────────────────────

    [Fact]
    public void Process_HeadingMarkdown_RendersH1Tag()
    {
        var result = Process("# My Heading", "slug");

        // UseAdvancedExtensions adds auto-id attributes: <h1 id="my-heading">
        Assert.Contains(">My Heading</h1>", result);
        Assert.Contains("<h1", result);
    }

    [Fact]
    public void Process_BoldMarkdown_RendersStrongTag()
    {
        var result = Process("**bold text**", "slug");

        Assert.Contains("<strong>bold text</strong>", result);
    }

    [Fact]
    public void Process_LinkMarkdown_RendersAnchorTag()
    {
        var result = Process("[click here](https://example.com)", "slug");

        Assert.Contains("<a href=\"https://example.com\"", result);
        Assert.Contains("click here", result);
    }

    // ── Process — image wrapping ──────────────────────────────────────────────

    [Fact]
    public void Process_InlineImage_WrapsInSpanWithCorrectCssPrefix()
    {
        var raw = "![alt text](https://example.com/img.jpg)";

        var result = Process(raw, "slug", imgCssPrefix: "build-img");

        Assert.Contains("class=\"build-img-wrap\"", result);
        Assert.Contains("class=\"build-img-hint\"", result);
        Assert.Contains("<img", result);
    }

    [Fact]
    public void Process_InlineImage_WrapSpanContainsZoomSvg()
    {
        var raw = "![alt](https://example.com/img.png)";

        var result = Process(raw, "slug");

        Assert.Contains("<svg", result);
        Assert.Contains("<circle", result);
    }

    [Fact]
    public void Process_MultipleImages_EachWrappedSeparately()
    {
        var raw = "![a](https://example.com/a.jpg)\n\n![b](https://example.com/b.jpg)";

        var result = Process(raw, "slug", imgCssPrefix: "test-img");

        Assert.Equal(2, CountOccurrences(result, "class=\"test-img-wrap\""));
    }

    // ── Process — relative image rewriting ────────────────────────────────────

    [Fact]
    public void Process_RelativeImage_RewritesToResolvedUrl()
    {
        var raw = "![alt](./photo.jpg)";

        var result = Process(raw, "my-build", resolveAssetUrl: (entrySlug, assetPath) => $"https://cdn.example.com/{entrySlug}/{assetPath}");

        Assert.Contains("https://cdn.example.com/my-build/photo.jpg", result);
        Assert.DoesNotContain("./photo.jpg", result);
    }

    [Fact]
    public void Process_AbsoluteImageUrl_IsNotRewritten()
    {
        var raw = "![alt](https://external.com/img.jpg)";
        var wasResolved = false;

        Process(raw, "slug", resolveAssetUrl: (_, _) => { wasResolved = true; return "https://replaced.com/img.jpg"; });

        Assert.False(wasResolved, "Absolute URLs should not be passed through resolveAssetUrl.");
    }

    // ── Process — relative link rewriting ─────────────────────────────────────

    [Fact]
    public void Process_RewriteLinksTrue_RewritesRelativeHref()
    {
        var raw = "[See page](print-settings)";

        var result = Process(raw, "my-build", rewriteLinks: true, baseRoute: "builds");

        Assert.Contains("href=\"/builds/my-build/print-settings\"", result);
    }

    [Fact]
    public void Process_RewriteLinksFalse_LeavesRelativeHrefUnchanged()
    {
        var raw = "[See page](print-settings)";

        var result = Process(raw, "my-build", rewriteLinks: false, baseRoute: "builds");

        Assert.Contains("href=\"print-settings\"", result);
        Assert.DoesNotContain("/builds/", result);
    }

    [Fact]
    public void Process_RewriteLinks_DoesNotTouchAbsoluteHrefs()
    {
        var raw = "[External](https://example.com/page)";

        var result = Process(raw, "slug", rewriteLinks: true, baseRoute: "builds");

        Assert.Contains("href=\"https://example.com/page\"", result);
    }

    [Fact]
    public void Process_RewriteLinks_SlugIsUrlEncoded()
    {
        var raw = "[page](subpage)";

        var result = Process(raw, "slug with spaces", rewriteLinks: true, baseRoute: "builds");

        Assert.Contains("slug%20with%20spaces", result);
    }

    // ── Process — mermaid block rewriting ────────────────────────────────────

    [Fact]
    public void Process_MermaidFence_OutputsWrappingPre()
    {
        var raw = """
            ```mermaid
            graph LR
                A --> B
            ```
            """;

        var result = Process(raw, "slug");

        Assert.Contains("<pre class=\"mermaid\">", result);
        Assert.Contains("graph LR", result);
    }

    [Fact]
    public void Process_MermaidFence_ContentIsNotHtmlEncoded()
    {
        // The --> arrow contains > which Markdig would encode as &gt; if it
        // processed the block as a code fence. Converting to a raw HTML pre
        // first ensures the content is passed through verbatim.
        var raw = """
            ```mermaid
            graph LR
                A -->|label| B
            ```
            """;

        var result = Process(raw, "slug");

        Assert.Contains("-->", result);
        Assert.DoesNotContain("&gt;", result);
    }

    [Fact]
    public void Process_MermaidFence_PreservesContentAcrossBlankLines()
    {
        // Blank lines inside a diagram must not truncate the output.
        // <pre> is a CommonMark Type 1 HTML block (ends only at </pre>),
        // so internal blank lines are safe. A <div> would be Type 6 and
        // would be terminated by the first blank line.
        var raw = """
            ```mermaid
            graph LR
                subgraph "Group"
                    A --> B
                end

                C --> D
            ```
            """;

        var result = Process(raw, "slug");

        Assert.Contains("C --> D", result);
        Assert.DoesNotContain("language-mermaid", result);
    }

    [Fact]
    public void Process_MermaidFence_DoesNotProduceCodeBlock()
    {
        var raw = """
            ```mermaid
            graph LR
                A --> B
            ```
            """;

        var result = Process(raw, "slug");

        Assert.DoesNotContain("language-mermaid", result);
    }

    [Fact]
    public void Process_NonMermaidFence_IsNotAffected()
    {
        var raw = """
            ```csharp
            var x = 1;
            ```
            """;

        var result = Process(raw, "slug");

        Assert.DoesNotContain("class=\"mermaid\"", result);
        Assert.Contains("var x = 1", result);
    }

    // ── Process — combined pipeline ───────────────────────────────────────────

    [Fact]
    public void Process_FullPipeline_FrontMatterStrippedAndImageWrappedAndLinkRewritten()
    {
        var raw = """
            ---
            title: "Full Test"
            ---
            # Introduction

            ![cover](./cover.jpg)

            [More details](details)
            """;

        var result = Process(
            raw, "my-slug",
            rewriteLinks:    true,
            resolveAssetUrl: (entrySlug, assetPath) => $"/cdn/{entrySlug}/{assetPath}",
            imgCssPrefix:    "build-img",
            baseRoute:       "builds");

        Assert.DoesNotContain("title: \"Full Test\"", result);
        Assert.Contains(">Introduction</h1>", result);
        Assert.Contains("/cdn/my-slug/cover.jpg", result);
        Assert.Contains("class=\"build-img-wrap\"", result);
        Assert.Contains("href=\"/builds/my-slug/details\"", result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Process(
        string raw,
        string slug,
        bool rewriteLinks = false,
        Func<string, string, string>? resolveAssetUrl = null,
        string imgCssPrefix = "test-img",
        string baseRoute = "test")
    {
        resolveAssetUrl ??= (_, assetPath) => $"https://cdn.example.com/{assetPath}";
        return MarkdownProcessor.Process(raw, slug, rewriteLinks, resolveAssetUrl, imgCssPrefix, baseRoute);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }
}
