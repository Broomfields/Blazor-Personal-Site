namespace Blazor_Personal_Site.Models;

/// <summary>
/// Live metadata fetched from the GitHub REST API for a public repository.
/// Cached in-memory by <see cref="Services.GitHubService"/>.
/// </summary>
public class GitHubRepoStats
{
    /// <summary>Total number of stars (stargazers).</summary>
    public int Stars { get; set; }

    /// <summary>Total number of forks.</summary>
    public int Forks { get; set; }

    /// <summary>Number of watchers subscribed to the repository.</summary>
    public int Watchers { get; set; }

    /// <summary>Number of open issues (includes pull requests in the GitHub API count).</summary>
    public int OpenIssues { get; set; }

    /// <summary>
    /// Primary language as reported by GitHub (e.g. "Lua", "C#").
    /// Null when the repo has no detected language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>ISO 8601 timestamp of the most recent push to the repository.</summary>
    public string? PushedAt { get; set; }

    /// <summary>Canonical HTML URL of the repository.</summary>
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// GitHub repository topics (labels), e.g. ["game", "lua", "chess"].
    /// Populated from the topics field in the main repos API response.
    /// Empty when the repo has no topics.
    /// </summary>
    public List<string> Topics { get; set; } = new();

    /// <summary>
    /// Language breakdown from the /languages endpoint.
    /// Key = language name, Value = bytes of code.
    /// Ordered by descending byte count. Used to render the proportional language bar and donut chart.
    /// </summary>
    public Dictionary<string, long> Languages { get; set; } = new();

    /// <summary>
    /// Up to 5 most recent commits from the /commits endpoint, newest first.
    /// Each entry carries the subject line of the commit message and the authored date.
    /// Empty when the fetch fails or the repo has no commits.
    /// </summary>
    public List<GitHubCommit> RecentCommits { get; set; } = new();
}
