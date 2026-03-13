/**
 * builds-lightbox.js
 *
 * Uses EVENT DELEGATION — a single click listener on `document` intercepts
 * clicks on any <img> inside a .build-content element, regardless of when
 * Blazor injected that content into the DOM.
 *
 * No Blazor JS-interop call is required. The script is loaded once in
 * App.razor and works automatically for every build page.
 */
window.buildsLightbox = (function () {
    'use strict';

    var overlay    = null;
    var lightboxImg  = null;
    var lightboxLink = null;

    // ── Build the overlay DOM (once) ─────────────────────────────────────────

    function buildOverlay() {
        if (overlay) return;

        overlay = document.createElement('div');
        overlay.id = 'builds-lightbox';
        overlay.setAttribute('role', 'dialog');
        overlay.setAttribute('aria-modal', 'true');
        overlay.setAttribute('aria-label', 'Image preview');

        overlay.innerHTML =
            '<div class="blb-backdrop"></div>' +
            '<div class="blb-panel">' +
            '  <button class="blb-close" aria-label="Close image preview">&#x2715;</button>' +
            '  <img class="blb-img" alt="" />' +
            '  <a class="blb-link" target="_blank" rel="noopener noreferrer">&#x2197;&nbsp;View full image</a>' +
            '</div>';

        document.body.appendChild(overlay);

        lightboxImg  = overlay.querySelector('.blb-img');
        lightboxLink = overlay.querySelector('.blb-link');

        overlay.querySelector('.blb-backdrop').addEventListener('click', close);
        overlay.querySelector('.blb-close').addEventListener('click', close);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') close();
        });
    }

    // ── Open / close ─────────────────────────────────────────────────────────

    function open(src, alt) {
        buildOverlay();
        lightboxImg.src   = src;
        lightboxImg.alt   = alt || '';
        lightboxLink.href = src;
        overlay.classList.add('is-open');
        document.body.style.overflow = 'hidden';
        overlay.querySelector('.blb-close').focus();
    }

    function close() {
        if (!overlay) return;
        overlay.classList.remove('is-open');
        document.body.style.overflow = '';
    }

    // ── Event delegation ─────────────────────────────────────────────────────
    //
    // Attached immediately when the script loads — no init() call needed.
    // Catches clicks anywhere inside a .build-img-wrap (the container injected
    // by CadBuildsService.WrapImages around every <img>). This covers clicks
    // on the image itself and on the zoom-hint overlay span.

    document.addEventListener('click', function (e) {
        var wrap = e.target.closest('.build-img-wrap');
        if (wrap && wrap.closest('.build-content')) {
            e.preventDefault();
            var img = wrap.querySelector('img');
            if (img) open(img.src, img.alt);
        }
    });

    return { open: open, close: close };
}());
