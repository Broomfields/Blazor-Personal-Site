namespace Blazor_Personal_Site.Models;

/// <summary>
/// A single commit record fetched from the GitHub REST API /commits endpoint.
/// Only the first line of the commit message is stored (the subject line).
/// </summary>
public class GitHubCommit
{
    /// <summary>First line of the commit message (subject line).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Display name of the commit author.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>UTC timestamp the commit was authored.</summary>
    public DateTime Date { get; set; }
}
