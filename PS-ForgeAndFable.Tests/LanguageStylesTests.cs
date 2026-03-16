namespace ForgeAndFable.Tests;

public class LanguageStylesTests
{
    // ── Known languages ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("c#")]
    [InlineData("python")]
    [InlineData("javascript")]
    [InlineData("typescript")]
    [InlineData("rust")]
    [InlineData("go")]
    [InlineData("lua")]
    [InlineData("java")]
    [InlineData("c++")]
    [InlineData("c")]
    [InlineData("html")]
    [InlineData("css")]
    [InlineData("shell")]
    [InlineData("bash")]
    [InlineData("gdscript")]
    [InlineData("ruby")]
    [InlineData("swift")]
    [InlineData("kotlin")]
    [InlineData("dart")]
    [InlineData("php")]
    [InlineData("elixir")]
    [InlineData("haskell")]
    [InlineData("scala")]
    [InlineData("r")]
    [InlineData("makefile")]
    [InlineData("dockerfile")]
    public void GetBadgeStyle_KnownLanguage_ReturnsNonEmptyStyle(string language)
    {
        var result = LanguageStyles.GetBadgeStyle(language);

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("background:", result);
        Assert.Contains("color:", result);
    }

    [Fact]
    public void GetBadgeStyle_CSharp_ReturnsDistinctPurpleStyle()
    {
        var result = LanguageStyles.GetBadgeStyle("c#");

        Assert.Contains("c375d9", result);
    }

    [Fact]
    public void GetBadgeStyle_Python_ReturnsDistinctBlueStyle()
    {
        var result = LanguageStyles.GetBadgeStyle("python");

        Assert.Contains("69b4f5", result);
    }

    // ── Case insensitivity ────────────────────────────────────────────────────

    [Theory]
    [InlineData("C#")]
    [InlineData("PYTHON")]
    [InlineData("Python")]
    [InlineData("JavaScript")]
    [InlineData("JAVASCRIPT")]
    [InlineData("TypeScript")]
    [InlineData("Rust")]
    [InlineData("Go")]
    public void GetBadgeStyle_UppercaseInput_ReturnsSameStyleAsLowercase(string language)
    {
        var lowercase = LanguageStyles.GetBadgeStyle(language.ToLowerInvariant());
        var actual    = LanguageStyles.GetBadgeStyle(language);

        Assert.Equal(lowercase, actual);
    }

    // ── Unknown / fallback ────────────────────────────────────────────────────

    [Theory]
    [InlineData("cobol")]
    [InlineData("fortran")]
    [InlineData("unknown-lang")]
    [InlineData("")]
    public void GetBadgeStyle_UnknownLanguage_ReturnsFallbackGreyStyle(string language)
    {
        var result = LanguageStyles.GetBadgeStyle(language);

        // Fallback style uses 9ab0c0 as the colour token.
        Assert.Contains("9ab0c0", result);
    }

    [Fact]
    public void GetBadgeStyle_KnownLanguage_StyleDiffersFromFallback()
    {
        var fallback = LanguageStyles.GetBadgeStyle("not-a-language");
        var known    = LanguageStyles.GetBadgeStyle("rust");

        Assert.NotEqual(fallback, known);
    }

    // ── Style string format ───────────────────────────────────────────────────

    [Theory]
    [InlineData("c#")]
    [InlineData("python")]
    [InlineData("unknown")]
    public void GetBadgeStyle_AnyInput_StyleStringEndsWithSemicolon(string language)
    {
        var result = LanguageStyles.GetBadgeStyle(language);

        Assert.EndsWith(";", result);
    }
}
