"""Reconstruct two-column reading order from `pdftotext -layout` output.

The Creature Law PDF is laid out in two columns. -layout preserves horizontal
position, so a single physical line can contain a fragment of the LEFT column
and a fragment of the RIGHT column separated by a wide gutter. We split each
line into whitespace-separated segments, classify each segment as left/right by
its start column, then emit the whole left column followed by the whole right
column (correct reading order for a 2-col page).
"""
import re
import subprocess
import sys

PDF = "docs/rules/Rolemaster_Creature_Law_I.pdf"
SPLITX = 50            # columns starting >= this belong to the right column
GAP = re.compile(r"\s{3,}")   # a run of 3+ spaces separates columns/segments


def page_text(first, last):
    out = subprocess.run(
        ["pdftotext", "-layout", "-f", str(first), "-l", str(last), PDF, "-"],
        capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
    return out.split("\f")


def reconstruct(page):
    left, right = [], []
    for raw in page.split("\n"):
        line = raw.rstrip("\r")
        if not line.strip():
            left.append("")
            right.append("")
            continue
        # segments = (start_col, text)
        segs, pos = [], 0
        for part in GAP.split(line):
            if part == "":
                # leading gap; advance pos to the next non-space
                pass
            idx = line.find(part, pos)
            if part.strip():
                segs.append((idx, part))
            pos = idx + len(part)
        lparts = [t for (c, t) in segs if c < SPLITX]
        rparts = [t for (c, t) in segs if c >= SPLITX]
        left.append(" ".join(lparts))
        right.append(" ".join(rparts))
    return "\n".join(left), "\n".join(right)


if __name__ == "__main__":
    f, l = int(sys.argv[1]), int(sys.argv[2])
    for p in page_text(f, l):
        lt, rt = reconstruct(p)
        print("===== LEFT COLUMN =====")
        print(lt.strip())
        print("===== RIGHT COLUMN =====")
        print(rt.strip())
        print("########## PAGE BREAK ##########")
