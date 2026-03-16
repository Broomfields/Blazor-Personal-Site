using ForgeAndFable.Services;
using ForgeAndFable.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForgeAndFable.Tests;

public class ProgrammingServiceTests
{
    private readonly Mock<ICmsDataSource> _dataSource = new();
    private readonly ILogger<ProgrammingService> _logger = Mock.Of<ILogger<ProgrammingService>>();

    private ProgrammingService CreateSut() => new(_dataSource.Object, _logger);

    // ── FetchManifestAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task FetchManifestAsync_ValidJson_ReturnsDeserializedManifest()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""{"projects":[{"slug":"my-project","title":"My Project"}]}""");

        var result = await CreateSut().FetchManifestAsync();

        Assert.NotNull(result);
        Assert.Single(result.Projects);
        Assert.Equal("my-project", result.Projects[0].Slug);
        Assert.Equal("My Project",  result.Projects[0].Title);
    }

    [Fact]
    public async Task FetchManifestAsync_EmptyJson_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync("");

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
        _dataSource.Setup(source => source.FetchManifestJsonAsync()).ReturnsAsync("{bad json}");

        var result = await CreateSut().FetchManifestAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchManifestAsync_CalledTwice_OnlyFetchesFromDataSourceOnce()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""{"projects":[]}""");
        var sut = CreateSut();

        await sut.FetchManifestAsync();
        await sut.FetchManifestAsync();

        _dataSource.Verify(source => source.FetchManifestJsonAsync(), Times.Once);
    }

    [Fact]
    public async Task FetchManifestAsync_ManifestWithMultipleProjects_DeserializesAll()
    {
        _dataSource.Setup(source => source.FetchManifestJsonAsync())
                   .ReturnsAsync("""
                       {
                         "projects": [
                           {"slug":"proj-a","title":"Project A"},
                           {"slug":"proj-b","title":"Project B"},
                           {"slug":"proj-c","title":"Project C"}
                         ]
                       }
                       """);

        var result = await CreateSut().FetchManifestAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Projects.Count);
        Assert.Equal("proj-c", result.Projects[2].Slug);
    }

    // ── FetchProjectHtmlAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task FetchProjectHtmlAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("my-project", "my-project.md"))
                   .ReturnsAsync((string?)null);
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchProjectHtmlAsync("my-project");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchProjectHtmlAsync_ValidMarkdown_ReturnsRenderedHtml()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("chess", "chess.md"))
                   .ReturnsAsync("# Chess Game\n\nA multiplayer chess game.");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchProjectHtmlAsync("chess");

        Assert.NotNull(result);
        Assert.Contains(">Chess Game</h1>", result);
        Assert.Contains("A multiplayer chess game", result);
    }

    [Fact]
    public async Task FetchProjectHtmlAsync_UsesProjectImgCssPrefix()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("![screenshot](https://example.com/screen.jpg)");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchProjectHtmlAsync("slug");

        Assert.NotNull(result);
        // Programming service must use "project-img" prefix, not "build-img".
        Assert.Contains("class=\"project-img-wrap\"", result);
        Assert.DoesNotContain("class=\"build-img-wrap\"", result);
    }

    [Fact]
    public async Task FetchProjectHtmlAsync_RewritesRelativeLinksUsingProgrammingRoute()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("[Details](details-page)");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchProjectHtmlAsync("slug");

        Assert.NotNull(result);
        Assert.Contains("href=\"/programming/slug/details-page\"", result);
    }

    [Fact]
    public async Task FetchProjectHtmlAsync_CalledTwiceWithSameSlug_FetchesMarkdownOnce()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "slug.md"))
                   .ReturnsAsync("# Content");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");
        var sut = CreateSut();

        await sut.FetchProjectHtmlAsync("slug");
        await sut.FetchProjectHtmlAsync("slug");

        _dataSource.Verify(source => source.FetchMarkdownAsync("slug", "slug.md"), Times.Once);
    }

    // ── FetchSubPageHtmlAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task FetchSubPageHtmlAsync_DataSourceReturnsNull_ReturnsNull()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "architecture.md"))
                   .ReturnsAsync((string?)null);
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "architecture");

        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSubPageHtmlAsync_ValidMarkdown_ReturnsHtml()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "architecture.md"))
                   .ReturnsAsync("## Architecture\n\nDescription.");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "architecture");

        Assert.NotNull(result);
        Assert.Contains(">Architecture</h2>", result);
    }

    [Fact]
    public async Task FetchSubPageHtmlAsync_DoesNotRewriteRelativeLinks()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "subpage.md"))
                   .ReturnsAsync("[link](other-page)");
        _dataSource.Setup(source => source.ResolveAssetUrl(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns((string entrySlug, string assetPath) => $"/cdn/{entrySlug}/{assetPath}");

        var result = await CreateSut().FetchSubPageHtmlAsync("slug", "subpage");

        Assert.NotNull(result);
        Assert.Contains("href=\"other-page\"", result);
        Assert.DoesNotContain("/programming/", result);
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
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "architecture.md"))
                   .ReturnsAsync("---\ntitle: \"Architecture Overview\"\n---\nContent.");

        var result = await CreateSut().FetchSubPageTitleAsync("slug", "architecture");

        Assert.Equal("Architecture Overview", result);
    }

    [Fact]
    public async Task FetchSubPageTitleAsync_NoFrontMatterTitle_ReturnsTitleCasedSubpageName()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "getting-started.md"))
                   .ReturnsAsync("# Content without front matter title");

        var result = await CreateSut().FetchSubPageTitleAsync("slug", "getting-started");

        Assert.Equal("Getting Started", result);
    }

    [Fact]
    public async Task FetchSubPageTitleAsync_CalledTwice_FetchesMarkdownOnce()
    {
        _dataSource.Setup(source => source.FetchMarkdownAsync("slug", "subpage.md"))
                   .ReturnsAsync("Content.");
        var sut = CreateSut();

        await sut.FetchSubPageTitleAsync("slug", "subpage");
        await sut.FetchSubPageTitleAsync("slug", "subpage");

        _dataSource.Verify(source => source.FetchMarkdownAsync("slug", "subpage.md"), Times.Once);
    }

    // ── ResolveCdnUrl ─────────────────────────────────────────────────────────

    [Fact]
    public void ResolveCdnUrl_DelegatesToDataSource()
    {
        _dataSource.Setup(source => source.ResolveAssetUrl("proj", "logo.svg"))
                   .Returns("https://cdn.example.com/proj/logo.svg");

        var result = CreateSut().ResolveCdnUrl("proj", "logo.svg");

        Assert.Equal("https://cdn.example.com/proj/logo.svg", result);
    }
}
