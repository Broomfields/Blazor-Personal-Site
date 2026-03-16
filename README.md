
# PS-ForgeAndFable

Look, every developer has a personal site. This is mine.

<div align="center">

<img src="./docs/screenshot.png" alt="Landing page" width="500" />

**[🌐 shaunbroomfield.com](https://shaunbroomfield.com)** &nbsp;·&nbsp; **[📋 Roadmap](./ROADMAP.md)**

</div>

Built on .NET 8 Blazor Server with MudBlazor, which probably tells you something about me already. It's a living showcase — programming projects, physical builds, and fiction writing, each updated as the repos behind them move forward. Right now it's all hobby stuff. Maybe some of it turns into something more eventually. That's the hope, anyway — especially on the writing side.

The **PS-** prefix stands for Personal Site — an internal convention that ties this repo and its CMS siblings together as a family. **Forge & Fable** is the internal project name; thematic with what the site is doing, but more of a behind-the-scenes label than something splashed across the front page.

The colour palette came out of the world-building. The main writing project is **World of Essentia**. Its first story, *All She Gave the Flame*, features a volcanic archipelago: black sand, fertile volcanic soil, mountains and coastlines smothered in greenery because the land is that rich. I kept coming back to reference photos of places like Mælifell in Iceland: a moss-covered volcanic cone rising out of black sand near Mýrdalsjökull. Dark basalts, muted greens, earthy warmth. Eventually those images just ended up in the site itself. The writing and the site fed each other a bit. That felt right.

## What's on the site

**Programming** — utilities I needed and built myself, learning projects *(chess, asteroids)*, game mods. Some were fun. Some were just: I needed a thing, I had the tools to make it.

**Builds** — 3D printing. Most of it is utilitarian — I needed something, I had a printer, I modelled it and made it. I don't get to make things that enter the physical world as often as I'd like, but this is where it lives when I do.

**Writing** — world-building and fiction. Each project is its own world, with a wiki for the lore and stories for the actual writing. Multiple stories can live in the same world; the wiki grows alongside them. **World of Essentia** is the main project — *All She Gave the Flame* is its first story, set in that volcanic archipelago. Plans, chapter outlines, and world-building documents all stay private. Only the compiled output makes it onto the site: the wiki pages and the written chapters themselves.

**Career** — a timeline of my professional path as a software engineer. It's here because it's part of who I am. But it's not the point of this place.

## Tech stack

- [.NET 8, Blazor Server](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
- [MudBlazor](https://mudblazor.com)
- C#
- Hosted on my own server, custom domain

Started from a MudBlazor project template. Blazor Server was a deliberate choice. I have a server. It runs fine. I don't need WASM.

## Running locally

```bash
git clone https://github.com/broomfields/PS-ForgeAndFable.git
cd PS-ForgeAndFable
dotnet run
```

Needs the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Should come up at `https://localhost:5001` or wherever it tells you.

## License

MIT — see [LICENSE](LICENSE) for details.
