using System.Numerics;

namespace Vegetation;

/// <summary>
/// A tree's leaves as vertex arrays, from
/// <see cref="TreeLeaves.Build(TreeSkeleton, TreeSpecies, uint, TreeDetail)"/>, which an engine
/// copies into a mesh of its own.
/// </summary>
/// <remarks>
/// <para>
/// A leaf is a card of four vertices, its stalk's two first and its tip's two after, two triangles
/// counterclockwise seen from its face. A frond is a strip of two vertices at each point of its
/// stem, two triangles between points. Places are in meters with Y up, as the skeleton's are.
/// </para>
/// <para>
/// <see cref="Sky"/> and <see cref="Out"/> are a card's, the same at its four vertices, and absent
/// for fronds, which are not shaded as a crown. An engine darkens a leaf by its sky, the share of
/// the sky that reaches it through the crown, to a floor of its own, and bends the leaf's diffuse
/// light toward its way out.
/// </para>
/// </remarks>
public sealed class TreeLeafMesh
{
    internal TreeLeafMesh(Vector3[] positions, Vector3[] normals, Vector2[] uvs, float[]? sky, Vector3[]? @out, Vector4[] limb, Vector4[] own, uint[] indices, int[] twigs)
    {
        (Positions, Normals, Uvs, Sky, Out, Limb, Own, Indices, Twigs) = (positions, normals, uvs, sky, @out, limb, own, indices, twigs);
    }

    /// <summary>Each vertex's place in meters.</summary>
    public IReadOnlyList<Vector3> Positions { get; }

    /// <summary>Each vertex's normal at unit length, the way its card or strip faces.</summary>
    public IReadOnlyList<Vector3> Normals { get; }

    /// <summary>
    /// Each vertex's texture coordinates in the leaves' atlas, from 0 to 1, U from the atlas's left
    /// and V from its top, a card's stalk at its box's bottom.
    /// </summary>
    public IReadOnlyList<Vector2> Uvs { get; }

    /// <summary>
    /// Each vertex's share of the sky that reaches its card through the crown, from 0 to 1, from
    /// <see cref="TreeCrown.Shade"/>, or nothing for fronds.
    /// </summary>
    public IReadOnlyList<float>? Sky { get; }

    /// <summary>
    /// Each vertex's way out of the crown at its card, at unit length, from
    /// <see cref="TreeCrown.Shade"/>, or nothing for fronds.
    /// </summary>
    public IReadOnlyList<Vector3>? Out { get; }

    /// <summary>
    /// Each vertex's limb's pivot in meters in X, Y and Z and the limb's phase from 0 up to 1 in W,
    /// from <see cref="TreeLimbs.Pivot"/> of the limb its twig grows from.
    /// </summary>
    public IReadOnlyList<Vector4> Limb { get; }

    /// <summary>
    /// Each vertex's own pivot in meters in X, Y and Z, the place its leaf flutters about, a card's
    /// stalk or a frond's foot, and the leaf's phase from 0 up to 1 in W.
    /// </summary>
    public IReadOnlyList<Vector4> Own { get; }

    /// <summary>The triangles, three vertices each by their index.</summary>
    public IReadOnlyList<uint> Indices { get; }

    /// <summary>
    /// The node of the tip of the twig each card stands on, a card a leaf, which a far card drawn
    /// in place of a branch's leaves gathers them by; empty for fronds.
    /// </summary>
    public IReadOnlyList<int> Twigs { get; }
}
