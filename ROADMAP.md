# Portfolio Site Roadmap

A personal portfolio and writing showcase built in .NET 8 Blazor Server with MudBlazor.

**Hosting:** Personal server (existing hosting, custom domain)
**Goal:** A clean, up-to-date site I can hand to anyone at a networking event that tells them who I am, shows my programming work, and showcases my creative writing projects.

---

## Phase 1 — Foundation ✦ _Quick wins_

Get the skeleton looking good and something live worth sharing.

- [x] Clean up template cruft — remove placeholder pages (Counter, Weather)
- [ ] Finalise layout and nav structure (MudBlazor `MudAppBar` / drawer)
- [ ] **Hero section** — photo, 2–3 sentence intro (who I am, interests, what's on the site)
- [ ] **About / intro card** — a little more detail, links to GitHub and LinkedIn
- [ ] **GitHub projects preview** — pull top/pinned repos from GitHub API, display as cards with language, description, star count
- [ ] Basic dark/light theme toggle (MudBlazor theming)
- [ ] Mobile-responsive layout pass
- [ ] Replace placeholder favicon and app icons with personal branding

---

## Phase 2 — Programming Projects

A proper showcase of work, not just a GitHub link dump.

- [ ] **Projects index page** — grid/card layout with category filter tabs
  - Categories: Utilities & Tools · Learning Projects · Game Mods
- [ ] **Individual project pages** — dynamic routing (`/projects/{slug}`)
  - Custom description, screenshots, tech stack tags, GitHub link
  - Some pages link out to GitHub; others are self-contained showcases
- [ ] **Love2D Chess** — WebAssembly embed on its own project page
  - Compile Love2D-Chess to WASM via Emscripten / love.js
  - Embed in an iframe on `/projects/chess`
- [ ] **Love2D Asteroids** — same treatment as Chess if WASM build is viable
- [ ] GitHub API integration — live star counts, last-commit dates on project cards
- [ ] Tag/filter system (language, category)

---

## Phase 3 — Writing Section

Showcase world-building and fiction writing work. The source writing repo is private; only compiled/published output surfaces on the site.

- [ ] **Writing index page** — overview of writing projects, split by type
  - World-building projects (e.g. Opus Civile)
  - Active story projects
  - Skill/author study repos (brief mention only)
- [ ] **Chapter progress dashboard** — reads YAML frontmatter status fields from a published content bundle; shows chapter statuses, word counts, arc progress
- [ ] **World-building wiki** — markdown files compiled and served as navigable wiki pages (`/writing/{project}/wiki/{page}`)
- [ ] **Story reader** — host the latest compiled version of the active story (`/writing/{project}/read`)
- [ ] **GitHub Actions pipeline** — extend existing PDF-build action to also publish a JSON content bundle (chapter metadata, rendered HTML) to a public endpoint the Blazor site can fetch
- [ ] Auto-update hook — site rebuilds/refreshes content when writing repo publishes a new bundle

---

## Phase 4 — Polish & Extras

- [ ] SEO meta tags, Open Graph cards (so links look good when shared)
- [ ] `/cv` page or downloadable PDF CV (sourced from the Resume repo)
- [ ] Contact section — email link (using custom domain), GitHub, LinkedIn
- [ ] Accessibility pass (ARIA labels, keyboard nav, colour contrast)
- [ ] Performance — lazy loading, image optimisation
- [ ] Custom 404 page

---

## Completed

- Removed Counter, Weather, and Home2 template placeholder pages; cleaned up NavMenu; added `.DS_Store` to `.gitignore`.

---

## Notes & Decisions

- **Blazor Server** — chosen deliberately; hosted on existing personal server with custom domain. No need to migrate to WASM.
- **MudBlazor** — already wired up as the component library.
- **Writing content privacy** — source writing repo stays private. Only a compiled/processed output bundle is ever made public, fetched by the site at runtime or build time.
- **Love2D WASM** — Love2D has official web export support via Emscripten. Chess and Asteroids are candidates for embedded project pages.
- **GitHub API** — use the public REST API for repo data (no auth needed for public repos, rate limit is generous for a portfolio site).
