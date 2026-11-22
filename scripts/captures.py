#!/usr/bin/env python3
"""Draws the README's two captures, the trees and the grass, and writes them into a folder.

    scripts/captures.py <output folder>

The module's own program, scripts/ReadmeDump, grows four presets at seed 1 and scatters a patch of
grass, and this script draws what it wrote with each set's own albedo, decoded from the KTX2 files
the collection carries. It writes readme-trees.webp, 1156 by 350, and readme-grass.webp, 1200 by
420, and a file moved from there into .github/assets/readings takes the day's date in its name.

Each picture is drawn four times as wide and as tall and each pixel is the average of the sixteen
samples under it, taken in linear light, so a leaf's or a blade's cutout counts by the share of the
pixel it covers and not as all or nothing (NORM.md, N 4.12). It needs Python 3, Pillow and the
.NET SDK, and no window and no GPU, so it takes no lock. The same inputs give the same files.
"""

import io
import json
import math
import multiprocessing
import os
import struct
import subprocess
import sys
import tempfile

from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DUMP_PROJECT = os.path.join(ROOT, "scripts", "ReadmeDump", "ReadmeDump.csproj")

TREES = ["QuakingAspen", "BlackTupelo", "WeepingWillow", "CaliforniaBlackOak"]
TREE_HEIGHT = 330
GAP = 10
GRASS_SIZE = (1200, 420)

# Samples along each side of a pixel, so sixteen a pixel.
S = 4

# The 8-bit sRGB values in linear light, which the samples are averaged in.
LINEAR = [(c / 255 / 12.92 if c / 255 <= 0.04045 else ((c / 255 + 0.055) / 1.055) ** 2.4) for c in range(256)]


def encode(value):
    """A linear value as an 8-bit sRGB one."""
    value = 0.0 if value < 0 else 1.0 if value > 1 else value
    return round(255 * (12.92 * value if value <= 0.0031308 else 1.055 * value ** (1 / 2.4) - 0.055))


def reduce(img):
    """A box of S by S samples to each pixel, averaged in linear light and encoded after."""
    width, height = img.width // S, img.height // S
    bands = []
    for band in img.split():
        data = band.tobytes()
        out = []
        for y in range(height):
            acc = [0.0] * width
            for dy in range(S):
                row = [LINEAR[c] for c in data[(y * S + dy) * img.width:(y * S + dy + 1) * img.width]]
                cols = [row[dx::S] for dx in range(S)]
                acc = [a + sum(t) for a, t in zip(acc, zip(*cols))]
            out += [encode(a / (S * S)) for a in acc]
        bands.append(Image.frombytes("L", (width, height), bytes(out)))
    return Image.merge("RGB", bands)


def decode(path):
    """A KTX2 file of one BC7 level as its width, height and RGBA bytes, through Pillow's decoder
    given the level under a DDS header, which names the format as BC7 in sRGB."""
    with open(path, "rb") as file:
        data = file.read()
    _, _, width, height = struct.unpack_from("<4I", data, 12)
    offset, length, _ = struct.unpack_from("<3Q", data, 80)
    header = (b"DDS " + struct.pack("<7I", 124, 0x1 | 0x2 | 0x4 | 0x1000 | 0x80000, height, width, length, 0, 1)
              + b"\0" * 44 + struct.pack("<2I4s5I", 32, 4, b"DX10", 0, 0, 0, 0, 0)
              + struct.pack("<5I", 0x1000, 0, 0, 0, 0) + struct.pack("<5I", 99, 3, 0, 1, 0))
    image = Image.open(io.BytesIO(header + data[offset:offset + length])).convert("RGBA")
    return width, height, bytearray(image.tobytes())


def srgb(linear):
    return tuple(encode(c) for c in linear)


def lines(path):
    with open(path) as file:
        return [tuple(float(v) for v in line.split("\t")) for line in file]


class Dump:
    """What ReadmeDump wrote into a folder, with each set's albedo decoded once."""

    def __init__(self, folder):
        self.folder = folder
        with open(os.path.join(folder, "presets.json")) as file:
            self.presets = {p["name"]: p for p in json.load(file)}
        self.atlases = {}

    def atlas(self, entry):
        if not entry["found"]:
            return None
        if entry["albedo"] not in self.atlases:
            self.atlases[entry["albedo"]] = decode(os.path.join(self.folder, "sets", entry["albedo"]))
        return self.atlases[entry["albedo"]]


dump = None


def tree(name):
    """One preset seen from the side on white, its stems as quads with a stand-in bark mapping of
    one turn of bark to a stem's girth, its leaves cut by alpha, back to front by depth."""
    p = dump.presets[name]
    nodes = lines(os.path.join(dump.folder, f"{name}-nodes.tsv"))
    verts = lines(os.path.join(dump.folder, f"{name}-vertices.tsv"))
    with open(os.path.join(dump.folder, f"{name}-indices.tsv")) as file:
        idx = [int(line) for line in file]
    xs = [n[1] for n in nodes] + [v[0] for v in verts]
    ys = [n[2] for n in nodes] + [v[1] for v in verts]
    top, half = max(ys) * 1.03, max(abs(min(xs)), abs(max(xs))) * 1.05
    scale = min(TREE_HEIGHT / top, 0.95 * TREE_HEIGHT / (2 * half))
    width = max(int(2 * half * scale), 240)
    scale *= S
    width *= S
    height = TREE_HEIGHT * S
    bark, leaves = dump.atlas(p["bark"]), dump.atlas(p["leaves"])
    bark_mean = srgb(p["bark"]["mean"]) if p["bark"]["found"] else srgb(p["barkColor"])
    leaf_mean = srgb(p["leaves"]["mean"])
    img = Image.new("RGB", (width, height), "white")
    px = img.load()
    tris = []
    for parent, x, y, z, r in nodes:
        if parent < 0:
            continue
        a = nodes[int(parent)]
        dx, dy = x - a[1], y - a[2]
        length = math.hypot(dx, dy) or 1e-9
        nx, ny = -dy / length * r, dx / length * r
        v1 = math.hypot(dx, dy, z - a[3]) / (2 * math.pi * max(r, 1e-4))
        q = [(a[1] - nx, a[2] - ny, 0, 0.0), (a[1] + nx, a[2] + ny, 0.5, 0.0), (x + nx, y + ny, 0.5, v1), (x - nx, y - ny, 0, v1)]
        zm = (z + a[3]) / 2
        tris += [(zm, "bark", q[0], q[1], q[2]), (zm, "bark", q[0], q[2], q[3])]
    for t in range(0, len(idx), 3):
        a, b, c = (verts[idx[t + j]] for j in range(3))
        tris.append(((a[2] + b[2] + c[2]) / 3, "leaf", (a[0], a[1], a[3], a[4]), (b[0], b[1], b[3], b[4]), (c[0], c[1], c[3], c[4])))
    tris.sort(key=lambda t: t[0])
    for _, kind, *corners in tris:
        pts = [((cx * scale) + width / 2, height - cy * scale, u, v) for cx, cy, u, v in corners]
        if max(p_[1] for p_ in pts) < 0:
            continue
        x0, x1 = max(0, int(min(p_[0] for p_ in pts))), min(width - 1, int(max(p_[0] for p_ in pts)) + 1)
        y0, y1 = max(0, int(min(p_[1] for p_ in pts))), min(height - 1, int(max(p_[1] for p_ in pts)) + 1)
        (ax, ay, au, av), (bx, by, bu, bv), (cx, cy, cu, cv) = pts
        d = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy)
        if abs(d) < 1e-12:
            if kind == "bark" and 0 <= x0 < width and 0 <= y0 < height:
                px[x0, y0] = bark_mean
            continue
        tex = bark if kind == "bark" else leaves
        for yy in range(y0, y1 + 1):
            for xx in range(x0, x1 + 1):
                sx, sy = xx + 0.5, yy + 0.5
                l1 = ((by - cy) * (sx - cx) + (cx - bx) * (sy - cy)) / d
                l2 = ((cy - ay) * (sx - cx) + (ax - cx) * (sy - cy)) / d
                l3 = 1 - l1 - l2
                if l1 < -1e-6 or l2 < -1e-6 or l3 < -1e-6:
                    continue
                if tex is None:
                    px[xx, yy] = bark_mean if kind == "bark" else leaf_mean
                    continue
                tw, th, data = tex
                u, v = l1 * au + l2 * bu + l3 * cu, l1 * av + l2 * bv + l3 * cv
                if kind == "bark":
                    u, v = u % 1.0, v % 1.0
                o = 4 * (min(tw - 1, max(0, int(u * tw))) + tw * min(th - 1, max(0, int(v * th))))
                if kind == "leaf" and data[o + 3] <= 127:
                    continue
                px[xx, yy] = (data[o], data[o + 1], data[o + 2])
    return reduce(img)


def trees():
    with multiprocessing.get_context("fork").Pool(len(TREES)) as pool:
        images = pool.map(tree, TREES)
    sheet = Image.new("RGB", (sum(i.width for i in images) + GAP * (len(images) + 1), TREE_HEIGHT + 2 * GAP), "white")
    x = GAP
    for image in images:
        sheet.paste(image, (x, GAP))
        x += image.width + GAP
    return sheet


def grass():
    """The patch seen from the front and above, each clump two crossed cards of the set's albedo,
    far to near, on a strip of soil."""
    width, height = GRASS_SIZE
    with open(os.path.join(dump.folder, "grass.json")) as file:
        g = json.load(file)
    tw, th, data = decode(os.path.join(dump.folder, "sets", g["albedo"]))
    clumps = lines(os.path.join(dump.folder, "grass-clumps.tsv"))
    el = math.radians(28)
    aspect = g["size"][0] / g["size"][1]
    # The soil strip is this program's own, a brown block to stand the blades on, not the module's.
    soil_x, soil_z, soil_h, soil_f = 1.5, 0.6, 0.09, 0.95
    # The scale that holds the soil's front edge and the tallest blades inside the frame.
    top_y = max(h for _, _, _, h in clumps) * math.cos(el) + soil_z * math.sin(el)
    low_y = -soil_h * math.cos(el) - soil_f * math.sin(el)
    margin = 14
    scale = (height - 2 * margin) / (top_y - low_y)
    base = margin + top_y * scale
    W, H = width * S, height * S
    img = Image.new("RGB", (W, H), "white")
    px = img.load()

    def screen(X, Y, Z):
        return (S * (width / 2 + X * scale), S * (base - (Y * math.cos(el) + Z * math.sin(el)) * scale))

    d = ImageDraw.Draw(img)
    d.polygon([screen(-soil_x, 0, soil_z), screen(soil_x, 0, soil_z), screen(soil_x, 0, -soil_f), screen(-soil_x, 0, -soil_f)], fill=(112, 82, 56))
    d.polygon([screen(-soil_x, 0, -soil_f), screen(soil_x, 0, -soil_f), screen(soil_x, -soil_h, -soil_f), screen(-soil_x, -soil_h, -soil_f)], fill=(78, 55, 38))
    cards = []
    for x, z, turn, h in clumps:
        for k in range(2):
            cards.append((z, x, turn + k * math.pi / 2, h))
    cards.sort(reverse=True)
    for z, x, t, h in cards:
        w = h * aspect
        c, s_ = math.cos(t), math.sin(t)
        l = screen(x - w / 2 * c, 0, z - w / 2 * s_)
        r = screen(x + w / 2 * c, 0, z + w / 2 * s_)
        lt = screen(x - w / 2 * c, h, z - w / 2 * s_)
        rt = screen(x + w / 2 * c, h, z + w / 2 * s_)
        x0, x1 = int(min(l[0], r[0])), int(max(l[0], r[0])) + 1
        for xx in range(max(0, x0), min(W, x1 + 1)):
            f = (xx + 0.5 - l[0]) / (r[0] - l[0]) if abs(r[0] - l[0]) > 1e-6 else 0.5
            if not 0 <= f <= 1:
                continue
            by = l[1] + f * (r[1] - l[1])
            ty = lt[1] + f * (rt[1] - lt[1])
            for yy in range(max(0, int(min(by, ty))), min(H, int(max(by, ty)) + 1)):
                v = (yy + 0.5 - ty) / (by - ty) if abs(by - ty) > 1e-6 else 0.5
                if not 0 <= v <= 1:
                    continue
                o = 4 * (min(tw - 1, int(f * tw)) + tw * min(th - 1, int(v * th)))
                if data[o + 3] <= 127:
                    continue
                px[xx, yy] = (data[o], data[o + 1], data[o + 2])
    return reduce(img)


def main():
    global dump
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    with tempfile.TemporaryDirectory() as folder:
        subprocess.run(["dotnet", "run", "--project", DUMP_PROJECT, "-c", "Release", "--", folder], check=True)
        dump = Dump(folder)
        for name, image in (("readme-trees.webp", trees()), ("readme-grass.webp", grass())):
            image.save(os.path.join(out, name), quality=92, method=6)
            print(os.path.join(out, name), image.size)


if __name__ == "__main__":
    main()
