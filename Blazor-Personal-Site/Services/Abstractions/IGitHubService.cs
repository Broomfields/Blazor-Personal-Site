using Blazor_Personal_Site.Models;

namespace Blazor_Personal_Site.Services.Abstractions;

/// <summary>
/// Abstraction over <see cref="GitHubService"/> consumed by Razor components.
///
/// Decouples component code from the concrete service implementation, making
/// it straightforward to substitute a test double in unit/component tests.
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Fetches (and caches) public repository metadata from the GitHub REST API.
    /// Returns null if the URL is empty, unparseable, or the API call fails.
    /// </summary>
    Task<GitHubRepoStats?> FetchRepoStatsAsync(string? repoUrl);
}
