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

// Pan/zoom controller shared by every map view (town, village, dungeon, cave, building,
// world). Applies a CSS transform to the <svg> inside the given frame: the wheel and a
// two-finger pinch zoom toward the pointer, dragging pans at any zoom level, and the
// exposed zoom()/reset() drive the on-screen buttons. A drag swallows the click that ends
// it so panning never opens a location menu.
window.townMap = (function () {
    const MIN_SCALE = 0.2, MAX_SCALE = 16;
    const clamp = (v, lo, hi) => Math.min(hi, Math.max(lo, v));

    function attach(frame) {
        if (!frame || frame._tm) return;
        const svg = frame.querySelector('svg');
        if (!svg) return;
        svg.style.transformOrigin = '0 0';

        let scale = 1, tx = 0, ty = 0;
        let dragging = false, moved = false, sx = 0, sy = 0, ox = 0, oy = 0;
        // Set when a gesture actually moved the map, so the click that ends the drag can be
        // swallowed. Cleared when the next gesture starts, or the first click after any pan
        // would be eaten too — including a click on the zoom buttons.
        let suppressClick = false;

        // Live pointers on the frame, so two of them can pinch.
        const points = new Map();
        let pinchDist = 0, pinchScale = 1;

        const apply = () => {
            svg.style.transform = 'translate(' + tx + 'px,' + ty + 'px) scale(' + scale + ')';
        };

        // On-screen size of the map. A CSS transform doesn't affect layout, so clientWidth
        // stays the untransformed size and the current scale gives the visible extent.
        function content() {
            return {
                w: (svg.clientWidth  || frame.clientWidth)  * scale,
                h: (svg.clientHeight || frame.clientHeight) * scale,
            };
        }

        // Zoomed past fit on an axis: pin that edge so no empty band shows beside the map.
        // Smaller than the frame: let it be dragged freely within the frame, so a zoomed-out
        // map can be positioned instead of being stuck in the corner.
        function constrain() {
            const fw = frame.clientWidth, fh = frame.clientHeight;
            const { w, h } = content();
            tx = w >= fw ? clamp(tx, fw - w, 0) : clamp(tx, 0, fw - w);
            ty = h >= fh ? clamp(ty, fh - h, 0) : clamp(ty, 0, fh - h);
        }

        // Zoom about a point in frame coordinates, so whatever is under the cursor or the
        // pinch centre stays put.
        function zoomAt(cx, cy, factor) {
            const prev = scale;
            scale = clamp(scale * factor, MIN_SCALE, MAX_SCALE);
            if (scale === prev) return;
            const k = scale / prev;
            tx = cx - k * (cx - tx);
            ty = cy - k * (cy - ty);
            constrain();
            apply();
        }

        function centre() {
            const fw = frame.clientWidth, fh = frame.clientHeight;
            const { w, h } = content();
            if (w < fw) tx = (fw - w) / 2;
            if (h < fh) ty = (fh - h) / 2;
            constrain();
            apply();
        }

        function local(e) {
            const r = frame.getBoundingClientRect();
            return { x: e.clientX - r.left, y: e.clientY - r.top };
        }

        function pair() {
            const [a, b] = [...points.values()];
            return {
                dist: Math.hypot(a.x - b.x, a.y - b.y),
                cx: (a.x + b.x) / 2,
                cy: (a.y + b.y) / 2,
            };
        }

        frame.addEventListener('wheel', function (e) {
            e.preventDefault();
            const p = local(e);
            zoomAt(p.x, p.y, e.deltaY < 0 ? 1.12 : 1 / 1.12);
        }, { passive: false });

        frame.addEventListener('pointerdown', function (e) {
            // A new gesture: whatever the last one did, this click is the user's own.
            suppressClick = false;
            if (e.pointerType === 'mouse' && e.button !== 0) return;
            if (e.target.closest && e.target.closest('.town-zoom-controls')) return;

            points.set(e.pointerId, { x: e.clientX, y: e.clientY });

            if (points.size === 2) {
                // Second finger down: stop panning and start a pinch from the current spread.
                dragging = false;
                frame.classList.remove('tm-panning');
                const p = pair();
                pinchDist = p.dist;
                pinchScale = scale;
                return;
            }

            dragging = true; moved = false;
            sx = e.clientX; sy = e.clientY; ox = tx; oy = ty;
        });

        window.addEventListener('pointermove', function (e) {
            // Navigating between maps leaves the old frame's listeners behind; ignore them.
            if (!frame.isConnected) return;
            if (points.has(e.pointerId)) points.set(e.pointerId, { x: e.clientX, y: e.clientY });

            if (points.size === 2) {
                const p = pair();
                if (pinchDist > 0) {
                    const r = frame.getBoundingClientRect();
                    moved = true;
                    zoomAt(p.cx - r.left, p.cy - r.top, (p.dist / pinchDist) * (pinchScale / scale));
                }
                return;
            }

            if (!dragging) return;
            const dx = e.clientX - sx, dy = e.clientY - sy;
            if (Math.abs(dx) + Math.abs(dy) > 4) { moved = true; frame.classList.add('tm-panning'); }
            tx = ox + dx; ty = oy + dy;
            constrain();
            apply();
        });

        function endPointer(e) {
            points.delete(e.pointerId);
            if (points.size < 2) pinchDist = 0;
            // Lifting one finger of a pinch hands control back to the remaining one.
            if (points.size === 1) {
                const [p] = [...points.values()];
                dragging = true;
                sx = p.x; sy = p.y; ox = tx; oy = ty;
            } else if (points.size === 0) {
                dragging = false;
                suppressClick = moved;
                moved = false;
                frame.classList.remove('tm-panning');
            }
        }

        window.addEventListener('pointerup', endPointer);
        window.addEventListener('pointercancel', endPointer);

        // Re-fit after a resize so the map never ends up stranded outside the frame.
        window.addEventListener('resize', function () { constrain(); apply(); });

        frame.addEventListener('click', function (e) {
            if (suppressClick) { e.preventDefault(); e.stopPropagation(); suppressClick = false; }
        }, true);

        frame._tm = {
            zoom: function (factor) { zoomAt(frame.clientWidth / 2, frame.clientHeight / 2, factor); },
            reset: function () { scale = 1; tx = 0; ty = 0; centre(); },
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

// ── Session note editor ─────────────────────────────────────────────────────
// Quill wrapper for the two rich-text fields on the GM's session notes page. The .NET side
// pulls the HTML with getHtml() when Save is pressed rather than being told about every
// keystroke, which would chatter across the circuit for no benefit.
window.noteEditor = (function () {
    const editors = new Map();          // key -> Quill instance

    const TOOLBAR = [
        [{ header: [1, 2, 3, false] }],
        ['bold', 'italic', 'underline', 'strike'],
        [{ list: 'ordered' }, { list: 'bullet' }],
        [{ align: [] }],
        ['blockquote', 'code-block', 'link'],
        ['image'],
        ['clean'],
    ];

    // A pasted phone screenshot is routinely several megabytes and 4000px wide, which is both
    // far more than the page can show and a lot to keep in SQL. Re-encode a display copy in
    // the browser: it keeps ImageSharp out of the server entirely, and the untouched original
    // is uploaded alongside so nothing is actually lost.
    const MAX_EDGE = 1600;

    function downscale(file) {
        return new Promise((resolve) => {
            const url = URL.createObjectURL(file);
            const img = new Image();
            img.onload = () => {
                URL.revokeObjectURL(url);
                const scale = Math.min(1, MAX_EDGE / Math.max(img.width, img.height));
                if (scale === 1 && file.size < 400 * 1024) {
                    // Already small: let it be its own display copy rather than re-encoding
                    // a screenshot of text into something blurrier than it started.
                    resolve({ blob: file, width: img.width, height: img.height, verbatim: true });
                    return;
                }
                const canvas = document.createElement('canvas');
                canvas.width = Math.round(img.width * scale);
                canvas.height = Math.round(img.height * scale);
                const ctx = canvas.getContext('2d');
                ctx.imageSmoothingQuality = 'high';
                ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
                canvas.toBlob(
                    (blob) => resolve({ blob, width: canvas.width, height: canvas.height, verbatim: false }),
                    'image/webp', 0.85);
            };
            img.onerror = () => { URL.revokeObjectURL(url); resolve(null); };
            img.src = url;
        });
    }

    function antiforgeryToken() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    async function upload(file, gmOnly) {
        const small = await downscale(file);
        if (!small || !small.blob) throw new Error('That file could not be read as an image.');

        const form = new FormData();
        const name = file.name || 'pasted.png';
        form.append('display', small.blob, small.verbatim ? name : name.replace(/\.[^.]+$/, '') + '.webp');
        if (!small.verbatim) form.append('original', file, name);
        form.append('width', small.width);
        form.append('height', small.height);
        form.append('gmOnly', gmOnly ? 'true' : 'false');
        form.append('__RequestVerificationToken', antiforgeryToken());

        const res = await fetch('/session-notes/image', { method: 'POST', body: form });
        if (!res.ok) throw new Error(await res.text() || 'The upload was refused.');
        return await res.json();
    }

    function insertImage(quill, url) {
        const range = quill.getSelection(true);
        quill.insertEmbed(range.index, 'image', url, 'user');
        quill.setSelection(range.index + 1, 0, 'user');
    }

    async function handleFiles(quill, files, gmOnly, onError) {
        for (const file of files) {
            if (!file.type.startsWith('image/')) continue;
            try {
                const result = await upload(file, gmOnly);
                insertImage(quill, result.url);
            } catch (e) {
                onError(e.message || String(e));
            }
        }
    }

    return {
        init: function (key, element, initialHtml, gmOnly, dotNetRef) {
            if (editors.has(key)) this.destroy(key);

            // .NET passes the element's id. Quill takes a node or a CSS selector, so a bare id
            // has to be looked up rather than handed straight over.
            const el = typeof element === 'string'
                ? (document.getElementById(element) || document.querySelector(element))
                : element;
            if (!el) { console.error('noteEditor: no element for ' + element); return; }

            const quill = new Quill(el, {
                theme: 'snow',
                modules: { toolbar: TOOLBAR },
                placeholder: 'Write the session up…',
            });
            if (initialHtml) quill.clipboard.dangerouslyPasteHTML(initialHtml, 'silent');

            const report = (msg) => {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnEditorError', msg);
                else console.error(msg);
            };

            // Toolbar button, paste and drop all funnel into the same upload path.
            quill.getModule('toolbar').addHandler('image', () => {
                const input = document.createElement('input');
                input.type = 'file';
                input.accept = 'image/png,image/jpeg,image/webp,image/gif';
                input.onchange = () => handleFiles(quill, input.files, gmOnly, report);
                input.click();
            });

            // Capture phase, because Quill binds its own paste handler during construction and
            // would otherwise get there first and inline the picture as a base64 data: URI —
            // which the sanitiser then drops, and which bloats the note enormously besides.
            quill.root.addEventListener('paste', (e) => {
                const files = [...(e.clipboardData?.files || [])].filter(f => f.type.startsWith('image/'));
                if (files.length === 0) return;      // let Quill handle pasted text normally
                e.preventDefault();
                e.stopPropagation();
                handleFiles(quill, files, gmOnly, report);
            }, true);

            quill.root.addEventListener('drop', (e) => {
                const files = [...(e.dataTransfer?.files || [])].filter(f => f.type.startsWith('image/'));
                if (files.length === 0) return;
                e.preventDefault();
                e.stopPropagation();
                handleFiles(quill, files, gmOnly, report);
            }, true);

            editors.set(key, quill);
        },

        getHtml: function (key) {
            const quill = editors.get(key);
            if (!quill) return '';
            // Quill writes "<p><br></p>" for an empty document; report that as empty so the
            // note stores NULL rather than a paragraph containing nothing.
            const html = quill.root.innerHTML;
            return (html === '<p><br></p>' || html.trim() === '') ? '' : html;
        },

        destroy: function (key) {
            const quill = editors.get(key);
            if (!quill) return;
            // Quill inserts its toolbar as a sibling before the container. Blazor does not own
            // that node, so without this a reopened editor stacks a second toolbar above itself.
            const toolbar = quill.container.previousElementSibling;
            if (toolbar && toolbar.classList.contains('ql-toolbar')) toolbar.remove();
            quill.root.innerHTML = '';
            editors.delete(key);
        },
    };
})();
