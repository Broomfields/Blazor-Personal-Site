using ForgeAndFable.Services.DataSources;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForgeAndFable.Tests;

public class LocalCmsDataSourceTests
{
    private static LocalCmsDataSource Create(string repoRoot, string contentFolder = "builds") =>
        new(repoRoot, contentFolder, Mock.Of<ILogger<LocalCmsDataSource>>());

    // ── ResolveAssetUrl ───────────────────────────────────────────────────────

    [Fact]
    public void ResolveAssetUrl_RelativePath_ReturnsDevCdnUrl()
    {
        var sut = Create("/repo", "builds");

        var result = sut.ResolveAssetUrl("my-build", "photo.jpg");

        Assert.Equal("/dev-cdn/builds/my-build/photo.jpg", result);
    }

    [Fact]
    public void ResolveAssetUrl_PathWithLeadingDotSlash_StripsPrefix()
    {
        var sut = Create("/repo", "builds");

        var result = sut.ResolveAssetUrl("my-build", "./photo.jpg");

        Assert.Equal("/dev-cdn/builds/my-build/photo.jpg", result);
    }

    [Fact]
    public void ResolveAssetUrl_PathWithLeadingSlash_StripsPrefix()
    {
        var sut = Create("/repo", "builds");

        var result = sut.ResolveAssetUrl("my-build", "/photo.jpg");

        Assert.Equal("/dev-cdn/builds/my-build/photo.jpg", result);
    }

    [Fact]
    public void ResolveAssetUrl_PathWithDotDotSlash_StripsLeadingDots()
    {
        var sut = Create("/repo", "builds");

        // The method strips leading '.' and '/' characters, so "../photo.jpg"
        // becomes "photo.jpg" after trimming.
        var result = sut.ResolveAssetUrl("my-build", "../photo.jpg");

        Assert.Equal("/dev-cdn/builds/my-build/photo.jpg", result);
    }

    [Fact]
    public void ResolveAssetUrl_DifferentContentFolder_UsesCorrectFolder()
    {
        var sut = Create("/repo", "programming");

        var result = sut.ResolveAssetUrl("my-project", "screenshot.png");

        Assert.Equal("/dev-cdn/programming/my-project/screenshot.png", result);
    }

    [Fact]
    public void ResolveAssetUrl_NestedPath_PreservesSubdirectory()
    {
        var sut = Create("/repo", "builds");

        var result = sut.ResolveAssetUrl("slug", "images/gallery/photo.jpg");

        Assert.Equal("/dev-cdn/builds/slug/images/gallery/photo.jpg", result);
    }

    [Fact]
    public void ResolveAssetUrl_UsesDevCdnPrefixConstant()
    {
        var sut = Create("/repo", "builds");

        var result = sut.ResolveAssetUrl("slug", "file.jpg");

        Assert.StartsWith(LocalCmsDataSource.DevCdnPrefix, result);
    }

    // ── Constructor — tilde expansion ─────────────────────────────────────────

    [Fact]
    public void Constructor_TildeSlashPath_ExpandsToHomeDirectory()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var sut = Create("~/my-cms-repo");

        Assert.Equal(Path.Combine(homePath, "my-cms-repo"), sut.RepoRoot);
    }

    [Fact]
    public void Constructor_JustTilde_ExpandsToHomeDirectory()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var sut = Create("~");

        Assert.Equal(homePath, sut.RepoRoot);
    }

    [Fact]
    public void Constructor_AbsolutePath_IsKeptUnchanged()
    {
        var sut = Create("/absolute/path/to/repo");

        Assert.Equal("/absolute/path/to/repo", sut.RepoRoot);
    }

    [Fact]
    public void Constructor_RelativePathWithoutTilde_IsKeptUnchanged()
    {
        var sut = Create("relative/path");

        Assert.Equal("relative/path", sut.RepoRoot);
    }

    // ── ContentFolder property ─────────────────────────────────────────────────

    [Theory]
    [InlineData("builds")]
    [InlineData("programming")]
    public void ContentFolder_ReflectsConstructorArgument(string folder)
    {
        var sut = Create("/repo", folder);

        Assert.Equal(folder, sut.ContentFolder);
    }
}
