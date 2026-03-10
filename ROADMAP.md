# Portfolio Site Roadmap

A personal portfolio and writing showcase built in .NET 8 Blazor Server with MudBlazor.

**Hosting:** Personal server (existing hosting, custom domain)
**Goal:** A clean, up-to-date site I can hand to anyone at a networking event that tells them who I am, shows my programming work, showcases my physical builds, and showcases my creative writing projects.

---

## Site Structure

Four main sections, each with its own index page and individual project/detail pages:

| Section | URL | Description |
|---|---|---|
| Home | `/` | Hero + feature nav |
| Programming | `/programming` | Utilities, toy projects, game mods |
| Builds | `/builds` | 3D-printed projects (functional + creative) |
| Writing | `/writing` | World-building wikis + active stories |
| Contact | `/contact` | GitHub, LinkedIn, email |

### Programming sub-categories
- Utilities & Tools
- Learning / Toy Projects (Chess, Asteroids)
- Game Mods

### Builds sub-categories
- Functional / Engineering (rocket brackets, cable tidies)
- Creative / Decorative (ornaments, display pieces)

### Writing sub-categories
- **World Building projects** — each world has a wiki + stories
  - `/writing/{world}/wiki/{page}` — navigable world bible
  - `/writing/{world}/stories/{story}` — chapter progress + reader
- Skill / author study repos stay private; not surfaced on the site

---

## Phase 1 — Foundation ✦ _Quick wins_

Get the skeleton looking good and something live worth sharing.

- [x] Clean up template cruft — remove placeholder pages (Counter, Weather)
- [x] Finalise layout and nav structure
  - App bar: site name + dark/light toggle only
  - Side drawer: section links (Programming, Builds, Writing, Contact)
  - Home page feature nav: horizontal line with bubble buttons (see Phase 1 nav note)
- [x] **Hero section** — photo, 2–3 sentence intro, GitHub + LinkedIn links
- [ ] **About / intro card** — a little more detail
- [ ] **GitHub projects preview** — pull top/pinned repos from GitHub API, display as cards with language, description, star count
- [x] Basic dark/light theme toggle (MudBlazor theming)
- [ ] Mobile-responsive layout pass
- [ ] Replace placeholder favicon and app icons with personal branding

### Phase 1 Nav Note
The home page feature nav is a horizontal-line design: pill-shaped buttons sit on a centred line (like stations on a timeline). Hovering a button scales it up (bubble/spring effect) and drops a preview card below it. Clicking navigates to the section page. This is implemented as `FeatureNav.razor` with a paired `FeatureNav.razor.css`.

---

## Phase 2 — Programming Projects

A proper showcase of work, not just a GitHub link dump.

- [ ] **Projects index page** — grid/card layout with sub-category filter tabs
  - Categories: Utilities & Tools · Learning Projects · Game Mods
- [ ] **Individual project pages** — dynamic routing (`/programming/{slug}`)
  - Custom description, screenshots, tech stack tags, GitHub link
- [ ] **Love2D Chess** — WebAssembly embed on its own project page
  - Compile Love2D-Chess to WASM via Emscripten / love.js
  - Embed in an iframe on `/programming/chess`
- [ ] **Love2D Asteroids** — same treatment as Chess if WASM build is viable
- [ ] GitHub API integration — live star counts, last-commit dates on project cards
- [ ] Tag/filter system (language, category)

---

## Phase 2b — Builds (3D Printing)

_"Builds" chosen as the category name: broad enough for functional engineering and creative prints._

- [ ] **Builds index page** — grid/card layout with sub-category filter
  - Categories: Functional / Engineering · Creative / Decorative
- [ ] **Individual build pages** — dynamic routing (`/builds/{slug}`)
  - Description, photos, print specs, use case
- [ ] Consider a "print files" link (Printables / Thingiverse) per project

---

## Phase 3 — Writing Section

Showcase world-building and fiction writing work. The source writing repo is private; only compiled/published output surfaces on the site.

- [ ] **Writing index page** — overview of worlds, each as a card
- [ ] **Chapter progress dashboard** — reads YAML frontmatter status fields from published content bundle; shows chapter statuses, word counts, arc progress per story
- [ ] **World-building wiki** — markdown files compiled and served as navigable wiki pages (`/writing/{world}/wiki/{page}`)
- [ ] **Story reader** — host the latest compiled version of the active story (`/writing/{world}/stories/{story}`)
- [ ] **GitHub Actions pipeline** — extend existing PDF-build action to also publish a JSON content bundle (chapter metadata, rendered HTML) to a public endpoint the Blazor site can fetch
- [ ] Auto-update hook — site rebuilds/refreshes content when writing repo publishes a new bundle

### Writing architecture decisions
- Source writing repos stay **private** — only the compiled bundle is ever public
- Skill / author study repos are **not surfaced on the site** (internal learning tools, not portfolio pieces)
- Each world is its own project entry; URL structure: `/writing/{world}/wiki/...` and `/writing/{world}/stories/...`
- Markdown-as-wiki approach is correct — version-controlled, auto-compilable, maps cleanly to the pipeline

---

## Phase 4 — Polish & Extras

- [ ] SEO meta tags, Open Graph cards (so links look good when shared)
- [ ] `/contact` page — email link (using custom domain), GitHub, LinkedIn
- [ ] Accessibility pass (ARIA labels, keyboard nav, colour contrast)
- [ ] Performance — lazy loading, image optimisation
- [ ] Custom 404 page
- [ ] `/cv` page or downloadable PDF CV (sourced from the Resume repo)

---

## Completed

- Removed Counter, Weather, and Home2 template placeholder pages; cleaned up NavMenu; added `.DS_Store` to `.gitignore`.
- Phase 1 layout and nav structure — hero with real content, feature nav, stub section pages.

---

## Notes & Decisions

- **Blazor Server** — chosen deliberately; hosted on existing personal server with custom domain. No need to migrate to WASM.
- **MudBlazor** — already wired up as the component library.
- **"Builds"** — chosen as the name for the 3D printing section. Broad enough to cover functional (rocket brackets, cable tidies) and creative (ornaments) projects; room to expand if other physical making is added later.
- **Writing content privacy** — source writing repo stays private. Only a compiled/processed output bundle is ever made public, fetched by the site at runtime or build time.
- **Writing sub-categories not on the site** — skill study repos and author study notes are internal tools, not portfolio items.
- **Love2D WASM** — Love2D has official web export support via Emscripten. Chess and Asteroids are candidates for embedded project pages.
- **GitHub API** — use the public REST API for repo data (no auth needed for public repos, rate limit is generous for a portfolio site).
