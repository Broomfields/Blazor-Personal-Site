using Blazor_Personal_Site.Components;
using Blazor_Personal_Site.Services;
using Blazor_Personal_Site.Services.Abstractions;
using Blazor_Personal_Site.Services.DataSources;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;

namespace Blazor_Personal_Site
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // HttpClient factory — used by GitHubCmsDataSource (avoids socket exhaustion)
            builder.Services.AddHttpClient();

            // Named HTTP client for the GitHub REST API.
            // The API requires a User-Agent header on every request.
            builder.Services.AddHttpClient("github", client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Blazor-Personal-Site/1.0");
            });

            // ── CMS data sources ──────────────────────────────────────────────────
            // "GitHub" (default) fetches from GitHub over HTTP.
            // "Local"  reads directly from local repo clones on disk,
            //          enabling testing against local content branches without pushing.
            // Controlled via "Cms:Source" in appsettings (override in
            // appsettings.Development.json to activate local mode during development).
            //
            // Each CMS is registered as a keyed ICmsDataSource so future consumers
            // (ProgrammingService, WritingService, …) can each inject their own source.
            // To add a new CMS: register another keyed entry here, add a corresponding
            // service below, and add its key to the local dev CDN list further down.
            var cmsSource = builder.Configuration["Cms:Source"] ?? "GitHub";

            if (cmsSource.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddKeyedSingleton<ICmsDataSource>("builds", (sp, _) =>
                    new LocalCmsDataSource(
                        repoRoot:      builder.Configuration["Cms:LocalPaths:builds"] ?? string.Empty,
                        contentFolder: "builds",
                        logger:        sp.GetRequiredService<ILogger<LocalCmsDataSource>>()));

                builder.Services.AddKeyedSingleton<ICmsDataSource>("programming", (sp, _) =>
                    new LocalCmsDataSource(
                        repoRoot:      builder.Configuration["Cms:LocalPaths:programming"] ?? string.Empty,
                        contentFolder: "projects",
                        logger:        sp.GetRequiredService<ILogger<LocalCmsDataSource>>()));
            }
            else
            {
                builder.Services.AddKeyedSingleton<ICmsDataSource>("builds", (sp, _) =>
                    new GitHubCmsDataSource(
                        repoName:          "PS-CMS-Builds",
                        contentFolder:     "builds",
                        httpClientFactory: sp.GetRequiredService<IHttpClientFactory>(),
                        logger:            sp.GetRequiredService<ILogger<GitHubCmsDataSource>>()));

                builder.Services.AddKeyedSingleton<ICmsDataSource>("programming", (sp, _) =>
                    new GitHubCmsDataSource(
                        repoName:          "PS-CMS-Programming",
                        contentFolder:     "projects",
                        httpClientFactory: sp.GetRequiredService<IHttpClientFactory>(),
                        logger:            sp.GetRequiredService<ILogger<GitHubCmsDataSource>>()));
            }

            // ── CMS consumer services ─────────────────────────────────────────────
            // Singletons so in-memory caches are shared across all requests.
            builder.Services.AddSingleton<BuildsService>();
            builder.Services.AddSingleton<ProgrammingService>();

            // ── GitHub API service ────────────────────────────────────────────────
            // Fetches and caches live repo stats (stars, forks, etc.) for the
            // Programming page. Uses the named "github" HttpClient registered above.
            builder.Services.AddSingleton<GitHubService>();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // ── Local dev CDN ─────────────────────────────────────────────────────
            // When using LocalCmsDataSource, asset URLs are root-relative paths under
            // /dev-cdn/{contentFolder}/. Each registered local source gets its own
            // PhysicalFileProvider mounted at that sub-path so the browser can fetch
            // images through the running dev server without needing file:// access.
            // To add a new local CMS source: just add its key to the list below.
            if (cmsSource.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var key in new[] { "builds", "programming" })
                {
                    var source = app.Services.GetRequiredKeyedService<ICmsDataSource>(key)
                        as LocalCmsDataSource;

                    if (source is null) continue;

                    var physPath = Path.Combine(source.RepoRoot, source.ContentFolder);
                    if (!Directory.Exists(physPath))
                    {
                        app.Logger.LogWarning(
                            "Local CMS path for '{Key}' not found: {Path}. " +
                            "Asset serving is disabled for this source.",
                            key, physPath);
                        continue;
                    }

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(physPath),
                        RequestPath  = $"{LocalCmsDataSource.DevCdnPrefix}/{source.ContentFolder}"
                    });
                }
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
