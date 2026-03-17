// ── Mermaid initialisation ────────────────────────────────────────────────
//
// Theme variables are mapped directly from the site's Icelandic Volcanic
// Terrain palette (defined in site.css :root).
//
//   #2A2723  ash-plain      → diagram canvas / edge label backgrounds
//   #3D3A35  tephra         → node fills
//   #504C45  cinder         → secondary fills, borders, lifelines
//   #7A9E35  highland-green → primary borders, arrows, active accents
//   #9CC240  lichen-flash   → signals, sequence arrows
//   #E8E4DC  text-primary   → all label text
//
mermaid.initialize({
    startOnLoad: false,
    theme: 'base',
    themeVariables: {

        // ── Canvas ──────────────────────────────────────────────────────────
        background:                 '#2A2723',
        mainBkg:                    '#2A2723',

        // ── Default node (flowchart, most diagram types) ─────────────────
        primaryColor:               '#3D3A35',
        primaryBorderColor:         '#7A9E35',
        primaryTextColor:           '#E8E4DC',

        // ── Secondary / tertiary nodes ───────────────────────────────────
        secondaryColor:             '#504C45',
        secondaryBorderColor:       '#7A9E35',
        secondaryTextColor:         '#E8E4DC',
        tertiaryColor:              '#3D3A35',
        tertiaryBorderColor:        '#504C45',
        tertiaryTextColor:          '#E8E4DC',

        // ── Edges and general text ───────────────────────────────────────
        lineColor:                  '#7A9E35',
        textColor:                  '#E8E4DC',
        edgeLabelBackground:        '#2A2723',

        // ── Flowchart subgraphs ──────────────────────────────────────────
        clusterBkg:                 '#2A2723',
        clusterBorder:              '#504C45',
        titleColor:                 '#E8E4DC',

        // ── Sequence diagram ─────────────────────────────────────────────
        actorBkg:                   '#3D3A35',
        actorBorder:                '#7A9E35',
        actorTextColor:             '#E8E4DC',
        actorLineColor:             '#504C45',
        signalColor:                '#9CC240',
        signalTextColor:            '#E8E4DC',
        labelBoxBkgColor:           '#3D3A35',
        labelBoxBorderColor:        '#7A9E35',
        labelTextColor:             '#E8E4DC',
        loopTextColor:              '#E8E4DC',
        noteBkgColor:               '#504C45',
        noteBorderColor:            '#7A9E35',
        noteTextColor:              '#E8E4DC',
        activationBkgColor:         '#504C45',
        activationBorderColor:      '#7A9E35',
        sequenceNumberColor:        '#E8E4DC',
    }
});

// ── renderMermaid ─────────────────────────────────────────────────────────
//
// Called via JS interop from Blazor pages after each render cycle.
// Mermaid tracks processed elements via data-processed, so calling run()
// repeatedly is safe — it only acts on elements it hasn't touched yet.
//
window.renderMermaid = function () {
    if (typeof mermaid !== 'undefined') {
        mermaid.run({ querySelector: '.mermaid' });
    }
};
