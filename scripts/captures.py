#!/usr/bin/env python3
"""Draws the README's two captures, the trees and the grass, and writes them into a folder.

    scripts/captures.py <output folder> [picture file name ...]

With no names it draws all five pictures below, and with names only those.

The module's own program, scripts/ReadmeDump, grows four presets at seed 1 and scatters a patch of
grass as blades, and this script draws what it wrote, the trees with each set's own albedo, decoded
from the KTX2 files the collection carries, and the grass on a strip of soil under a sun and a sky,
each blade cut from the photographed blades' albedo, normal and surface, tinted by its colors, lit
from the front and through from behind, in the blades' shadow on one another and on the soil. It
writes readme-trees.webp, 1156 by 350, and readme-grass.webp, 1200 by 420, and
beside them readme-grass-close.webp, a patch seen from near the ground where single blades show,
grass-pair.webp, the crossed cards and the blades at one view, and grass-sheet.webp, one patch
across the seasons, the health and the thickness. A file moved from there into
.github/assets/readings takes the day's date in its name.

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
PAIR_SIZE = (1000, 420)
GRASS_PATCH = (2.4, 0.6)
CLOSE_PATCH = (0.35, 0.25)
CLOSE_SIZE = (960, 640)

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


BC7_SRGB, BC7_LINEAR, BC5 = 99, 98, 83


def decode(path, fmt=BC7_SRGB):
    """A KTX2 file of one BC7 or BC5 level as its width, height and RGBA bytes, through Pillow's
    decoder given the level under a DDS header, which names the format as BC7 in sRGB unless given
    BC7 in linear or BC5."""
    with open(path, "rb") as file:
        data = file.read()
    _, _, width, height = struct.unpack_from("<4I", data, 12)
    offset, length, _ = struct.unpack_from("<3Q", data, 80)
    header = (b"DDS " + struct.pack("<7I", 124, 0x1 | 0x2 | 0x4 | 0x1000 | 0x80000, height, width, length, 0, 1)
              + b"\0" * 44 + struct.pack("<2I4s5I", 32, 4, b"DX10", 0, 0, 0, 0, 0)
              + struct.pack("<5I", 0x1000, 0, 0, 0, 0) + struct.pack("<5I", fmt, 3, 0, 1, 0))
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


class View:
    """A patch of grass seen from the front and above on a strip of soil, in orthographic view,
    under a sun and a sky, scaled to fill a frame of its height and width."""

    # The soil strip is this program's own, a brown block to stand the blades on, not the module's.
    SOIL_ALBEDO = tuple(LINEAR[c] for c in (128, 94, 66))
    SOIL_DEPTH = 0.09
    # Toward the sun, high, from the left and a little behind the grass, so the blades' faces and
    # their backs are lit differently, and the light is lighter than the shade it leaves.
    SUN = (-0.45, 0.8, 0.4)
    SUN_COLOR = (1.3, 1.15, 0.9)
    SKY_COLOR = (0.62, 0.70, 0.82)
    # How much of the light on a blade's far side shows through it.
    THROUGH = 1.4
    # How much brighter the through light makes each channel, yellower for less blue, and the
    # sheen's strength on a blade facing the sky.
    YELLOW = (0.25, 0.35, -0.2)
    SHEEN = 0.06
    # The height above which a blade's sky is open past the blades about it, in meters.
    ROOTS = 0.12

    def __init__(self, size, patch, elevation, tallest):
        self.width, self.height = size
        self.patch = patch
        self.el = math.radians(elevation)
        norm = math.sqrt(sum(c * c for c in self.SUN))
        self.sun = tuple(c / norm for c in self.SUN)
        self.eye = (0.0, math.sin(self.el), -math.cos(self.el))
        # The soil reaches only a twentieth of the patch's width and a fifth of its depth past its
        # edges.
        self.soil_x = patch[0] * 0.55
        self.soil_z = patch[1] / 2 + patch[1] * 0.05
        self.soil_f = patch[1] / 2 + patch[1] * 0.2
        top_y = tallest * math.cos(self.el) + self.soil_z * math.sin(self.el)
        low_y = -self.SOIL_DEPTH * math.cos(self.el) - self.soil_f * math.sin(self.el)
        margin = 14
        self.scale = min((self.height - 2 * margin) / (top_y - low_y), (self.width - 2 * margin) / (2 * self.soil_x))
        self.base = margin + top_y * self.scale
        self.image = Image.new("RGB", (self.width * S, self.height * S), "white")
        self.soil()

    def screen(self, X, Y, Z):
        """A place of the scene as a sample of the image, its Z away from the eye rising up."""
        return (S * (self.width / 2 + X * self.scale),
                S * (self.base - (Y * math.cos(self.el) + Z * math.sin(self.el)) * self.scale))

    def depth(self, Y, Z):
        """How far from the eye along its way, larger the farther."""
        return Z * math.cos(self.el) - Y * math.sin(self.el)

    def lit(self, albedo, normal):
        """A color in linear light of an albedo under the sun and the sky for a normal, turned to
        face the eye, a part of the sun's light showing through from the far side."""
        n = normal
        if sum(a * b for a, b in zip(n, self.eye)) < 0:
            n = (-n[0], -n[1], -n[2])
        facing = sum(a * b for a, b in zip(n, self.sun))
        sun = max(0.0, facing) + self.THROUGH * max(0.0, -facing)
        sky = 0.5 + 0.5 * n[1]
        return tuple(a * (sun * c + sky * k) for a, c, k in zip(albedo, self.SUN_COLOR, self.SKY_COLOR))

    def soil(self):
        top = srgb(self.lit(self.SOIL_ALBEDO, (0, 1, 0)))
        front = srgb(self.lit(self.SOIL_ALBEDO, (0, 0, -1)))
        draw = ImageDraw.Draw(self.image)
        s = self.screen
        draw.polygon([s(-self.soil_x, 0, self.soil_z), s(self.soil_x, 0, self.soil_z),
                      s(self.soil_x, 0, -self.soil_f), s(-self.soil_x, 0, -self.soil_f)], fill=top)
        draw.polygon([s(-self.soil_x, 0, -self.soil_f), s(self.soil_x, 0, -self.soil_f),
                      s(self.soil_x, -self.SOIL_DEPTH, -self.soil_f),
                      s(-self.soil_x, -self.SOIL_DEPTH, -self.soil_f)], fill=front)

    def cards(self, name, tex):
        """Each clump as two crossed cards of the set's albedo, far to near, lit by their flat
        faces."""
        tw, th, data = tex
        with open(os.path.join(dump.folder, "grass.json")) as file:
            aspect = json.load(file)["size"]
        aspect = aspect[0] / aspect[1]
        W, H = self.width * S, self.height * S
        px = self.image.load()
        cards = []
        for x, z, turn, h in lines(os.path.join(dump.folder, f"{name}-clumps.tsv")):
            for k in range(2):
                cards.append((z, x, turn + k * math.pi / 2, h))
        cards.sort(reverse=True)
        for z, x, t, h in cards:
            w = h * aspect
            c, s_ = math.cos(t), math.sin(t)
            light = self.lit((1.0, 1.0, 1.0), (-s_, 0.0, c))
            l = self.screen(x - w / 2 * c, 0, z - w / 2 * s_)
            r = self.screen(x + w / 2 * c, 0, z + w / 2 * s_)
            lt = self.screen(x - w / 2 * c, h, z - w / 2 * s_)
            rt = self.screen(x + w / 2 * c, h, z + w / 2 * s_)
            for xx in range(max(0, int(min(l[0], r[0]))), min(W, int(max(l[0], r[0])) + 2)):
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
                    px[xx, yy] = tuple(encode(LINEAR[data[o + k]] * light[k]) for k in range(3))

    def blades(self, name):
        """The blades' triangles, far to near by their middles, each sample lit by the photographed
        blade's albedo, normal and surface at its coordinates, tinted, in the sun's shadow of the
        blades and in the sky's light dimmed at the roots, drawn a band of rows to a process."""
        global JOB
        rows = lines(os.path.join(dump.folder, f"{name}-mesh.tsv"))
        with open(os.path.join(dump.folder, f"{name}-mesh-indices.tsv")) as file:
            idx = [int(line) for line in file]
        with open(os.path.join(dump.folder, "grass.json")) as file:
            blade = json.load(file)["blade"]
        sets = os.path.join(dump.folder, "sets")
        textures = (decode(os.path.join(sets, blade["albedo"])),
                    decode(os.path.join(sets, blade["normal"]), BC5),
                    decode(os.path.join(sets, blade["surface"]), BC7_LINEAR))
        tris = []
        for t in range(0, len(idx), 3):
            corners = []
            for j in range(3):
                x, y, z, nx, ny, nz, cr, cg, cb, u, v = rows[idx[t + j]]
                sx, sy = self.screen(x, y, z)
                corners.append((sx, sy, x, y, z, nx, ny, nz, u, v, cr, cg, cb, self.depth(y, z)))
            tris.append((-sum(c[13] for c in corners) / 3, corners, tangents(corners)))
        tris.sort(key=lambda t: t[0])
        shadow = Shadow(self.sun, [t[1] for t in tris])
        JOB = (self, [(t[1], t[2]) for t in tris], textures, shadow)
        height = self.height * S
        step = height // (4 * os.cpu_count()) + 1
        bands = [(y, min(height, y + step)) for y in range(0, height, step)]
        with multiprocessing.get_context("fork").Pool() as pool:
            parts = pool.map(blade_band, bands)
        for (y0, y1), part in zip(bands, parts):
            self.image.paste(Image.frombytes("RGB", (self.width * S, y1 - y0), part), (0, y0))

    def shade(self, albedo, normal, tangent, bitangent, tex, y, shadow, position):
        """A sample's color in linear light, from its albedo, the vertex normal bent by the normal
        map of its texel, the share of its light that passes through and its occlusion."""
        nw, nh, normals = tex[1]
        sw, sh, surface = tex[2]
        u, v = position[3], position[4]
        o = 4 * (min(nw - 1, int(u * nw)) + nw * min(nh - 1, int(v * nh)))
        mx, my = normals[o] / 127.5 - 1, normals[o + 1] / 127.5 - 1
        mz = math.sqrt(max(0.0, 1 - mx * mx - my * my))
        n = tuple(tangent[k] * mx + bitangent[k] * my + normal[k] * mz for k in range(3))
        length = math.sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]) or 1.0
        n = (n[0] / length, n[1] / length, n[2] / length)
        if sum(a * b for a, b in zip(n, self.eye)) < 0:
            n = (-n[0], -n[1], -n[2])
        o = 4 * (min(sw - 1, int(u * sw)) + sw * min(sh - 1, int(v * sh)))
        occlusion, through = surface[o] / 255, surface[o + 3] / 255
        facing = sum(a * b for a, b in zip(n, self.sun))
        lit = 0.0 if shadow.covers(position[:3]) else 1.0
        # Light through a blade is lighter and yellower than the light on it, and a blade that
        # faces the sky takes a soft sheen of it.
        passing = lit * self.THROUGH * through * max(0.0, -facing)
        sun = lit * max(0.0, facing) + passing
        albedo = tuple(a * (1.0 + self.YELLOW[k] * passing) for k, a in enumerate(albedo))
        sheen = self.SHEEN * max(0.0, n[1]) ** 4 * (lit * max(0.0, self.sun[1]) + 0.3)
        # Close to the ground the sky is hidden by the blades round, more so the lower it is.
        roots = 0.12 + 0.88 * min(1.0, y / self.ROOTS) ** 0.8
        sky = (0.5 + 0.5 * n[1]) * occlusion * roots
        return tuple(a * (sun * c + sky * k) + sheen * c * 0.5
                     for a, c, k in zip(albedo, self.SUN_COLOR, self.SKY_COLOR))

    def soil_band(self, pix, y0, y1, shadow):
        """The soil's top face over the rows y0 up to y1, in the shadow of the blades and under the
        canopy's dimmer sky, its edge washing out to the open sky within a tenth of the patch's
        width."""
        W = self.width * S
        sin_el = math.sin(self.el)
        facing = max(0.0, self.sun[1])
        edge = 0.05 + 0.05 * self.patch[0]
        for yy in range(y0, y1):
            z = (self.base - (yy + 0.5) / S) / (sin_el * self.scale)
            if not -self.soil_f <= z <= self.soil_z:
                continue
            for xx in range(0, W):
                x = ((xx + 0.5) / S - self.width / 2) / self.scale
                if not -self.soil_x <= x <= self.soil_x:
                    continue
                lit = 0.0 if shadow.covers((x, 0.0, z)) else 1.0
                inside = max(abs(x) - self.patch[0] / 2, abs(z) - self.patch[1] / 2)
                sky = 1.0 if inside > edge else 0.3 + 0.7 * max(0.0, inside) / edge
                color = tuple(a * (lit * facing * c + sky * k)
                              for a, c, k in zip(self.SOIL_ALBEDO, self.SUN_COLOR, self.SKY_COLOR))
                pix[xx, yy - y0] = (encode(color[0]), encode(color[1]), encode(color[2]))

    def finish(self):
        return reduce(self.image)


JOB = None


def tangents(corners):
    """The unit directions along a triangle's width and up its length, from its places and its
    coordinates, a blade's picture standing up its box, so up the length is against v."""
    p0, p1, p2 = (c[2:5] for c in corners)
    u0, v0, u1, v1, u2, v2 = (corners[i][j] for i in range(3) for j in (8, 9))
    e1 = [p1[k] - p0[k] for k in range(3)]
    e2 = [p2[k] - p0[k] for k in range(3)]
    du1, dv1, du2, dv2 = u1 - u0, v1 - v0, u2 - u0, v2 - v0
    det = du1 * dv2 - du2 * dv1
    if abs(det) < 1e-12:
        return (1.0, 0.0, 0.0), (0.0, 1.0, 0.0)
    across = [(e1[k] * dv2 - e2[k] * dv1) / det for k in range(3)]
    down = [(e2[k] * du1 - e1[k] * du2) / det for k in range(3)]
    return unit(across), unit([-c for c in down])


def unit(v):
    length = math.sqrt(sum(c * c for c in v)) or 1.0
    return tuple(c / length for c in v)


class Shadow:
    """The blades' shadow as the sun makes it, a depth map seen from the sun in cells of a
    millimeter and a half, a point shadowed when something stands nearer the sun over its cell."""

    CELL = 0.0015
    BIAS = 0.004

    def __init__(self, sun, tris):
        self.sun = sun
        up = (0.0, 1.0, 0.0)
        self.e1 = unit((up[1] * sun[2] - up[2] * sun[1], up[2] * sun[0] - up[0] * sun[2],
                        up[0] * sun[1] - up[1] * sun[0]))
        self.e2 = (sun[1] * self.e1[2] - sun[2] * self.e1[1], sun[2] * self.e1[0] - sun[0] * self.e1[2],
                   sun[0] * self.e1[1] - sun[1] * self.e1[0])
        placed = [[self.place(c[2:5]) for c in tri] for tri in tris]
        points = [q for tri in placed for q in tri]
        self.a0 = min(q[0] for q in points) - 0.01
        self.b0 = min(q[1] for q in points) - 0.01
        self.na = int((max(q[0] for q in points) + 0.01 - self.a0) / self.CELL) + 1
        self.nb = int((max(q[1] for q in points) + 0.01 - self.b0) / self.CELL) + 1
        self.depth = [-1e9] * (self.na * self.nb)
        for tri in placed:
            self.raster(tri)

    def place(self, p):
        """A place as its two coordinates across the sun's view and its nearness to the sun."""
        return (sum(a * b for a, b in zip(p, self.e1)), sum(a * b for a, b in zip(p, self.e2)),
                sum(a * b for a, b in zip(p, self.sun)))

    def raster(self, tri):
        (ax, ay, az), (bx, by, bz), (cx, cy, cz) = tri
        d = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy)
        if abs(d) < 1e-12:
            return
        c = self.CELL
        ia, ib = max(0, int((min(ax, bx, cx) - self.a0) / c) - 1), min(self.na - 1, int((max(ax, bx, cx) - self.a0) / c) + 1)
        ja, jb = max(0, int((min(ay, by, cy) - self.b0) / c) - 1), min(self.nb - 1, int((max(ay, by, cy) - self.b0) / c) + 1)
        depth = self.depth
        for j in range(ja, jb + 1):
            y = self.b0 + (j + 0.5) * c
            for i in range(ia, ib + 1):
                x = self.a0 + (i + 0.5) * c
                l1 = ((by - cy) * (x - cx) + (cx - bx) * (y - cy)) / d
                l2 = ((cy - ay) * (x - cx) + (ax - cx) * (y - cy)) / d
                l3 = 1 - l1 - l2
                if l1 < -0.3 or l2 < -0.3 or l3 < -0.3:
                    continue
                z = l1 * az + l2 * bz + l3 * cz
                if z > depth[i + j * self.na]:
                    depth[i + j * self.na] = z

    def covers(self, p):
        """Whether something stands between a place and the sun."""
        x, y, z = self.place(p)
        i, j = int((x - self.a0) / self.CELL), int((y - self.b0) / self.CELL)
        if not (0 <= i < self.na and 0 <= j < self.nb):
            return False
        return self.depth[i + j * self.na] > z + self.BIAS


def blade_band(band):
    """The rows y0 up to y1 of the view with the soil and every blade's triangle drawn into them."""
    view, tris, textures, shadow = JOB
    y0, y1 = band
    W = view.width * S
    px = view.image.crop((0, y0, W, y1))
    pix = px.load()
    view.soil_band(pix, y0, y1, shadow)
    aw, ah, albedo_data = textures[0]
    for corners, (tangent, bitangent) in tris:
        (ax, ay, *pa), (bx, by, *pb), (cx, cy, *pc) = corners
        if max(ay, by, cy) < y0 or min(ay, by, cy) >= y1:
            continue
        d = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy)
        if abs(d) < 1e-9:
            continue
        xa, xb = max(0, int(min(ax, bx, cx))), min(W - 1, int(max(ax, bx, cx)) + 1)
        ya, yb = max(y0, int(min(ay, by, cy))), min(y1 - 1, int(max(ay, by, cy)) + 1)
        for yy in range(ya, yb + 1):
            sy = yy + 0.5
            for xx in range(xa, xb + 1):
                sx = xx + 0.5
                l1 = ((by - cy) * (sx - cx) + (cx - bx) * (sy - cy)) / d
                l2 = ((cy - ay) * (sx - cx) + (ax - cx) * (sy - cy)) / d
                l3 = 1 - l1 - l2
                if l1 < -1e-6 or l2 < -1e-6 or l3 < -1e-6:
                    continue
                # pa holds x y z, nx ny nz, u v, r g b and depth for each corner.
                mix = [l1 * pa[k] + l2 * pb[k] + l3 * pc[k] for k in range(12)]
                u, v = mix[6], mix[7]
                o = 4 * (min(aw - 1, int(u * aw)) + aw * min(ah - 1, int(v * ah)))
                if albedo_data[o + 3] <= 127:
                    continue
                albedo = tuple(LINEAR[albedo_data[o + k]] * mix[8 + k] for k in range(3))
                color = view.shade(albedo, unit(mix[3:6]), tangent, bitangent, textures, mix[1], shadow, mix[:3] + [u, v])
                pix[xx, yy - y0] = (encode(color[0]), encode(color[1]), encode(color[2]))
    return px.tobytes()


def patch_height(name):
    return max(h for _, _, _, h in lines(os.path.join(dump.folder, f"{name}-clumps.tsv")))


def grass_view(name, size, patch, elevation, near, room=1.0):
    """A patch drawn as its near level, the blades, or as its far level, the crossed cards, with
    room times its tallest clump's height above the soil, for cards that stand wider than tall."""
    view = View(size, patch, elevation, patch_height(name) * room)
    if near:
        view.blades(name)
    else:
        with open(os.path.join(dump.folder, "grass.json")) as file:
            albedo = json.load(file)["albedo"]
        view.cards(name, decode(os.path.join(dump.folder, "sets", albedo)))
    return view.finish()


def grass():
    """The README's patch of lawn grass as blades."""
    return grass_view("grass", GRASS_SIZE, GRASS_PATCH, 28, True)


def close():
    """A smaller patch from near the ground, where single blades show."""
    return grass_view("close", CLOSE_SIZE, CLOSE_PATCH, 9, True)


def pair():
    """The patch of the README as its far level's cards beside its near level's blades, from one
    view, for the dump."""
    cards, blades = (grass_view("grass", PAIR_SIZE, GRASS_PATCH, 28, near, 1.5) for near in (False, True))
    sheet = Image.new("RGB", (2 * PAIR_SIZE[0] + GAP, PAIR_SIZE[1]), "white")
    sheet.paste(cards, (0, 0))
    sheet.paste(blades, (PAIR_SIZE[0] + GAP, 0))
    return sheet


def sheet():
    """The one patch across the seasons, the health and the thickness, a row each, a tile to a
    variant with its label, for the dump."""
    with open(os.path.join(dump.folder, "sheet.json")) as file:
        variants = json.load(file)
    tile = (400, 300)
    patch = (0.5, 0.35)
    tallest = max(patch_height(v["name"]) for v in variants)
    rows = [[v for v in variants if v["label"].startswith(kind)] for kind in ("season", "health", "thickness")]
    columns = max(len(r) for r in rows)
    out = Image.new("RGB", (columns * tile[0] + GAP * (columns + 1), len(rows) * (tile[1] + 20) + GAP), "white")
    draw = ImageDraw.Draw(out)
    for r, row in enumerate(rows):
        for c, v in enumerate(row):
            view = View(tile, patch, 20, tallest)
            view.blades(v["name"])
            x, y = GAP + c * (tile[0] + GAP), GAP + r * (tile[1] + 20)
            out.paste(view.finish(), (x, y + 20))
            draw.text((x, y + 4), v["label"], fill="black")
    return out


def main():
    global dump
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    out = sys.argv[1]
    only = sys.argv[2:]
    os.makedirs(out, exist_ok=True)
    with tempfile.TemporaryDirectory() as folder:
        subprocess.run(["dotnet", "run", "--project", DUMP_PROJECT, "-c", "Release", "--", folder], check=True)
        dump = Dump(folder)
        pictures = (("readme-trees.webp", trees), ("readme-grass.webp", grass),
                    ("readme-grass-close.webp", close), ("grass-pair.webp", pair),
                    ("grass-sheet.webp", sheet))
        for name, draw in pictures:
            if only and name not in only:
                continue
            image = draw()
            image.save(os.path.join(out, name), quality=92, method=6)
            print(os.path.join(out, name), image.size)


if __name__ == "__main__":
    main()
