// World map terrain brush.
//
// Deliberately narrow: only the brush lives here, because only the brush is a drag that needs
// to keep up with the pointer. Tracing a road or a river is discrete clicks, so the page
// handles those in Blazor. Modelled on fogPaint in app.js, which already drags across the
// same .wm-fc hex overlay and reports a whole stroke in one call.
window.worldEdit = (function () {
    let dotnet = null;
    let enabled = false;
    let terrain = null;      // the palette entry being painted
    let fill = null;         // its colour, for the live preview
    let wide = false;        // false = one hex, true = the hex and its six neighbours
    let painting = false;
    let seen = null;         // "q,r" -> [q, r], so one stroke reports each hex once

    const cellAt = (x, y) => {
        const el = document.elementFromPoint(x, y);
        return el && el.classList && el.classList.contains('wm-fc') ? el : null;
    };

    // Even rows are the ones nudged right in this layout, so which way the diagonals lean
    // depends on the row's parity. Mirrors WorldMapService.Neighbours.
    function blob(q, r) {
        const lean = (r & 1) === 0 ? 0 : -1;
        return [
            [q, r],
            [q + 1, r], [q - 1, r],
            [q + 1 + lean, r - 1], [q + lean, r - 1],
            [q + 1 + lean, r + 1], [q + lean, r + 1],
        ];
    }

    // Repaint the hex under the overlay cell so the stroke is visible while it is being drawn;
    // the authoritative redraw happens when .NET rebuilds the map on pointer-up.
    function preview(q, r) {
        const hex = document.querySelector(`.wm-hex[data-q="${q}"][data-r="${r}"]`);
        if (hex && fill) { hex.style.fill = fill; hex.style.stroke = fill; }
    }

    function paint(el) {
        const q = +el.getAttribute('data-q');
        const r = +el.getAttribute('data-r');
        for (const [bq, br] of (wide ? blob(q, r) : [[q, r]])) {
            const k = bq + ',' + br;
            if (seen.has(k)) continue;
            seen.set(k, [bq, br]);
            preview(bq, br);
        }
    }

    function onDown(e) {
        if (!enabled || e.button !== 0 || !terrain) return;
        const el = cellAt(e.clientX, e.clientY);
        if (!el) return;
        e.preventDefault();
        e.stopPropagation();          // keep the map from panning under the brush
        painting = true;
        seen = new Map();
        paint(el);
    }

    function onMove(e) {
        if (!painting) return;
        const el = cellAt(e.clientX, e.clientY);
        if (el) paint(el);
    }

    async function onUp() {
        if (!painting) return;
        painting = false;
        if (!seen || seen.size === 0) return;

        // Flat [q, r, q, r, …], the same shape ApplyFogBatch takes.
        const flat = [];
        for (const [q, r] of seen.values()) { flat.push(q); flat.push(r); }
        seen = null;
        await dotnet.invokeMethodAsync('ApplyTerrainBatch', flat, terrain);
    }

    return {
        init(frame, ref) {
            dotnet = ref;
            if (frame._we) return;
            frame._we = true;
            frame.addEventListener('pointerdown', onDown, true);
            window.addEventListener('pointermove', onMove);
            window.addEventListener('pointerup', onUp);
        },
        setBrush(on, value, colour, isWide) {
            enabled = on;
            terrain = value;
            fill = colour;
            wide = !!isWide;
        },
    };
})();
