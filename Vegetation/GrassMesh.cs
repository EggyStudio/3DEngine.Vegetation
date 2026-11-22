using System.Numerics;

namespace Vegetation;

/// <summary>
/// A patch's blades as vertex arrays, from <see cref="GrassBlades.Write"/>, which an engine copies
/// into a mesh of its own.
/// </summary>
/// <remarks>
/// <para>
/// A blade is a strip of rows of three vertices, its left edge, its middle and its right edge,
/// narrowing to a single vertex at its tip, and each cell of the strip is two triangles
/// counterclockwise seen from the face its normals point from. Places are in meters with Y up,
/// from the patch's middle, as <see cref="GrassClump.Root"/> is.
/// </para>
/// <para>
/// A blade has a face and no back, so an engine draws it with both sides and turns the normal
/// toward the eye on the side it sees. The normals are bent across the width, the edges' away from
/// the middle's, so a flat strip shades as a rounded blade, and the middle stands out toward the
/// face a little, so the blade is folded about its midrib.
/// </para>
/// <para>
/// A vertex's coordinates are in the atlas of <see cref="GrassBlades.Set"/>, in the box of its
/// blade's look, across the blade's width and up its length, so the albedo, the normal and the
/// surface of that set, sampled there, give the blade its color, its midrib and veins and the light
/// it lets through. The colors are the tint that multiplies the albedo.
/// </para>
/// </remarks>
public sealed class GrassMesh
{
    internal GrassMesh(Vector3[] positions, Vector3[] normals, Vector2[] uvs, Vector3[] colors, uint[] indices, int blades)
    {
        (Positions, Normals, Uvs, Colors, Indices, Blades) = (positions, normals, uvs, colors, indices, blades);
    }

    /// <summary>Each vertex's place in meters.</summary>
    public IReadOnlyList<Vector3> Positions { get; }

    /// <summary>
    /// Each vertex's normal at unit length, the way its blade's face looks, bent across the blade's
    /// width.
    /// </summary>
    public IReadOnlyList<Vector3> Normals { get; }

    /// <summary>
    /// Each vertex's place in the atlas of <see cref="GrassBlades.Set"/>, as the share of its width
    /// from the left and of its height from the top, inside the box of its blade's look.
    /// </summary>
    public IReadOnlyList<Vector2> Uvs { get; }

    /// <summary>
    /// Each vertex's tint in linear sRGB, which multiplies the albedo sampled at its coordinates,
    /// its blade's root tint at its root and its tip tint at its tip, mixed by how far up the blade
    /// it stands.
    /// </summary>
    public IReadOnlyList<Vector3> Colors { get; }

    /// <summary>
    /// The triangles' corners, three to a triangle, as indices into the arrays above.
    /// </summary>
    public IReadOnlyList<uint> Indices { get; }

    /// <summary>How many blades the arrays hold.</summary>
    public int Blades { get; }
}
