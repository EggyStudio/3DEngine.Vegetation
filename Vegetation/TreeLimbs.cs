using System.Numerics;

namespace Vegetation;

/// <summary>
/// Which limb each stem of a tree sways with in the wind and where the limb pivots, worked out from
/// the skeleton alone, which an engine writes into each vertex of the stems and the leaves for its
/// wind to move them by.
/// </summary>
/// <remarks>
/// <para>
/// A stem runs from the point it starts at through each point that carries it on, its thickest
/// child at each. The <see cref="Limbs"/> thickest stems are the tree's limbs, the trunk among
/// them, and each thinner stem swings with the limb it grows from. A stem that grows from the trunk
/// below every limb swings as a limb of its own, and the trunk swings with none.
/// </para>
/// <para>
/// A limb pivots where it leaves its parent, with a phase of its own, so the limbs of a tree do not
/// swing in step. The wind's push, a species' stiffness and how a vertex moves about its pivots are
/// the engine's.
/// </para>
/// </remarks>
public sealed class TreeLimbs
{
    /// <summary>How many of a tree's thickest stems are its limbs, the trunk among them.</summary>
    public const int Limbs = 16;

    private readonly int[] _stemOf;
    private readonly Dictionary<int, int> _owner;
    private readonly IReadOnlyList<TreeNode> _nodes;

    private TreeLimbs(int[] stemOf, IReadOnlyList<int> starts, IReadOnlySet<int> kept, Dictionary<int, int> owner, IReadOnlyList<TreeNode> nodes)
    {
        (_stemOf, Starts, Kept, _owner, _nodes) = (stemOf, starts, kept, owner, nodes);
    }

    /// <summary>
    /// The node each stem starts at, the trunk's, node 0, first and each after its parent's.
    /// </summary>
    public IReadOnlyList<int> Starts { get; }

    /// <summary>The nodes the limbs start at, the <see cref="Limbs"/> thickest stems.</summary>
    public IReadOnlySet<int> Kept { get; }

    /// <summary>Works out the limbs of a grown tree.</summary>
    /// <param name="skeleton">
    /// The tree, as <see cref="TreeSkeleton.Grow(TreeSpecies, uint)"/> grew it.
    /// </param>
    /// <returns>The tree's limbs.</returns>
    public static TreeLimbs Plan(TreeSkeleton skeleton)
    {
        ArgumentNullException.ThrowIfNull(skeleton);
        var nodes = skeleton.Nodes;
        var carried = Carried(nodes);
        var stemOf = new int[nodes.Count];
        var starts = new List<int>();
        for (var start = 0; start < nodes.Count; start++)
        {
            var parent = nodes[start].Parent;
            if (parent >= 0 && carried[parent] == start) continue;
            starts.Add(start);
            for (var node = start; node >= 0; node = carried[node]) stemOf[node] = start;
        }

        var kept = starts.OrderByDescending(start => nodes[start].Radius).ThenBy(start => start).Take(Limbs).ToHashSet();

        // A stem starts after the one it grows from, so each stem's owner is known by its turn.
        var owner = new Dictionary<int, int>();
        foreach (var start in starts)
            owner[start] = kept.Contains(start) || start == 0 ? start : owner[stemOf[nodes[start].Parent]];

        return new TreeLimbs(stemOf, starts, kept, owner, nodes);
    }

    /// <summary>The node the stem a node stands on starts at.</summary>
    /// <param name="node">The node, by its index in the skeleton.</param>
    /// <returns>The stem's first node.</returns>
    public int StemOf(int node) => _stemOf[node];

    /// <summary>
    /// The limb a stem grows from, by where the limb starts, itself where it is a limb.
    /// </summary>
    /// <param name="stem">The stem, by the node it starts at.</param>
    /// <returns>The node the limb starts at.</returns>
    public int OwnerOf(int stem) => _owner[stem];

    /// <summary>
    /// The limb a stem swings with, none for the trunk, the limb it grows from, or itself for a
    /// stem that grows from the trunk below every limb.
    /// </summary>
    /// <param name="stem">The stem, by the node it starts at.</param>
    /// <returns>The node the limb starts at, or -1 for none.</returns>
    public int LimbOf(int stem) => stem == 0 ? -1 : _owner[stem] == 0 ? stem : _owner[stem];

    /// <summary>
    /// A limb's pivot and phase, where it leaves its parent in X, Y and Z and its phase from 0 up
    /// to 1 in W, or a point's own place and no phase for no limb, so the point does not swing.
    /// </summary>
    /// <param name="limb">The limb, by the node it starts at, or -1 for none.</param>
    /// <param name="at">
    /// The point's place, which stands for the pivot where there is no limb.
    /// </param>
    /// <returns>The pivot in meters and the phase.</returns>
    public Vector4 Pivot(int limb, Vector3 at)
    {
        if (limb < 0) return new Vector4(at.X, at.Y, at.Z, 0f);
        var pivot = _nodes[_nodes[limb].Parent].Place;
        return new Vector4(pivot.X, pivot.Y, pivot.Z, Phase(limb));
    }

    /// <summary>
    /// A phase from 0 up to 1 for a limb or a leaf, by its number, spread by Knuth's hash.
    /// </summary>
    /// <param name="number">The limb's or the leaf's number.</param>
    /// <returns>The phase.</returns>
    public static float Phase(int number) => (uint)(number * 2654435761u) / 4294967296f;

    // The child each node carries its stem on to, its thickest, or -1 at a stem's tip.
    internal static int[] Carried(IReadOnlyList<TreeNode> nodes)
    {
        var carried = new int[nodes.Count];
        Array.Fill(carried, -1);
        for (var i = 1; i < nodes.Count; i++)
        {
            var parent = nodes[i].Parent;
            if (carried[parent] < 0 || nodes[i].Radius > nodes[carried[parent]].Radius) carried[parent] = i;
        }

        return carried;
    }
}
