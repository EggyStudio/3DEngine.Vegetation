using System.Numerics;

namespace Vegetation;

/// <summary>
/// The bark's pictures and the leaves' atlas made in code, which an engine draws a tree with where
/// its species names no set of the collection.
/// </summary>
/// <remarks>
/// <para>
/// Each picture is worked out from noise or from a leaf's outline with no seed, so it is the same
/// every time it is made and an engine makes it once. The pictures are white or gray, so a
/// species' bark color and leaf color tint them as they tint a set's.
/// </para>
/// <para>
/// The pictures are RGBA, four bytes a texel, a row at a time from the top row. An engine reads
/// the colors as sRGB and the normal and the depth as linear.
/// </para>
/// </remarks>
public static class TreeLooks
{
    /// <summary>The bark's pictures' width in texels, round the stem.</summary>
    public const int BarkWidth = 128;

    /// <summary>The bark's pictures' height in texels, along the stem.</summary>
    public const int BarkHeight = 512;

    /// <summary>The leaves' atlas's width and height in texels, two leaves by two.</summary>
    public const int LeafSize = 256;

    /// <summary>
    /// Where each of the atlas's four leaves is, a square cell each, as the share of the atlas's
    /// width and height from its left and top and the share it spans, in the form
    /// <see cref="TreeTextureSet.LeafBoxes"/> gives a set's, each leaf's stalk at the bottom.
    /// </summary>
    public static IReadOnlyList<(float U, float V, float Width, float Height)> LeafBoxes { get; } =
        [(0f, 0f, 0.5f, 0.5f), (0.5f, 0f, 0.5f, 0.5f), (0f, 0.5f, 0.5f, 0.5f), (0.5f, 0.5f, 0.5f, 0.5f)];

    /// <summary>
    /// Makes the bark's color, its normal map and its depth, each tiling both ways.
    /// </summary>
    /// <returns>
    /// Three pictures <see cref="BarkWidth"/> by <see cref="BarkHeight"/>, opaque. The color is a
    /// gray of ridges running along the stem, from noise four times finer round it than along it.
    /// The normal is the one the ridges make, X toward the picture's right and Y toward its top
    /// row, each component taken from minus one to one onto 0 to 255. The depth is the gray turned
    /// over, 255 the deepest.
    /// </returns>
    public static (byte[] Color, byte[] Normal, byte[] Depth) Bark()
    {
        var height = new float[BarkWidth * BarkHeight];
        for (var y = 0; y < BarkHeight; y++)
        for (var x = 0; x < BarkWidth; x++)
        {
            // Three octaves of value noise, each finer and fainter, its lattice wrapping so the
            // picture tiles, the ridges four lattice cells round for one along.
            var sum = 0f;
            var weight = 0.55f;
            for (var octave = 0; octave < 3; octave++)
            {
                var cells = 8 << octave;
                sum += weight * Noise(x * (4f * cells) / BarkWidth, y * (float)cells / BarkHeight, 4 * cells, cells, octave);
                weight *= 0.5f;
            }

            height[x + BarkWidth * y] = sum;
        }

        var color = new byte[BarkWidth * BarkHeight * 4];
        var normal = new byte[BarkWidth * BarkHeight * 4];
        var depth = new byte[BarkWidth * BarkHeight * 4];
        for (var y = 0; y < BarkHeight; y++)
        for (var x = 0; x < BarkWidth; x++)
        {
            var at = x + BarkWidth * y;
            var gray = (byte)Math.Clamp(110f + 145f * height[at], 0f, 255f);
            color[4 * at] = color[4 * at + 1] = color[4 * at + 2] = gray;
            color[4 * at + 3] = 255;
            depth[4 * at] = depth[4 * at + 1] = depth[4 * at + 2] = (byte)(255 - gray);
            depth[4 * at + 3] = 255;

            // The slope round and along, the ridges standing out by a few texels' worth.
            var dx = height[(x + 1) % BarkWidth + BarkWidth * y] - height[(x + BarkWidth - 1) % BarkWidth + BarkWidth * y];
            var dy = height[x + BarkWidth * ((y + 1) % BarkHeight)] - height[x + BarkWidth * ((y + BarkHeight - 1) % BarkHeight)];
            var n = new Vector3(-dx * 3f, dy * 3f, 1f).Normalized;
            normal[4 * at] = (byte)Math.Clamp(127.5f + 127.5f * n.X, 0f, 255f);
            normal[4 * at + 1] = (byte)Math.Clamp(127.5f + 127.5f * n.Y, 0f, 255f);
            normal[4 * at + 2] = (byte)Math.Clamp(127.5f + 127.5f * n.Z, 0f, 255f);
            normal[4 * at + 3] = 255;
        }

        return (color, normal, depth);
    }

    /// <summary>
    /// Makes the leaves' atlas, four leaves of a shape, each a little wider than the last.
    /// </summary>
    /// <param name="shape">
    /// The leaves' outline, as <see cref="TreeSpecies.LeafShape"/> names it.
    /// </param>
    /// <param name="width">
    /// A leaf's width against its length, as <see cref="TreeSpecies.LeafScaleX"/> gives it, each
    /// leaf's held between a tenth and one.
    /// </param>
    /// <returns>
    /// The atlas <see cref="LeafSize"/> by <see cref="LeafSize"/>, the leaves in the cells
    /// <see cref="LeafBoxes"/> gives, white with a darker midrib and stalk, cut out by their alpha,
    /// every texel outside a leaf zero.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The width is not a positive number.
    /// </exception>
    public static byte[] Leaves(TreeLeafShape shape, float width)
    {
        if (!float.IsFinite(width) || width <= 0f)
            throw new ArgumentOutOfRangeException(nameof(width), width, "A leaf's width against its length is a positive number.");

        var pixels = new byte[LeafSize * LeafSize * 4];
        var cell = LeafSize / 2;
        for (var index = 0; index < 4; index++)
        {
            var (left, top) = ((index % 2) * cell, (index / 2) * cell);
            var wide = Math.Clamp(width * (0.9f + 0.07f * index), 0.1f, 1f);
            for (var y = 0; y < cell; y++)
            for (var x = 0; x < cell; x++)
            {
                // The leaf's own space, its stalk at the bottom middle and its tip at the top.
                var along = 1f - (y + 0.5f) / cell;
                var across = (x + 0.5f) / cell - 0.5f;
                var (inside, rib) = Inside(shape, along, across, wide);
                if (!inside) continue;

                var at = 4 * (left + x + LeafSize * (top + y));
                var shade = (byte)(rib ? 190 : 235 - (int)(20f * along));
                pixels[at] = pixels[at + 1] = pixels[at + 2] = shade;
                pixels[at + 3] = 255;
            }
        }

        return pixels;
    }

    // Whether a point of a leaf's space is in the leaf, and on its midrib or its stalk.
    private static (bool Inside, bool Rib) Inside(TreeLeafShape shape, float along, float across, float width)
    {
        // The stalk, the bottom tenth, a thin line up the middle.
        const float Stalk = 0.1f;
        if (along < Stalk) return (MathF.Abs(across) < 0.012f, true);
        var y = (along - Stalk) / (1f - Stalk);
        var rib = MathF.Abs(across) < 0.008f + 0.006f * (1f - y);

        var half = shape switch
        {
            TreeLeafShape.Triangle => 0.5f * width * (1f - y),
            TreeLeafShape.ThreeLobeOak => Oval(y, width) * (0.72f + 0.28f * MathF.Abs(MathF.Cos(y * 3f * MathF.PI))),
            TreeLeafShape.ThreeLobeMaple or TreeLeafShape.FiveLobeMaple => 0f,
            TreeLeafShape.ThreeLeaflets => 0f,
            _ => Oval(y, width),
        };

        if (shape is TreeLeafShape.ThreeLobeMaple or TreeLeafShape.FiveLobeMaple)
        {
            // Lobes spreading from a point a third of the way up, pointed at their tips.
            var lobes = shape == TreeLeafShape.ThreeLobeMaple ? 3 : 5;
            var dx = across / (0.5f * width);
            var dy = (y - 0.33f) / 0.67f;
            var angle = MathF.Atan2(dx, dy);
            if (MathF.Abs(angle) > MathF.PI * 0.75f) return (false, false);
            var reach = 0.45f + 0.55f * MathF.Pow(MathF.Abs(MathF.Cos(angle * lobes * 0.5f)), 3f);
            return (MathF.Sqrt(dx * dx + dy * dy) < reach, rib && dy > 0f);
        }

        if (shape == TreeLeafShape.ThreeLeaflets)
        {
            // A long leaflet up the middle and two shorter ones leaning out, each an oval.
            foreach (var (lean, length) in (ReadOnlySpan<(float, float)>)[(0f, 1f), (0.7f, 0.6f), (-0.7f, 0.6f)])
            {
                var (sin, cos) = MathF.SinCos(lean);
                var u = (y * cos + across * sin) / length;
                var v = -y * sin + across * cos;
                if (u is > 0f and < 1f && MathF.Abs(v) < Oval(u, width) * length) return (true, MathF.Abs(v) < 0.008f);
            }

            return (false, false);
        }

        return (MathF.Abs(across) < half, rib);
    }

    // An oval leaf's half width at a share of its length, widest a little below its middle.
    private static float Oval(float y, float width) => 0.5f * width * MathF.Pow(MathF.Sin(MathF.PI * MathF.Pow(y, 0.8f)), 0.9f);

    // Value noise from zero to one on a lattice wrapping at a width and a height of cells, smoothed
    // between its corners.
    private static float Noise(float x, float y, int width, int height, int layer)
    {
        var (x0, y0) = ((int)MathF.Floor(x), (int)MathF.Floor(y));
        var (fx, fy) = (x - x0, y - y0);
        var (sx, sy) = (fx * fx * (3f - 2f * fx), fy * fy * (3f - 2f * fy));
        float Corner(int cx, int cy) => Hash(((cx % width) + width) % width, ((cy % height) + height) % height, layer);
        var top = Corner(x0, y0) + (Corner(x0 + 1, y0) - Corner(x0, y0)) * sx;
        var bottom = Corner(x0, y0 + 1) + (Corner(x0 + 1, y0 + 1) - Corner(x0, y0 + 1)) * sx;
        return top + (bottom - top) * sy;
    }

    // A number from zero to one for a lattice corner, the same for the same corner every time.
    private static float Hash(int x, int y, int layer)
    {
        var bits = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ (uint)(layer * 83492791);
        bits ^= bits >> 13;
        bits *= 0x5bd1e995;
        bits ^= bits >> 15;
        return (bits & 0xFFFFFF) / 16_777_216f;
    }
}
