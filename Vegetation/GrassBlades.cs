using System.Numerics;

namespace Vegetation;

/// <summary>
/// The blades of a clump of grass and of a patch, drawn from a seed, and the patch's blades as
/// vertex arrays that any engine draws.
/// </summary>
/// <remarks>
/// <para>
/// A clump's blades are drawn from <see cref="Pcg"/> seeded by the clump's own two phases, which
/// <see cref="GrassScatter.Clumps"/> draws as whole multiples of a 24-bit step, so the same clump
/// and the same settings give the same blades on every runtime and the scatter's own draws, its
/// seeds and its order, are left as they were. A clump has five to nine blades, and the draws of
/// each, in this order, are where its root falls in the clump's foot, how it faces, whether it is
/// low and how tall it is, how wide at its root, how it leans and curls, how many segments it has,
/// which look it has, whether it is dry and how its colors stray.
/// </para>
/// <para>
/// A blade faces outward from the clump's middle within a sixth of a turn either way, so a clump
/// opens as a fountain, and every blade is shorter than its clump's height and bends no further
/// than 77 degrees from the up axis, and <see cref="Reach"/> counts on that.
/// </para>
/// <para>
/// Two in five of the blades after the first are low, a third to a half of the clump's height
/// where the others stand from 0.6 of it up, so the ground under a patch is hidden by a body of
/// short blades with the tall blades' tips standing above it.
/// </para>
/// <para>
/// A blade is a meadow grass of three to eight millimeters at its root, cut from one of three
/// blades of the photographed set <see cref="Set"/>, whose box in the atlas is its look, so its
/// color, its midrib and veins and the light it lets through are the photograph's. A share of the
/// settings, <see cref="GrassSettings.DryShare"/>, are dry and yellowed, which are the same
/// pictures tinted to <see cref="GrassSettings.DryColor"/>. The tints are the color wanted over
/// the mean color of the look's picture, so the settings' colors tint the photograph and do not
/// replace it.
/// </para>
/// </remarks>
public static class GrassBlades
{
    // The mean color in linear sRGB of each look's picture, which a tint divides the color wanted
    // by, as the tools' reading of the set's albedo over the box gives it.
    private static readonly Vector3[] LookMeans =
    [
        new(0.1568f, 0.2412f, 0.0378f),
        new(0.1813f, 0.2687f, 0.0388f),
        new(0.1914f, 0.2761f, 0.0362f),
    ];

    // The fewest and the most blades of a clump, and its foot's radius as a share of its height.
    private const int Fewest = 5;
    private const int Spread = 5;
    private const float Foot = 0.15f;

    // The share of a clump's blades after its first that are low, a third to a half of its height,
    // so a patch has a body near the ground and tips above it. The first is always tall.
    private const float LowShare = 0.4f;

    // How far a blade's edge normals are bent from its middle's across the width, 0.6 radians, as
    // a cosine and a sine written as numbers so they are the same everywhere.
    private const float RoundCos = 0.8253356f;
    private const float RoundSin = 0.5646425f;

    /// <summary>
    /// The set of the texture collection the blades are cut from, one of the kind
    /// <see cref="TreeTextureKind.Grass"/> that holds whole blades across its width.
    /// </summary>
    public const string Set = "wheat-grass";

    /// <summary>
    /// The boxes of the three blades the set's atlas is cut into, each as a left and a top and a
    /// width and a height in shares of the atlas from its top left, the blade standing in its box
    /// from its root at the bottom to its point at the top, which <see cref="GrassBlade.Look"/>
    /// indexes.
    /// </summary>
    public static IReadOnlyList<(float U, float V, float Width, float Height)> Looks { get; } =
    [
        (0.3046875f, 0.2265625f, 0.01171875f, 0.328125f),
        (0.86328125f, 0.201171875f, 0.01171875f, 0.31640625f),
        (0.2578125f, 0.26953125f, 0.01171875f, 0.296875f),
    ];

    /// <summary>
    /// How far from a clump's root any part of any of its blades reaches along the ground, in
    /// meters.
    /// </summary>
    /// <param name="clump">The clump.</param>
    /// <returns>
    /// The radius of the disc about its root that holds every blade's foot and every blade's whole
    /// curve, and its height, in <see cref="GrassClump.Height"/>, holds the blades' tips.
    /// </returns>
    /// <remarks>
    /// A blade's curve stays within the polygon of its control points, whose far corner is no more
    /// than three quarters of its height to the side, and the blade is less than a centimeter
    /// wide, so under half of that either side of the curve.
    /// </remarks>
    public static float Reach(GrassClump clump) => clump.Height * (Foot + 0.75f) + 0.01f;

    /// <summary>Draws a clump's blades from the clump and the settings.</summary>
    /// <param name="clump">The clump, as <see cref="GrassScatter.Clumps"/> gives it.</param>
    /// <param name="settings">
    /// The colors the blades are tinted to, how many are dry and how far the colors stray.
    /// </param>
    /// <returns>
    /// Five to nine blades, their roots from the patch's middle, the same for the same clump and
    /// settings.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The settings' color variation or dry share is not usable.
    /// </exception>
    public static IReadOnlyList<GrassBlade> Of(GrassClump clump, GrassSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (!(settings.ColorVariation >= 0f && settings.ColorVariation <= 1f))
            throw new ArgumentException("Blades stray in color from nothing up to the whole.", nameof(settings));
        if (!(settings.DryShare >= 0f && settings.DryShare <= 1f))
            throw new ArgumentException("A share of the blades is dry from none up to all.", nameof(settings));

        var seed = ((ulong)(clump.Swing * 16_777_216f) << 24) | (ulong)(clump.Flutter * 16_777_216f);
        var random = new Pcg(seed);
        var count = Fewest + random.Below(Spread);
        var blades = new GrassBlade[count];
        for (var at = 0; at < count; at++)
        {
            var (sin, cos) = Exact.SinCos(random.Unit() * MathF.Tau);
            var radius = Foot * clump.Height * MathF.Sqrt(random.Unit());
            var root = new Vector3(clump.Root.X + radius * cos, 0f, clump.Root.Z + radius * sin);
            var outward = Exact.Atan2(sin, cos);
            var facing = outward + random.Signed() * (MathF.PI / 3f);
            facing -= MathF.Floor(facing / MathF.Tau) * MathF.Tau;
            var low = at > 0 && random.Unit() < LowShare;
            var height = clump.Height * (low ? 0.33f + 0.17f * random.Unit() : 0.6f + 0.4f * random.Unit());
            var width = 0.003f + 0.005f * random.Unit();
            var lean = 0.05f + 0.4f * random.Unit();
            var curl = 0.1f + 0.8f * random.Unit();
            var segments = 3 + random.Below(3);
            var look = random.Below(Looks.Count);
            var dry = random.Unit() < settings.DryShare;
            var shade = 1f + settings.ColorVariation * random.Signed();
            var stray = new Vector3(
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()),
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()),
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()));
            var (rootWanted, tipWanted) = dry
                ? (settings.DryColor * 0.8f, settings.DryColor)
                : (settings.RootColor, settings.TipColor);
            blades[at] = new GrassBlade(
                root, facing, height, width, lean, curl, segments, look, dry,
                Tint(rootWanted * stray, look), Tint(tipWanted * stray, look));
        }

        return blades;
    }

    // The multiplier that brings a look's mean color to the color wanted, never below zero.
    private static Vector3 Tint(Vector3 wanted, int look) =>
        Vector3.Max(wanted / LookMeans[look], Vector3.Zero);

    /// <summary>Writes a patch's blades as the arrays an engine draws.</summary>
    /// <param name="settings">
    /// The density, the height, the colors, how many blades are dry and how far the colors stray.
    /// </param>
    /// <param name="size">
    /// The patch's width along X in X and its depth along Z in Y, in meters, as
    /// <see cref="GrassPatch.Size"/> gives it.
    /// </param>
    /// <param name="seed">The patch's seed, as <see cref="GrassPatch.Seed"/> gives it.</param>
    /// <param name="segments">
    /// The most segments a blade is cut into, from one up, so a far patch takes fewer, a blade with
    /// fewer than that keeping its own count.
    /// </param>
    /// <returns>
    /// The blades of every clump of <see cref="GrassScatter.Clumps"/> over the patch, clump by
    /// clump and blade by blade, the same for the same arguments.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// A size or a setting is not usable, or <paramref name="segments"/> is below one.
    /// </exception>
    public static GrassMesh Write(GrassSettings settings, Vector2 size, uint seed, int segments)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 1);
        var clumps = GrassScatter.Clumps(settings, size, seed);
        var blades = new List<GrassBlade>(clumps.Count * 7);
        foreach (var clump in clumps)
            blades.AddRange(Of(clump, settings));

        // Every blade is 3 vertices a row for its segments and a tip, and 4 triangles between each
        // pair of rows and 2 at the tip, whatever its segments are.
        var (vertexCount, indexCount) = (0, 0);
        foreach (var blade in blades)
        {
            var rows = Math.Min(blade.Segments, segments);
            (vertexCount, indexCount) = (vertexCount + 3 * rows + 1, indexCount + 3 * (4 * rows - 2));
        }

        var positions = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var colors = new Vector3[vertexCount];
        var indices = new uint[indexCount];
        var (vertex, index) = (0, 0);
        foreach (var blade in blades)
            Strip(blade, Math.Min(blade.Segments, segments), positions, normals, uvs, colors, indices, ref vertex, ref index);
        return new GrassMesh(positions, normals, uvs, colors, indices, blades.Count);
    }

    // One blade's rows, from its root up, a row of three at each of the first rows' stations and
    // the one vertex of its tip at the end.
    private static void Strip(
        GrassBlade blade, int rows, Vector3[] positions, Vector3[] normals, Vector2[] uvs, Vector3[] colors,
        uint[] indices, ref int vertex, ref int index)
    {
        var (sin, cos) = Exact.SinCos(blade.Facing);
        var (along, side) = (new Vector3(cos, 0f, sin), new Vector3(-sin, 0f, cos));
        var (leanSin, leanCos) = Exact.SinCos(blade.Lean);
        var (tipSin, tipCos) = Exact.SinCos(blade.Lean + blade.Curl);
        var half = 0.5f * blade.Height;
        // The control polygon in the blade's own plane, X along its facing and Y up.
        var control = new Vector2(half * leanSin, half * leanCos);
        var tip = new Vector2(control.X + half * tipSin, control.Y + half * tipCos);
        var box = Looks[blade.Look];
        var first = (uint)vertex;
        for (var row = 0; row <= rows; row++)
        {
            var t = row / (float)rows;
            var u = 1f - t;
            var point = new Vector2(
                2f * u * t * control.X + t * t * tip.X,
                2f * u * t * control.Y + t * t * tip.Y);
            var slope = new Vector2(
                2f * u * control.X + 2f * t * (tip.X - control.X),
                2f * u * control.Y + 2f * t * (tip.Y - control.Y));
            var length = MathF.Sqrt(slope.X * slope.X + slope.Y * slope.Y);
            // The face looks to the facing's side of the curve, up and out as the blade leans.
            var face = new Vector2(slope.Y / length, -slope.X / length);
            var normal = along * face.X + Vector3.UnitY * face.Y;
            var middle = new Vector3(blade.Root.X + along.X * point.X, point.Y, blade.Root.Z + along.Z * point.X);
            var color = blade.RootColor * u + blade.TipColor * t;
            // The picture's root is the bottom of its box and its point the top, a blade standing
            // up its box, and its left and right edges are the box's sides.
            var v = box.V + box.Height * u;
            var (left, right) = (box.U, box.U + box.Width);
            if (row == rows)
            {
                positions[vertex] = middle;
                normals[vertex] = normal;
                uvs[vertex] = new Vector2(0.5f * (left + right), v);
                colors[vertex++] = color;
                break;
            }

            // The blade is folded about its midrib, which stands out toward the face, so the edges
            // lie back from the middle by the angle the edge normals are bent by.
            var reach = 0.5f * blade.Width * (1f - t * t);
            var bent = side * RoundSin;
            var back = normal * (reach * RoundSin);
            positions[vertex] = middle - side * (reach * RoundCos) - back;
            normals[vertex] = normal * RoundCos - bent;
            uvs[vertex] = new Vector2(left, v);
            colors[vertex++] = color;
            positions[vertex] = middle;
            normals[vertex] = normal;
            uvs[vertex] = new Vector2(0.5f * (left + right), v);
            colors[vertex++] = color;
            positions[vertex] = middle + side * (reach * RoundCos) - back;
            normals[vertex] = normal * RoundCos + bent;
            uvs[vertex] = new Vector2(right, v);
            colors[vertex++] = color;
        }

        // A cell between two rows is a pair of triangles for each half of the width, the +side
        // edge on the eye's left looking at the face, and the last row closes on the tip.
        for (var row = 0; row < rows; row++)
        {
            var lower = first + (uint)(3 * row);
            var last = row == rows - 1;
            var upper = lower + 3;
            for (var cell = 0; cell < 2; cell++)
            {
                var (right, left) = (lower + (uint)cell, lower + (uint)cell + 1);
                if (last)
                {
                    indices[index++] = left;
                    indices[index++] = right;
                    indices[index++] = upper;
                    continue;
                }

                var (upperRight, upperLeft) = (upper + (uint)cell, upper + (uint)cell + 1);
                indices[index++] = left;
                indices[index++] = right;
                indices[index++] = upperLeft;
                indices[index++] = right;
                indices[index++] = upperRight;
                indices[index++] = upperLeft;
            }
        }
    }
}
