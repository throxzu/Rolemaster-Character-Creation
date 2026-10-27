// Town map editor: reshaping buildings, walls and roads on an imported Watabou map.
//
// Modelled on fogPaint in app.js — the drag happens here at full frame rate and .NET is told
// once, when the gesture ends, so a 300 KB document is never shuttled across the circuit
// mid-drag. Kept out of app.js because it is a page's worth of behaviour on its own.
window.townEdit = (function () {
    const EDITABLE = 'polygon[data-layer], polyline[data-layer]';
    const SVGNS = 'http://www.w3.org/2000/svg';
    const state = new WeakMap();   // frame element -> session

    // A click is a map coordinate only once both the viewBox mapping and the CSS transform
    // townMap uses for pan and zoom have been undone. getScreenCTM accounts for both.
    function toMap(svg, clientX, clientY) {
        const pt = svg.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;
        const ctm = svg.getScreenCTM();
        if (!ctm) return { x: 0, y: 0 };
        const p = pt.matrixTransform(ctm.inverse());
        return { x: p.x, y: p.y };
    }

    // Two decimals, matching the "0.##" the server renders points with. Rounding harder here
    // would be invisible on a map 2000 units across; rounding softer makes the shape shift by
    // a hair the next time Blazor re-renders it, because the two sides disagree.
    const round = (v) => Math.round(v * 100) / 100;

    const readPoints = (el) =>
        (el.getAttribute('points') || '').trim().split(/\s+/).filter(Boolean)
            .map(pair => pair.split(',').map(Number));

    const writePoints = (el, pts) =>
        el.setAttribute('points', pts.map(p => round(p[0]) + ',' + round(p[1])).join(' '));

    function clearHandles(s) {
        s.handleLayer.replaceChildren();
        s.handles = [];
    }

    function drawHandles(s) {
        clearHandles(s);
        if (!s.selected) return;
        // Handles are sized from the viewBox so they stay usable at any zoom.
        const r = Math.max(1.2, s.svg.viewBox.baseVal.width / 400);
        readPoints(s.selected).forEach((p, i) => {
            const c = document.createElementNS(SVGNS, 'circle');
            c.setAttribute('cx', p[0]);
            c.setAttribute('cy', p[1]);
            c.setAttribute('r', r);
            c.setAttribute('class', 'te-handle');
            c.dataset.i = i;
            s.handleLayer.appendChild(c);
            s.handles.push(c);
        });
    }

    function select(s, el) {
        if (s.selected === el) return;
        if (s.selected) s.selected.classList.remove('te-selected');
        s.selected = el;
        if (el) el.classList.add('te-selected');
        drawHandles(s);
    }

    // Median footprint of the buildings already on the map, so a dropped block looks like it
    // belongs rather than arriving at some arbitrary size.
    function typicalSize(svg) {
        const sizes = [];
        svg.querySelectorAll('polygon[data-layer="buildings"]').forEach(el => {
            const b = el.getBBox();
            if (b.width > 0) sizes.push(Math.max(b.width, b.height));
        });
        if (sizes.length === 0) return 12;
        sizes.sort((a, b) => a - b);
        return sizes[Math.floor(sizes.length / 2)];
    }

    function distToSegment(p, a, b) {
        const vx = b[0] - a[0], vy = b[1] - a[1];
        const wx = p.x - a[0], wy = p.y - a[1];
        const len = vx * vx + vy * vy;
        const t = len === 0 ? 0 : Math.max(0, Math.min(1, (wx * vx + wy * vy) / len));
        return Math.hypot(p.x - (a[0] + t * vx), p.y - (a[1] + t * vy));
    }

    async function commit(s, el) {
        const ok = await s.dotnet.invokeMethodAsync(
            'EditSetPoints', el.dataset.layer, +el.dataset.id, readPoints(el));
        if (!ok) console.warn('townEdit: server refused the change to', el.dataset.layer, el.dataset.id);
    }

    function attach(frame, dotnet) {
        if (!frame || state.has(frame)) return;
        const svg = frame.querySelector('svg');
        if (!svg) return;

        // Handles live in their own group appended after everything Blazor renders, so a
        // re-render of the map never fights with them.
        const handleLayer = document.createElementNS(SVGNS, 'g');
        handleLayer.setAttribute('class', 'te-handles');
        svg.appendChild(handleLayer);

        const s = {
            frame, svg, dotnet, handleLayer,
            handles: [], selected: null, drag: null, pendingDrop: null, hoverVertex: null,
        };
        state.set(frame, s);

        s.onDown = (e) => {
            if (e.button !== 0) return;

            const handle = e.target.closest('.te-handle');
            if (handle) {
                e.preventDefault();
                e.stopPropagation();
                s.drag = { kind: 'vertex', index: +handle.dataset.i };
                return;
            }

            const shape = e.target.closest(EDITABLE);
            if (shape) {
                // stopPropagation keeps the map from panning out from under the gesture — the
                // same guard fogPaint.onDown uses. Deliberately NOT preventDefault: calling it
                // on pointerdown stops the browser ever synthesising the dblclick that adds a
                // corner. Text selection during a drag is handled with user-select instead.
                e.stopPropagation();
                select(s, shape);
                const m = toMap(svg, e.clientX, e.clientY);
                s.drag = { kind: 'shape', lastX: m.x, lastY: m.y };
                return;
            }

            // Empty ground: remember where, and drop a building there if nothing else happens.
            if (e.target === svg
                || e.target.classList.contains('town-earth')
                || e.target.classList.contains('town-district')) {
                e.preventDefault();
                e.stopPropagation();
                select(s, null);
                s.pendingDrop = toMap(svg, e.clientX, e.clientY);
            }
        };

        s.onMove = (e) => {
            if (!s.drag || !s.selected) return;
            const m = toMap(svg, e.clientX, e.clientY);
            const pts = readPoints(s.selected);

            if (s.drag.kind === 'vertex') {
                pts[s.drag.index] = [m.x, m.y];
                writePoints(s.selected, pts);
                const h = s.handles[s.drag.index];
                if (h) { h.setAttribute('cx', m.x); h.setAttribute('cy', m.y); }
            } else {
                const dx = m.x - s.drag.lastX, dy = m.y - s.drag.lastY;
                if (dx === 0 && dy === 0) return;
                s.drag.lastX = m.x;
                s.drag.lastY = m.y;
                writePoints(s.selected, pts.map(p => [p[0] + dx, p[1] + dy]));
                s.handles.forEach(h => {
                    h.setAttribute('cx', +h.getAttribute('cx') + dx);
                    h.setAttribute('cy', +h.getAttribute('cy') + dy);
                });
            }
            s.drag.dirty = true;
        };

        s.onUp = async () => {
            if (s.pendingDrop) {
                const at = s.pendingDrop;
                s.pendingDrop = null;
                const half = typicalSize(svg) / 2;
                const id = await s.dotnet.invokeMethodAsync('EditCreateBuilding', [
                    [at.x - half, at.y - half], [at.x + half, at.y - half],
                    [at.x + half, at.y + half], [at.x - half, at.y + half],
                ]);
                if (id >= 0) {
                    // Blazor renders the new polygon; select it once it has landed.
                    setTimeout(() => {
                        const el = svg.querySelector(
                            'polygon[data-layer="buildings"][data-id="' + id + '"]');
                        if (el) select(s, el);
                    }, 80);
                }
                return;
            }

            if (!s.drag) return;
            const dirty = s.drag.dirty;
            const el = s.selected;
            s.drag = null;
            if (dirty && el) await commit(s, el);
        };

        // Double-clicking an edge of the selected shape adds a corner there.
        s.onDblClick = async (e) => {
            const shape = e.target.closest(EDITABLE);
            if (!shape || shape !== s.selected) return;
            e.preventDefault();
            e.stopPropagation();

            const m = toMap(svg, e.clientX, e.clientY);
            const pts = readPoints(shape);
            const closed = shape.tagName.toLowerCase() === 'polygon';
            const segments = closed ? pts.length : pts.length - 1;

            let best = -1, bestDist = Infinity;
            for (let i = 0; i < segments; i++) {
                const d = distToSegment(m, pts[i], pts[(i + 1) % pts.length]);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            if (best < 0) return;

            pts.splice(best + 1, 0, [m.x, m.y]);
            writePoints(shape, pts);
            drawHandles(s);
            await commit(s, shape);
        };

        s.onOver = (e) => {
            const h = e.target.closest('.te-handle');
            s.hoverVertex = h ? +h.dataset.i : null;
        };

        s.onKey = async (e) => {
            if (!s.selected) return;
            if (e.key !== 'Delete' && e.key !== 'Backspace') return;
            const tag = (e.target.tagName || '').toLowerCase();
            if (tag === 'input' || tag === 'textarea' || e.target.isContentEditable) return;
            e.preventDefault();

            const el = s.selected;
            const pts = readPoints(el);
            const min = el.tagName.toLowerCase() === 'polygon' ? 3 : 2;

            // With a corner under the pointer, Delete removes that corner; otherwise it
            // removes the whole shape. A shape is never reduced below a drawable number
            // of corners.
            if (s.hoverVertex !== null && pts.length > min) {
                pts.splice(s.hoverVertex, 1);
                writePoints(el, pts);
                s.hoverVertex = null;
                drawHandles(s);
                await commit(s, el);
                return;
            }

            const ok = await s.dotnet.invokeMethodAsync('EditDelete', el.dataset.layer, +el.dataset.id);
            if (ok) select(s, null);
        };

        frame.addEventListener('pointerdown', s.onDown, true);
        frame.addEventListener('dblclick', s.onDblClick, true);
        frame.addEventListener('pointerover', s.onOver);
        window.addEventListener('pointermove', s.onMove);
        window.addEventListener('pointerup', s.onUp);
        window.addEventListener('keydown', s.onKey);
    }

    function detach(frame) {
        const s = state.get(frame);
        if (!s) return;
        select(s, null);
        s.handleLayer.remove();
        frame.removeEventListener('pointerdown', s.onDown, true);
        frame.removeEventListener('dblclick', s.onDblClick, true);
        frame.removeEventListener('pointerover', s.onOver);
        window.removeEventListener('pointermove', s.onMove);
        window.removeEventListener('pointerup', s.onUp);
        window.removeEventListener('keydown', s.onKey);
        state.delete(frame);
    }

    function clearSelection(frame) {
        const s = state.get(frame);
        if (s) { select(s, null); drawHandles(s); }
    }

    return { attach, detach, clearSelection };
})();
