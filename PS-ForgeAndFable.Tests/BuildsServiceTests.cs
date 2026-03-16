using ForgeAndFable.Services;
using ForgeAndFable.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForgeAndFable.Tests;

public class BuildsServiceTests
{
    private readonly Mock<ICmsDataSource> _dataSource = new();
    private readonly ILogger<BuildsService> _logger   = Mock.Of<ILogger<BuildsService>>();

    private BuildsService CreateSut() => new(_dataSource.Object, _logger);

    // ── FetchManifestAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task FetchManifestAsync_ValidJson_ReturnsDeserializedManifest()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""{"builds":[{"slug":"test-build","title":"Test Build"}]}""");

        var result = await CreateSut().FetchManifestAsync();

        Assert.NotNull(result);
        Assert.Single(result.Builds);
        Assert.Equal("test-build", result.Builds[0].Slug);
        Assert.Equal("Test Build",  result.Builds[0].Title);
    }

    [Fact]
    public async Task FetchManifestAsync_EmptyJson_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync("");

        var result = await CreateSut().FetchManifestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchManifestAsync_WhitespaceJson_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync("   ");

        var result = await CreateSut().FetchManifestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchManifestAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync((string?)null);

        var result = await CreateSut().FetchManifestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchManifestAsync_InvalidJson_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync("not valid json {{{{");

        var result = await CreateSut().FetchManifestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchManifestAsync_EmptyBuildsArray_ReturnsManifestWithNoBuilds()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""{"builds":[]}""");

        var result = await CreateSut().FetchManifestAsync();

        Assert.NotNull(result);
        Assert.Empty(result.Builds);
    }

    [Fact]
    public async Task FetchManifestAsync_CalledTwice_OnlyFetchesFromDataSourceOnce()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""{"builds":[]}""");
        var sut = CreateSut();

        await sut.FetchManifestAsync();
        await sut.FetchManifestAsync();

        _dataSource.Verify(source => source.FetchManifestJsonAsync(), Times.Once);
    }

    // ── FetchBuildHtmlAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task FetchBuildHtmlAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("my-build", "my-build.md"))
                   .ReturnsAsync((string?)null);
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchBuildHtmlAsync("my-build");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchBuildHtmlAsync_ValidMarkdown_ReturnsHtmlString()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("my-build", "my-build.md"))
                   .ReturnsAsync("# My Build\n\nSome content here.");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchBuildHtmlAsync("my-build");

        Assert.NotNull(result);
        Assert.Contains(">My Build</h1>", result);
        Assert.Contains("Some content here", result);
    }

    [Fact]
    public async Task FetchBuildHtmlAsync_MarkdownWithFrontMatter_FrontMatterIsStripped()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("---\ntitle: \"My Build\"\n---\n# Heading");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchBuildHtmlAsync("slug");

        Assert.NotNull(result);
        Assert.DoesNotContain("title:", result);
        Assert.Contains(">Heading</h1>", result);
    }

    [Fact]
    public async Task FetchBuildHtmlAsync_CalledTwiceWithSameSlug_FetchesMarkdownOnce()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("# Content");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");
        var sut = CreateSut();

        await sut.FetchBuildHtmlAsync("slug");
        await sut.FetchBuildHtmlAsync("slug");

        _dataSource.Verify(source => source.FetchMarkdownAsync("slug", "slug.md"), Times.Once);
    }

    [Fact]
    public async Task FetchBuildHtmlAsync_UsesBuildsImgCssPrefix()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("![alt](https://example.com/img.jpg)");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchBuildHtmlAsync("slug");

        Assert.NotNull(result);
        Assert.Contains("class=\"build-img-wrap\"", result);
    }

    // ── FetchSubPageHtmlAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task FetchSubPageHtmlAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "details.md"))
                   .ReturnsAsync((string?)null);
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "details");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSubPageHtmlAsync_ValidMarkdown_ReturnsHtml()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "details.md"))
                   .ReturnsAsync("## Details\n\nContent here.");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "details");

        Assert.NotNull(result);
        Assert.Contains(">Details</h2>", result);
    }

    [Fact]
    public async Task FetchSubPageHtmlAsync_DoesNotRewriteRelativeLinks()
    {
        // Sub-pages use rewriteLinks: false, so relative hrefs should remain as-is.
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "subpage.md"))
                   .ReturnsAsync("[link text](other-page)");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "subpage");

        Assert.NotNull(result);
        Assert.Contains("href=\"other-page\"", result);
        Assert.DoesNotContain("/builds/", result);
    }

    // ── FetchSubPageTitleAsync ────────────────────────────────────────────────

    [Fact]
    public async Task FetchSubPageTitleAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "subpage.md"))
                   .ReturnsAsync((string?)null);

        var result = await CreateSut().FetchSubPageTitleAsync("slug", "subpage");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSubPageTitleAsync_FrontMatterHasTitle_ReturnsThatTitle()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "print-settings.md"))
                   .ReturnsAsync("---\ntitle: \"Print Settings\"\n---\nContent.");

        var result = await CreateSut().FetchSubPageTitleAsync("slug", "print-settings");

        Assert.Equal("Print Settings", result);
    }

    [Fact]
    public async Task FetchSubPageTitleAsync_NoFrontMatterTitle_ReturnsTitleCasedSlug()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "print-settings.md"))
                   .ReturnsAsync("# Just a heading\n\nContent.");

        var result = await CreateSut().FetchSubPageTitleAsync("slug", "print-settings");

        Assert.Equal("Print Settings", result);
    }

    [Fact]
    public async Task FetchSubPageTitleAsync_CalledTwice_FetchesMarkdownOnce()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "subpage.md"))
                   .ReturnsAsync("---\ntitle: \"Title\"\n---\nContent.");
        var sut = CreateSut();

        await sut.FetchSubPageTitleAsync("slug", "subpage");
        await sut.FetchSubPageTitleAsync("slug", "subpage");

        _dataSource.Verify(source => source.FetchMarkdownAsync("slug", "subpage.md"), Times.Once);
    }

    // ── ResolveCdnUrl ─────────────────────────────────────────────────────────

    [Fact]
    public void ResolveCdnUrl_DelegatesToDataSource()
    {
        _dataSource.Setup(source => source.ResolveAssetUrl("slug", "image.jpg"))
                   .Returns("https://cdn.example.com/slug/image.jpg");

        var result = CreateSut().ResolveCdnUrl("slug", "image.jpg");

        Assert.Equal("https://cdn.example.com/slug/image.jpg", result);
    }
}
