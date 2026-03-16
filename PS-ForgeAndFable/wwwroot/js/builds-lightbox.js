/**
 * builds-lightbox.js
 *
 * Minimal JS surface for the Blazor BuildLightbox component.
 * All overlay markup, open/close state, and keyboard handling live in
 * BuildLightbox.razor — this file only provides two helpers that require
 * direct DOM access:
 *
 *   blbOpen(el)  — focuses the overlay element (enables Escape key handling)
 *                  and locks body scroll while the lightbox is visible.
 *
 *   blbClose()   — restores body scroll when the lightbox is dismissed
 *                  (called from C# on close and on component disposal).
 */

window.blbOpen = function (el) {
    if (el && el.focus) el.focus();
    document.body.style.overflow = 'hidden';
};

window.blbClose = function () {
    document.body.style.overflow = '';
};
