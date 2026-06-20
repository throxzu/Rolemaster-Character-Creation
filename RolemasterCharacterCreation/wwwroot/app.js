window.initTooltips = function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        bootstrap.Tooltip.getOrCreateInstance(el, { trigger: 'hover focus', html: false });
    });
};

window.scrollToBottom = function (el) {
    if (el) el.scrollTop = el.scrollHeight;
};

// Nudges a fixed-position popup back inside the viewport after it renders.
window.clampPopup = function (selector) {
    const el = document.querySelector(selector);
    if (!el) return;
    const m = 8;
    const r = el.getBoundingClientRect();
    const vw = window.innerWidth, vh = window.innerHeight;
    let left = r.left, top = r.top;
    if (left + r.width > vw - m) left = vw - r.width - m;
    if (left < m) left = m;
    if (top + r.height > vh - m) top = vh - r.height - m;
    if (top < m) top = m;
    el.style.left = left + 'px';
    el.style.top = top + 'px';
};

// Pan/zoom controller for the town map. Applies a CSS transform to the <svg> inside
// the given frame: wheel zooms toward the cursor, left-drag pans, and the exposed
// zoom()/reset() are driven by the on-screen buttons. A drag swallows the click that
// ends it so it never opens a building menu.
window.townMap = (function () {
    function attach(frame) {
        if (!frame || frame._tm) return;
        const svg = frame.querySelector('svg');
        if (!svg) return;
        svg.style.transformOrigin = '0 0';

        let scale = 1, tx = 0, ty = 0;
        let dragging = false, moved = false, sx = 0, sy = 0, ox = 0, oy = 0;

        const apply = () => { svg.style.transform = 'translate(' + tx + 'px,' + ty + 'px) scale(' + scale + ')'; };

        function constrain() {
            const w = frame.clientWidth, h = frame.clientHeight;
            // Zoomed in past fit: keep the edges pinned so no empty gap shows.
            // Zoomed out below fit: nothing to pan to, so anchor the smaller map to the
            // top-left rather than centring it, so there's no empty band above/left of it.
            if (scale >= 1) {
                tx = Math.min(0, Math.max(w - w * scale, tx));
                ty = Math.min(0, Math.max(h - h * scale, ty));
            } else {
                tx = 0;
                ty = 0;
            }
        }

        function zoomAt(cx, cy, factor) {
            const prev = scale;
            // Floor below 1 so the map can shrink smaller than its original fitted size.
            scale = Math.min(8, Math.max(0.25, scale * factor));
            const k = scale / prev;
            tx = cx - k * (cx - tx);
            ty = cy - k * (cy - ty);
            constrain();
            apply();
        }

        frame.addEventListener('wheel', function (e) {
            e.preventDefault();
            const r = frame.getBoundingClientRect();
            zoomAt(e.clientX - r.left, e.clientY - r.top, e.deltaY < 0 ? 1.15 : 1 / 1.15);
        }, { passive: false });

        frame.addEventListener('pointerdown', function (e) {
            if (e.button !== 0) return;
            if (e.target.closest && e.target.closest('.town-zoom-controls')) return;
            dragging = true; moved = false;
            sx = e.clientX; sy = e.clientY; ox = tx; oy = ty;
        });

        window.addEventListener('pointermove', function (e) {
            if (!dragging) return;
            const dx = e.clientX - sx, dy = e.clientY - sy;
            if (Math.abs(dx) + Math.abs(dy) > 4) { moved = true; frame.classList.add('tm-panning'); }
            tx = ox + dx; ty = oy + dy;
            constrain();
            apply();
        });

        window.addEventListener('pointerup', function () {
            dragging = false;
            frame.classList.remove('tm-panning');
        });

        frame.addEventListener('click', function (e) {
            if (moved) { e.preventDefault(); e.stopPropagation(); moved = false; }
        }, true);

        frame._tm = {
            zoom: function (factor) { zoomAt(frame.clientWidth / 2, frame.clientHeight / 2, factor); },
            reset: function () { scale = 1; tx = 0; ty = 0; apply(); }
        };
    }

    return {
        attach: attach,
        zoom: function (frame, factor) { attach(frame); if (frame && frame._tm) frame._tm.zoom(factor); },
        reset: function (frame) { if (frame && frame._tm) frame._tm.reset(); }
    };
})();

// Drag-to-paint fog reveal/hide for the world map. Painting is done client-side for a
// smooth drag, then the whole stroke is sent to .NET once on mouse-up. While enabled it
// takes over left-drag on the fog cells (so the map doesn't pan under the brush).
window.fogPaint = (function () {
    let dotnet = null, enabled = false;
    let painting = false, reveal = false, seen = null;

    function cellAt(x, y) {
        const el = document.elementFromPoint(x, y);
        return el && el.classList && el.classList.contains('wm-fc') ? el : null;
    }

    function paint(el) {
        const q = +el.getAttribute('data-q');
        const r = +el.getAttribute('data-r');
        const k = q + ',' + r;
        if (seen.has(k)) return;
        seen.set(k, [q, r]);
        el.classList.remove('wm-fc-on', 'wm-fc-off');
        el.classList.add(reveal ? 'wm-fc-on' : 'wm-fc-off');
    }

    function onDown(e) {
        if (!enabled || e.button !== 0) return;
        const el = cellAt(e.clientX, e.clientY);
        if (!el) return;
        e.preventDefault();
        e.stopPropagation();   // keep the map from panning under the brush
        painting = true;
        seen = new Map();
        reveal = !el.classList.contains('wm-fc-on'); // start on hidden → reveal; on revealed → hide
        paint(el);
    }

    function onMove(e) {
        if (!painting) return;
        const el = cellAt(e.clientX, e.clientY);
        if (el) paint(el);
    }

    function onUp() {
        if (!painting) return;
        painting = false;
        if (dotnet && seen.size) {
            const flat = [];
            seen.forEach(function (qr) { flat.push(qr[0], qr[1]); });
            dotnet.invokeMethodAsync('ApplyFogBatch', flat, reveal);
        }
        seen = null;
    }

    return {
        init: function (frame, net) {
            dotnet = net;
            if (!frame || frame._fogInit) return;
            frame._fogInit = true;
            frame.addEventListener('pointerdown', onDown, true); // capture, before pan
            window.addEventListener('pointermove', onMove);
            window.addEventListener('pointerup', onUp);
        },
        setEnabled: function (v) { enabled = !!v; }
    };
})();
