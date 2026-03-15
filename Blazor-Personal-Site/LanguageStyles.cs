namespace Blazor_Personal_Site;

/// <summary>
/// Inline CSS style strings for programming-language badge chips.
///
/// Colours are dark-mode–optimised: GitHub's canonical palette uses light-background
/// hues that are near-invisible on dark cards, so each entry uses a brightened or
/// desaturated equivalent that keeps the hue recognisable at low opacity.
///
/// Used by both <c>ProjectCard.razor</c> (card grid) and
/// <c>Programming.razor</c> (detail page) to ensure consistent language badge
/// appearance across all views.
/// </summary>
public static class LanguageStyles
{
    /// <summary>
    /// Returns an inline CSS <c>background</c>/<c>color</c> style string for the
    /// given language name. Falls back to a neutral grey for unrecognised languages.
    /// </summary>
    public static string GetBadgeStyle(string language) =>
        language.ToLowerInvariant() switch
        {
            "lua"        => "background:rgba(0,0,178,0.18);color:#5b8ef5;",
            "c#"         => "background:rgba(104,33,122,0.2);color:#c375d9;",
            "python"     => "background:rgba(53,114,165,0.2);color:#69b4f5;",
            "javascript" => "background:rgba(200,170,0,0.15);color:#e0c05a;",
            "typescript" => "background:rgba(49,120,198,0.2);color:#6ba3e0;",
            "rust"       => "background:rgba(200,100,50,0.2);color:#e08a56;",
            "go"         => "background:rgba(0,173,196,0.18);color:#50ccd8;",
            "java"       => "background:rgba(176,114,25,0.2);color:#e0a050;",
            "c++"        => "background:rgba(243,75,125,0.18);color:#e87aab;",
            "c"          => "background:rgba(85,85,85,0.25);color:#aaa;",
            "html"       => "background:rgba(200,70,30,0.18);color:#e0825a;",
            "css"        => "background:rgba(30,100,200,0.18);color:#6fa8e0;",
            "shell"      => "background:rgba(80,150,80,0.18);color:#7ec87e;",
            "bash"       => "background:rgba(80,150,80,0.18);color:#7ec87e;",
            "gdscript"   => "background:rgba(70,140,140,0.2);color:#60c0c0;",
            "ruby"       => "background:rgba(200,30,30,0.18);color:#e06060;",
            "swift"      => "background:rgba(240,81,56,0.18);color:#f09070;",
            "kotlin"     => "background:rgba(169,123,255,0.18);color:#b090f5;",
            "dart"       => "background:rgba(0,200,192,0.18);color:#50d0c8;",
            "php"        => "background:rgba(128,144,204,0.18);color:#98a8d8;",
            "elixir"     => "background:rgba(184,112,212,0.18);color:#c888e0;",
            "haskell"    => "background:rgba(152,120,212,0.18);color:#b098d8;",
            "scala"      => "background:rgba(224,64,96,0.18);color:#e08090;",
            "r"          => "background:rgba(68,170,238,0.18);color:#70b8f0;",
            "makefile"   => "background:rgba(106,170,48,0.18);color:#88c058;",
            "dockerfile" => "background:rgba(104,153,176,0.18);color:#90b8cc;",
            _            => "background:rgba(138,155,168,0.14);color:#9ab0c0;",
        };
}
