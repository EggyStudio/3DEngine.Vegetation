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
/// seeds and its order, are left as they were.
/// </para>
/// <para>
/// A clump has two layers. Three to five coarse blades stand tallest, from six tenths of the
/// clump's height up, and a short fine underfill of many more, a third to a half of the height
/// less what the droop takes, fills in beneath them, the settings' fine share of all the blades.
/// The underfill is dense enough that no ground shows between blades from any height of the eye
/// while the thickness is whole, and a lower thickness thins the fine layer first and then leaves
/// bare patches. Each blade draws, in this order, where its root falls in the clump's foot, how it
/// faces, whether it stands upright, how tall it is, how wide, how it bends, how many segments it
/// has, which look it has, whether it is dry and how its colors stray, whatever the settings.
/// </para>
/// <para>
/// A blade's bend grows with its length and with its distance from the clump's middle. It rises a
/// little, arches over, and its outer half lies near level or drooping, and it faces anywhere about
/// the clump so that blades cross, the few upright ones excepted, which are the share
/// <see cref="GrassSettings.UprightShare"/> asks for. The droop, a number from nothing to one,
/// blends this limp bend with the upright bend of a meadow, so a lawn lies and a meadow stands
/// with the same blades.
/// </para>
/// <para>
/// The height and the coarseness of the grass vary over the place in mounds, by a slow noise from
/// the patch's middle and the settings' seed, so a patch's middle must be given for neighboring
/// patches to join without a seam. A blade is a strip of nearly one width, narrowing to a point
/// over its last fifth, cut from one of three blades of the photographed set <see cref="Set"/>,
/// whose box in the atlas is its look, so its color, its midrib and veins and the light it lets
/// through are the photograph's. A share of the blades are dry and yellowed, the same pictures
/// tinted to the dry color. The tints are the color wanted over the mean color of the look's
/// picture, so the settings' colors tint the photograph and do not replace it.
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

    // A clump's coarse blades are three to five, and its foot's radius is a share of its height.
    private const int Fewest = 3;
    private const int Spread = 3;
    private const float Foot = 0.15f;

    // The side of the cells of the noise that bares the ground, in meters.
    private const float BareCell = 0.12f;

    // Where a blade's width starts to narrow, as a share of its length.
    private const float Taper = 0.8f;

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
    /// How wide a blade is at a place along its length, as a share of the width it holds over most
    /// of it.
    /// </summary>
    /// <param name="along">How far along the blade, 0 at its root up to 1 at its point.</param>
    /// <returns>
    /// Whole up to four fifths of the way along and narrowing in a straight line to nothing at the
    /// point, as a grass blade does.
    /// </returns>
    public static float WidthShare(float along) =>
        along <= Taper ? 1f : MathF.Max(0f, (1f - along) / (1f - Taper));

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
    /// than its height to the side, and the blade is less than a centimeter wide, so under half of
    /// that either side of the curve.
    /// </remarks>
    public static float Reach(GrassClump clump) => clump.Height * (Foot + 1f) + 0.01f;

    /// <summary>Draws a clump's blades from the clump and the settings.</summary>
    /// <param name="clump">The clump, as <see cref="GrassScatter.Clumps"/> gives it.</param>
    /// <param name="settings">
    /// The kind, the season, the health and the thickness, the bend, the layers and the colors.
    /// </param>
    /// <param name="middle">
    /// The middle of the clump's patch, as <see cref="GrassPatch.Middle"/> gives it, which places
    /// the clump on the noise of the mounds, so a clump has the same look where it stands whichever
    /// patch it belongs to. It is the origin where left out.
    /// </param>
    /// <returns>
    /// The clump's blades, three to five coarse ones and the fine ones after them, the same for the
    /// same clump and settings, and none of them where the thickness leaves the ground bare.
    /// </returns>
    /// <exception cref="ArgumentException">A setting is not usable.</exception>
    public static IReadOnlyList<GrassBlade> Of(GrassClump clump, GrassSettings settings, Vector2 middle = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return Grow(clump, settings, GrassLook.Of(settings), middle);
    }

    private static GrassBlade[] Grow(GrassClump clump, GrassSettings settings, GrassLook look, Vector2 middle)
    {
        var place = new Vector2(middle.X + clump.Root.X, middle.Y + clump.Root.Z);
        var variation = settings.PatchVariation;
        var thickness = settings.Thickness;
        // Soil shows between tufts a hand's breadth across as the thickness drops, which none of a
        // whole thickness does, and the mounds are taller and finer where their noise is high.
        if (Noise(place, BareCell, settings.Seed, 3) < 0.85f * (1f - thickness))
            return [];
        var tallness = 1f - 0.5f * variation * (1f - Noise(place, settings.MoundSize, settings.Seed, 1));
        var coarseness = Noise(place, settings.MoundSize, settings.Seed, 2);
        var widthScale = 1f + 0.6f * variation * (coarseness - 0.5f);
        var fineShare = look.FineShare * (1f - 0.4f * variation * coarseness);
        var seed = ((ulong)(clump.Swing * 16_777_216f) << 24) | (ulong)(clump.Flutter * 16_777_216f);
        var random = new Pcg(seed);
        var coarse = Fewest + random.Below(Spread);
        if (thickness < 1f) coarse = Math.Max(1, (int)MathF.Round(coarse * (0.4f + 0.6f * thickness)));
        var count = coarse + (int)MathF.Round(coarse * fineShare / (1f - fineShare) * thickness * thickness);
        var blades = new GrassBlade[count];
        var height = clump.Height * look.HeightScale * tallness;
        for (var at = 0; at < count; at++)
        {
            var fine = at >= coarse;
            var (sin, cos) = Exact.SinCos(random.Unit() * MathF.Tau);
            var share = MathF.Sqrt(random.Unit());
            var radius = Foot * clump.Height * share * (fine ? 2f : 1f);
            var root = new Vector3(clump.Root.X + radius * cos, 0f, clump.Root.Z + radius * sin);
            var outward = Exact.Atan2(sin, cos);
            var facing = outward + random.Signed() * (MathF.PI / 3f + 0.5f * MathF.PI * look.Droop);
            facing -= MathF.Floor(facing / MathF.Tau) * MathF.Tau;
            var upright = random.Unit() < settings.UprightShare;
            var tall = height * (fine
                ? 0.33f - 0.15f * settings.Droop + 0.17f * random.Unit()
                : 0.6f + 0.4f * random.Unit());
            var width = (fine ? look.FineLow + (look.FineHigh - look.FineLow) * random.Unit()
                : look.WidthLow + (look.WidthHigh - look.WidthLow) * random.Unit()) * widthScale;
            var (leanDraw, curlDraw, bendDraw) = (random.Unit(), random.Unit(), random.Unit());
            var segments = 3 + random.Below(3);
            var kind = random.Below(Looks.Count);
            var dry = random.Unit() < look.DryShare;
            var shade = 1f + settings.ColorVariation * random.Signed();
            var stray = new Vector3(
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()),
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()),
                shade * (1f + 0.5f * settings.ColorVariation * random.Signed()));

            // The upright bend is a meadow's, a lean of up to half a radian and a curl of up to
            // nine tenths, and the limp bend rises a little and arches over past level, further the
            // longer the blade and the further from the clump's middle it stands.
            var droop = upright ? 0f : look.Droop;
            var length = tall / (clump.Height * look.HeightScale);
            var bend = Math.Clamp(fine
                ? 0.3f + 0.5f * share + 0.2f * (bendDraw - 0.5f)
                : 0.25f + 0.45f * length + 0.3f * share + 0.2f * (bendDraw - 0.5f), 0f, 1f);
            var limpTip = (fine ? 1.4f + 0.9f * bend : 1.45f + 1.05f * bend) + 0.35f * look.Flat;
            var (uprightLean, uprightTip) = (0.05f + 0.4f * leanDraw, 0.05f + 0.4f * leanDraw + 0.1f + 0.8f * curlDraw);
            var lean = uprightLean + (0.15f + 0.35f * leanDraw - uprightLean) * droop;
            var tip = uprightTip + (limpTip - uprightTip) * droop;
            // A blade's tip stays above the ground however far it has drooped.
            tip = MathF.Max(lean, MathF.Min(tip, MathF.PI - lean - 0.2f));
            var (rootWanted, tipWanted) = dry ? (look.Dry * 0.8f, look.Dry) : (look.Root, look.Tip);
            blades[at] = new GrassBlade(
                root, facing, tall, width, lean, tip - lean, fine ? 2 : segments, kind, dry, fine,
                Tint(rootWanted * stray, kind), Tint(tipWanted * stray, kind));
        }

        return blades;
    }

    // The multiplier that brings a look's mean color to the color wanted, never below zero.
    private static Vector3 Tint(Vector3 wanted, int look) =>
        Vector3.Max(wanted / LookMeans[look], Vector3.Zero);

    // A slow noise from 0 up to 1 over the ground, one value to a cell of the given size smoothly
    // blended, from the settings' seed and a number for each of the noises.
    private static float Noise(Vector2 place, float cell, uint seed, uint salt)
    {
        var (x, z) = (place.X / cell, place.Y / cell);
        var (ix, iz) = (MathF.Floor(x), MathF.Floor(z));
        var (fx, fz) = (x - ix, z - iz);
        (fx, fz) = (fx * fx * (3f - 2f * fx), fz * fz * (3f - 2f * fz));
        var (cx, cz) = ((int)ix, (int)iz);
        var near = Lattice(cx, cz, seed, salt) + (Lattice(cx + 1, cz, seed, salt) - Lattice(cx, cz, seed, salt)) * fx;
        var far = Lattice(cx, cz + 1, seed, salt) + (Lattice(cx + 1, cz + 1, seed, salt) - Lattice(cx, cz + 1, seed, salt)) * fx;
        return near + (far - near) * fz;
    }

    private static float Lattice(int x, int z, uint seed, uint salt)
    {
        unchecked
        {
            var h = seed * 0x9E3779B9u ^ (uint)x * 0x85EBCA6Bu ^ (uint)z * 0xC2B2AE35u ^ salt * 0x27D4EB2Fu;
            h ^= h >> 15;
            h *= 0x2C1B3C6Du;
            h ^= h >> 12;
            h *= 0x297A2D39u;
            h ^= h >> 15;
            return (h >> 8) * (1f / 16_777_216f);
        }
    }

    /// <summary>Writes a patch's blades as the arrays an engine draws.</summary>
    /// <param name="settings">
    /// The density, the height, the kind, the season, the health and the thickness, the bend, the
    /// layers and the colors.
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
    /// <param name="middle">
    /// The patch's middle, as <see cref="GrassPatch.Middle"/> gives it, so the mounds of the grass
    /// run on over the borders of its patches. It is the origin where left out.
    /// </param>
    /// <returns>
    /// The blades of every clump of <see cref="GrassScatter.Clumps"/> over the patch, clump by
    /// clump and blade by blade, the same for the same arguments.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// A size or a setting is not usable, or <paramref name="segments"/> is below one.
    /// </exception>
    public static GrassMesh Write(GrassSettings settings, Vector2 size, uint seed, int segments, Vector2 middle = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 1);
        var look = GrassLook.Of(settings);
        var clumps = GrassScatter.Clumps(settings, size, seed);
        var blades = new List<GrassBlade>(clumps.Count * 12);
        foreach (var clump in clumps)
            blades.AddRange(Grow(clump, settings, look, middle));

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
            // The face looks to the outer side of the curve, up where the blade has arched over.
            var face = new Vector2(-slope.Y / length, slope.X / length);
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

            // The blade is folded in a channel about its midrib, which lies low, so the edges
            // rise toward the face by the angle the edge normals are bent by and its normals lean
            // in toward the middle, as a grass blade's do.
            var reach = 0.5f * blade.Width * WidthShare(t);
            var bent = side * RoundSin;
            var lift = normal * (reach * RoundSin);
            positions[vertex] = middle - side * (reach * RoundCos) + lift;
            normals[vertex] = normal * RoundCos + bent;
            uvs[vertex] = new Vector2(left, v);
            colors[vertex++] = color;
            positions[vertex] = middle;
            normals[vertex] = normal;
            uvs[vertex] = new Vector2(0.5f * (left + right), v);
            colors[vertex++] = color;
            positions[vertex] = middle + side * (reach * RoundCos) + lift;
            normals[vertex] = normal * RoundCos - bent;
            uvs[vertex] = new Vector2(right, v);
            colors[vertex++] = color;
        }

        // A cell between two rows is a pair of triangles for each half of the width, the -side
        // edge on the eye's right looking at the face, and the last row closes on the tip.
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
                    indices[index++] = right;
                    indices[index++] = left;
                    indices[index++] = upper;
                    continue;
                }

                var (upperRight, upperLeft) = (upper + (uint)cell, upper + (uint)cell + 1);
                indices[index++] = right;
                indices[index++] = left;
                indices[index++] = upperRight;
                indices[index++] = left;
                indices[index++] = upperLeft;
                indices[index++] = upperRight;
            }
        }
    }
}
