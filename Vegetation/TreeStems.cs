using System.Numerics;

namespace Vegetation;

/// <summary>
/// A stem of a tree as a tube needs it, its points from the joint where it leaves its parent, the
/// radius at each, how many times its bark goes round it, and how far along the bark's picture each
/// point stands, from <see cref="TreeStems.Build(TreeSkeleton, TreeSpecies)"/>.
/// </summary>
/// <param name="Points">The stem's points in meters, the joint on its parent first, Y up.</param>
/// <param name="Radii">The stem's radius at each point in meters.</param>
/// <param name="Around">
/// How many times the bark's picture goes round the stem, a whole count above zero.
/// </param>
/// <param name="Along">
/// How far along the bark's picture each point stands, in repeats from the stem's first point, a
/// tube's texture coordinate along it, the picture's across being its coordinate round.
/// </param>
public sealed record TreeStem(IReadOnlyList<Vector3> Points, IReadOnlyList<float> Radii, int Around, IReadOnlyList<float> Along);

/// <summary>
/// A grown tree's stems as tubes need them, each with its bark wrapped round and along it, which an
/// engine builds its stems' meshes from with whatever tube it has.
/// </summary>
/// <remarks>
/// <para>
/// A stem runs from the joint on its parent through each point that carries it on, its thickest
/// child at each, as <see cref="TreeLimbs"/> follows them. The trunk takes the whole count round
/// nearest its circumference over the bark's repeat round, and a branch the whole count nearest its
/// parent's count times its radius over its parent's at the joint, at least one, so the bark keeps
/// its scale across the joint but for what rounding the count takes.
/// </para>
/// <para>
/// Along a stem the picture's repeat shortens with the radius, so its texels keep the shape they
/// have at the stem's base the whole way and the bark's features grow finer toward a tip, where a
/// repeat held at the base's length squeezes them round a tapering stem, a sixth at a tip a sixth
/// as thick.
/// </para>
/// </remarks>
public static class TreeStems
{
    // The repeat of the bark TreeLooks makes, its 128 by 512 texels laid half a meter round.
    private static readonly (float Across, float Down) LooksRepeat = (0.5f, 2f);

    /// <summary>Gives a grown tree's stems with its species' bark wrapped round them.</summary>
    /// <param name="skeleton">
    /// The tree, as <see cref="TreeSkeleton.Grow(TreeSpecies, uint)"/> grew it.
    /// </param>
    /// <param name="species">
    /// Its species, whose bark set's <see cref="TreeTextureSet.Meters"/> wraps the stems, or the
    /// bark <see cref="TreeLooks.Bark"/> makes where it names no set the collection holds with
    /// them.
    /// </param>
    /// <returns>The stems, the trunk's first and each after its parent's.</returns>
    public static IReadOnlyList<TreeStem> Build(TreeSkeleton skeleton, TreeSpecies species)
    {
        ArgumentNullException.ThrowIfNull(species);
        var repeat = species.BarkSet is { } name && TreeTextures.Find(name) is { Meters: { } meters } ? meters : LooksRepeat;
        return Build(skeleton, repeat);
    }

    /// <summary>
    /// Gives a grown tree's stems with a bark of a given repeat wrapped round them.
    /// </summary>
    /// <param name="skeleton">
    /// The tree, as <see cref="TreeSkeleton.Grow(TreeSpecies, uint)"/> grew it.
    /// </param>
    /// <param name="repeat">
    /// How many meters one repeat of the bark's picture covers, round and along.
    /// </param>
    /// <returns>The stems, the trunk's first and each after its parent's.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A repeat is not a positive number.</exception>
    public static IReadOnlyList<TreeStem> Build(TreeSkeleton skeleton, (float Across, float Down) repeat)
    {
        ArgumentNullException.ThrowIfNull(skeleton);
        if (!(repeat.Across > 0f && float.IsFinite(repeat.Across) && repeat.Down > 0f && float.IsFinite(repeat.Down)))
            throw new ArgumentOutOfRangeException(nameof(repeat), repeat, "A bark's repeat is a positive number of meters round and along.");

        var nodes = skeleton.Nodes;
        var carried = TreeLimbs.Carried(nodes);
        var around = new int[nodes.Count];
        var stems = new List<TreeStem>();
        for (var start = 0; start < nodes.Count; start++)
        {
            var parent = nodes[start].Parent;
            if (parent >= 0 && carried[parent] == start) continue;

            var chain = new List<int>();
            for (var at = start; at >= 0; at = carried[at]) chain.Add(at);
            var points = new List<Vector3>();
            var radii = new List<float>();
            if (parent >= 0)
            {
                points.Add(nodes[parent].Place);
                radii.Add(nodes[start].Radius);
            }

            foreach (var at in chain)
            {
                points.Add(nodes[at].Place);
                radii.Add(nodes[at].Radius);
            }

            var count = parent < 0
                ? Math.Max(1, (int)MathF.Round(MathF.Tau * nodes[start].Radius / repeat.Across))
                : Math.Max(1, (int)MathF.Round(around[parent] * nodes[start].Radius / MathF.Max(nodes[parent].Radius, 1e-6f)));
            foreach (var at in chain) around[at] = count;

            // A repeat along as long as the circumference over the count round, in the picture's
            // proportion, at each segment's mean radius.
            var along = new float[points.Count];
            for (var i = 1; i < points.Count; i++)
            {
                var radius = 0.5f * (radii[i - 1] + radii[i]);
                var length = Length(points[i] - points[i - 1]);
                var perRepeat = MathF.Tau * MathF.Max(radius, 1e-6f) / count * (repeat.Down / repeat.Across);
                along[i] = along[i - 1] + length / perRepeat;
            }

            stems.Add(new TreeStem(points, radii, count, along));
        }

        return stems;
    }

    private static float Length(Vector3 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
}
