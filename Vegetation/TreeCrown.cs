using System.Numerics;

namespace Vegetation;

/// <summary>
/// A tree's crown as a volume of leaves, from which each leaf takes how much of the sky reaches it
/// through the leaves round it and which way is out of the crown at it, so an engine shades a leaf
/// deep in the crown darker than one on its rim and bends its light toward the outside.
/// </summary>
/// <remarks>
/// <para>
/// Places are in meters with Y up, as the skeleton's are, and the sky is read from straight up and
/// a ring of eight ways a third of the way down from it, each weighted by how high it stands, as a
/// cosine weights the sky a surface facing up sees.
/// </para>
/// <para>
/// The leaves' area is gathered into a grid of voxels over the crown, 24 across its longest side,
/// fine enough to tell a crown's middle from its rim and coarse enough that each voxel holds some
/// tens of leaves. The sky a leaf sees is the light let through along each way by Beer and
/// Lambert's law, the leaf area density times half of it, the mean share facing a ray of leaves
/// turned every way, summed along the ray from a voxel past the leaf's own, so a leaf does not
/// shade itself. A leaf's card is taken to be six tenths leaf, the rest of its box the transparent
/// corners of its picture.
/// </para>
/// <para>
/// The way out is the way the density, smoothed over a voxel each side, falls fastest, leaning a
/// quarter of the way toward the way from the crown's middle, and wholly that way where the density
/// is even, deep in a crown of one density.
/// </para>
/// </remarks>
public static class TreeCrown
{
    // The grid's voxels across the crown's longest side.
    private const int Voxels = 24;

    // The share of a leaf's card its leaf covers.
    private const float LeafFill = 0.6f;

    // The mean share of a leaf's area that faces a ray, for leaves turned every way.
    private const float FacingShare = 0.5f;

    private static readonly (Vector3 Way, float Weight)[] SkyWays = MakeSkyWays();

    /// <summary>
    /// Finds how much of the sky each leaf sees through the crown and which way is out of the
    /// crown at it.
    /// </summary>
    /// <param name="places">Each leaf's middle in meters, Y up.</param>
    /// <param name="areas">
    /// Each leaf's card's area in square meters, in the order of the places.
    /// </param>
    /// <returns>
    /// For each leaf in the order of the places, the share of the sky it sees from 0 to 1, and the
    /// way out of the crown at it at unit length, or two empty arrays for no leaves.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// A place is not finite, as the places argument, or the areas are not as many as the places
    /// or one is not a finite number at or above zero, as the areas argument.
    /// </exception>
    public static (float[] Sky, Vector3[] Out) Shade(IReadOnlyList<Vector3> places, IReadOnlyList<float> areas)
    {
        ArgumentNullException.ThrowIfNull(places);
        ArgumentNullException.ThrowIfNull(areas);
        if (areas.Count != places.Count)
            throw new ArgumentException($"There are {areas.Count} areas for {places.Count} leaves.", nameof(areas));
        for (var i = 0; i < places.Count; i++)
        {
            if (!float.IsFinite(places[i].X) || !float.IsFinite(places[i].Y) || !float.IsFinite(places[i].Z))
                throw new ArgumentException($"Leaf {i} stands at {places[i]}, which is not a place.", nameof(places));
            if (!float.IsFinite(areas[i]) || areas[i] < 0f)
                throw new ArgumentException($"Leaf {i}'s area is {areas[i]}, where an area is a finite number at or above zero.", nameof(areas));
        }

        var count = places.Count;
        if (count == 0) return ([], []);

        var low = places.Aggregate((a, b) => new Vector3(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z)));
        var high = places.Aggregate((a, b) => new Vector3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z)));
        var middle = (low + high) * 0.5f;
        var size = MathF.Max(MathF.Max(high.X - low.X, high.Y - low.Y), MathF.Max(high.Z - low.Z, 1e-3f)) / Voxels;
        low -= new Vector3(size);
        var (width, height, depth) = ((int)((high.X - low.X) / size) + 2, (int)((high.Y - low.Y) / size) + 2, (int)((high.Z - low.Z) / size) + 2);

        // The leaf area in each voxel over its volume, square meters a cubic meter.
        var density = new float[width * height * depth];
        int Index(int x, int y, int z) => x + width * (y + height * z);
        (int X, int Y, int Z) Voxel(Vector3 place) => ((int)MathF.Floor((place.X - low.X) / size), (int)MathF.Floor((place.Y - low.Y) / size), (int)MathF.Floor((place.Z - low.Z) / size));
        bool Inside(int x, int y, int z) => x >= 0 && y >= 0 && z >= 0 && x < width && y < height && z < depth;
        for (var i = 0; i < count; i++)
        {
            var (x, y, z) = Voxel(places[i]);
            density[Index(x, y, z)] += areas[i] * LeafFill / (size * size * size);
        }

        // Smoothed over a voxel each side for the way out, which a single voxel's count is too
        // uneven to give.
        var smooth = new float[density.Length];
        for (var z = 0; z < depth; z++)
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sum = 0f;
            for (var k = -1; k <= 1; k++)
            for (var j = -1; j <= 1; j++)
            for (var i = -1; i <= 1; i++)
                if (Inside(x + i, y + j, z + k)) sum += density[Index(x + i, y + j, z + k)];
            smooth[Index(x, y, z)] = sum / 27f;
        }

        float Smooth(int x, int y, int z) => Inside(x, y, z) ? smooth[Index(x, y, z)] : 0f;

        // Summed a product at a time rather than by a library's dot product, whose order of sums
        // may round apart from one runtime to the next.
        static float LengthSquared(Vector3 v) => v.X * v.X + v.Y * v.Y + v.Z * v.Z;

        var sky = new float[count];
        var outward = new Vector3[count];
        for (var i = 0; i < count; i++)
        {
            var place = places[i];
            var seen = 0f;
            foreach (var (way, weight) in SkyWays)
            {
                // Half a voxel a step, from a voxel off the leaf, until the ray leaves the grid.
                var depthAlong = 0f;
                for (var along = size; ; along += size * 0.5f)
                {
                    var (x, y, z) = Voxel(place + way * along);
                    if (!Inside(x, y, z)) break;
                    depthAlong += density[Index(x, y, z)] * size * 0.5f;
                }

                seen += weight * Exact.Exp(-FacingShare * depthAlong);
            }

            // Held at one, which the weights' sum passes by a float's last digit where every ray
            // leaves the crown unshaded.
            sky[i] = MathF.Min(seen, 1f);

            var (vx, vy, vz) = Voxel(place);
            var slope = new Vector3(Smooth(vx - 1, vy, vz) - Smooth(vx + 1, vy, vz), Smooth(vx, vy - 1, vz) - Smooth(vx, vy + 1, vz), Smooth(vx, vy, vz - 1) - Smooth(vx, vy, vz + 1));
            var away = place - middle;
            var from = LengthSquared(away) > 1e-6f ? away.Normalized : Vector3.UnitY;
            outward[i] = LengthSquared(slope) > 1e-6f ? (slope.Normalized + from * 0.25f).Normalized : from;
        }

        return (sky, outward);
    }

    // The ways the sky is read from with their weights, which sum to one.
    private static (Vector3 Way, float Weight)[] MakeSkyWays()
    {
        var ways = new List<(Vector3, float)> { (Vector3.UnitY, 1f) };
        var height = Exact.Sin(MathF.PI / 6f);
        var across = Exact.Cos(MathF.PI / 6f);
        for (var k = 0; k < 8; k++)
        {
            var (sin, cos) = Exact.SinCos(k * MathF.Tau / 8f);
            ways.Add((new Vector3(across * cos, height, across * sin), height));
        }

        var total = ways.Sum(way => way.Item2);
        return [.. ways.Select(way => (way.Item1, way.Item2 / total))];
    }
}
