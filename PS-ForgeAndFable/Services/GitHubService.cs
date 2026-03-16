using System.Text.Json;
using ForgeAndFable.Models;
using ForgeAndFable.Services.Abstractions;

namespace ForgeAndFable.Services;

/// <summary>
/// Fetches and caches public metadata from the GitHub REST API for repositories.
///
/// Requires a named HTTP client named <c>"github"</c> with a <c>User-Agent</c>
/// header configured in Program.cs (the GitHub API rejects requests without one).
///
/// Cache TTL is 30 minutes — repo stats change infrequently and the unauthenticated
/// GitHub API allows only 60 requests per hour per IP.
/// </summary>
public class GitHubService : IGitHubService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubService> _logger;

    // Key: "{owner}/{repo}"
    private readonly Dictionary<string, (GitHubRepoStats Stats, DateTime CachedAt)>
        _cache = new();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public GitHubService(
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    /// <summary>
    /// Fetches (and caches) public repo metadata from the GitHub REST API.
    /// Fires two requests in parallel: the main repos endpoint (stats + topics)
    /// and the /languages endpoint (byte counts per language for the bar chart).
    /// Returns null if the repo URL is empty, unparseable, or both API calls fail.
    /// </summary>
    public async Task<GitHubRepoStats?> FetchRepoStatsAsync(string? repoUrl)
    {
        if (string.IsNullOrWhiteSpace(repoUrl)) return null;

        var key = ExtractRepoKey(repoUrl);
        if (key is null) return null;

        if (_cache.TryGetValue(key, out var cached) &&
            DateTime.UtcNow - cached.CachedAt < CacheTtl)
            return cached.Stats;

        var http = _httpClientFactory.CreateClient("github");

        // Fire all three requests in parallel to minimise wall-clock time.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var statsTask     = FetchMainStatsAsync(http, key, cts.Token);
        var languagesTask = FetchLanguagesAsync(http, key, cts.Token);
        var commitsTask   = FetchCommitsAsync(http, key, cts.Token);

        await Task.WhenAll(statsTask, languagesTask, commitsTask);

        var stats = statsTask.Result;
        if (stats is null) return null;

        stats.Languages     = languagesTask.Result ?? new Dictionary<string, long>();
        stats.RecentCommits = commitsTask.Result   ?? new List<GitHubCommit>();

        _cache[key] = (stats, DateTime.UtcNow);
        return stats;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<GitHubRepoStats?> FetchMainStatsAsync(
        HttpClient http, string key, CancellationToken ct)
    {
        try
        {
            var response = await http.GetStringAsync(
                $"https://api.github.com/repos/{key}", ct);

            using var doc  = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var topicList = new List<string>();
            if (root.TryGetProperty("topics", out var topics) &&
                topics.ValueKind == JsonValueKind.Array)
            {
                foreach (var topic in topics.EnumerateArray())
                {
                    var topicName = topic.GetString();
                    if (!string.IsNullOrEmpty(topicName)) topicList.Add(topicName);
                }
            }

            return new GitHubRepoStats
            {
                Stars      = root.TryGetProperty("stargazers_count",  out var stars)      ? stars.GetInt32()      : 0,
                Forks      = root.TryGetProperty("forks_count",       out var forks)      ? forks.GetInt32()      : 0,
                OpenIssues = root.TryGetProperty("open_issues_count", out var openIssues) ? openIssues.GetInt32() : 0,
                PushedAt   = root.TryGetProperty("pushed_at",         out var pushedAt)   ? pushedAt.GetString()  : null,
                Topics     = topicList,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch GitHub repo stats for '{Key}'.", key);
            return null;
        }
    }

    private async Task<Dictionary<string, long>?> FetchLanguagesAsync(
        HttpClient http, string key, CancellationToken ct)
    {
        try
        {
            var response = await http.GetStringAsync(
                $"https://api.github.com/repos/{key}/languages", ct);

            using var doc = JsonDocument.Parse(response);
            var languageBytes = new Dictionary<string, long>();

            foreach (var prop in doc.RootElement.EnumerateObject())
                languageBytes[prop.Name] = prop.Value.GetInt64();

            // Sort descending by bytes so the largest language comes first.
            return languageBytes
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch GitHub language breakdown for '{Key}'.", key);
            return null;
        }
    }

    private async Task<List<GitHubCommit>?> FetchCommitsAsync(
        HttpClient http, string key, CancellationToken ct)
    {
        try
        {
            var response = await http.GetStringAsync(
                $"https://api.github.com/repos/{key}/commits?per_page=5", ct);

            using var doc = JsonDocument.Parse(response);
            var commitList = new List<GitHubCommit>();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (!item.TryGetProperty("commit", out var commit)) continue;

                // Take only the subject line (first line of the message).
                var fullMsg   = commit.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "";
                var firstLine = fullMsg.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                       .FirstOrDefault() ?? "";

                var authorName = string.Empty;
                var commitDate = DateTime.MinValue;

                if (commit.TryGetProperty("author", out var author))
                {
                    authorName = author.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "";
                    if (author.TryGetProperty("date", out var date) && date.GetString() is string dateString)
                        DateTime.TryParse(dateString, null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out commitDate);
                }

                commitList.Add(new GitHubCommit
                {
                    Message = firstLine,
                    Author  = authorName,
                    Date    = commitDate,
                });
            }

            return commitList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch GitHub commits for '{Key}'.", key);
            return null;
        }
    }

    /// <summary>
    /// Parses a GitHub URL (https://github.com/Owner/Repo) into "Owner/Repo".
    /// Returns null if the URL doesn't match the expected format.
    /// </summary>
    private static string? ExtractRepoKey(string url)
    {
        try
        {
            var uri = new Uri(url);
            if (!uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                return null;

            var parts = uri.AbsolutePath.Trim('/').Split('/');
            return parts.Length >= 2 ? $"{parts[0]}/{parts[1]}" : null;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }
}
