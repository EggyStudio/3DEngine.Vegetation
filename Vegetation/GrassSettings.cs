using System.Numerics;

namespace Vegetation;

/// <summary>
/// What a stretch of grass is planted with and how thick it stands, as <see cref="GrassScatter"/>
/// scatters it and <see cref="GrassBlades"/> grows its blades.
/// </summary>
/// <remarks>
/// <para>
/// The same settings and the same stretch give the same grass, every clump where it stood and as
/// tall, since the seed scatters, turns and sizes them.
/// </para>
/// <para>
/// A settings left as it is plants a green meadow grass 0.3 meters tall, and <see cref="Wheat"/> is
/// the taller and paler second preset. The blades are the near level, cut from the photographed
/// blades of <see cref="GrassBlades.Set"/> and tinted from <see cref="RootColor"/> at the root to
/// <see cref="TipColor"/> at the tip, <see cref="DryShare"/> of them dry and yellowed, and the
/// crossed cards of <see cref="Set"/> are the far level.
/// </para>
/// </remarks>
public sealed record GrassSettings
{
    /// <summary>
    /// Settings with every value at its value unless given, a field of green meadow grass.
    /// </summary>
    public GrassSettings()
    {
    }

    /// <summary>A meadow of green grass 0.3 meters tall, what settings left alone plant.</summary>
    public static GrassSettings Meadow { get; } = new();

    /// <summary>
    /// A field of wheat, taller than the meadow and paler, standing a little thinner.
    /// </summary>
    public static GrassSettings Wheat { get; } = new()
    {
        Density = 200f,
        Height = 0.9f,
        RootColor = TreeSpecies.Srgb(0.50f, 0.42f, 0.20f),
        TipColor = TreeSpecies.Srgb(0.82f, 0.72f, 0.42f),
        DryShare = 0.15f,
        DryColor = TreeSpecies.Srgb(0.80f, 0.68f, 0.38f),
    };

    /// <summary>
    /// The set of the texture collection the far cards are cut from, one of the kind
    /// <see cref="TreeTextureKind.Grass"/>, <c>wheat-grass</c> unless given.
    /// </summary>
    public string Set { get; init; } = "wheat-grass";

    /// <summary>
    /// How many clumps stand on a square meter, each five to nine blades or two cards crossed.
    /// </summary>
    public float Density { get; init; } = 350f;

    /// <summary>
    /// How tall a clump stands on average, in meters, its blades up to that tall and its far card
    /// as wide as the set's picture is for that height.
    /// </summary>
    public float Height { get; init; } = 0.3f;

    /// <summary>
    /// How far a clump's height strays from the average either way, as a share of it.
    /// </summary>
    public float Variation { get; init; } = 0.3f;

    /// <summary>The seed the clumps are scattered, turned, sized and swayed by.</summary>
    public uint Seed { get; init; }

    /// <summary>
    /// The color a green blade's picture is tinted to at its root, in linear sRGB, a deep green
    /// unless given.
    /// </summary>
    public Vector3 RootColor { get; init; } = TreeSpecies.Srgb(0.12f, 0.27f, 0.06f);

    /// <summary>
    /// The color a green blade's picture is tinted to at its tip, in linear sRGB, a lighter yellow
    /// green unless given.
    /// </summary>
    public Vector3 TipColor { get; init; } = TreeSpecies.Srgb(0.42f, 0.60f, 0.17f);

    /// <summary>
    /// The share of the blades that are dry and yellowed, from nothing up to the whole, which take
    /// <see cref="DryColor"/> in place of the two colors above.
    /// </summary>
    public float DryShare { get; init; } = 0.1f;

    /// <summary>
    /// The color of a dry blade at its tip, in linear sRGB, a straw yellow unless given, and
    /// four fifths of it at its root.
    /// </summary>
    public Vector3 DryColor { get; init; } = TreeSpecies.Srgb(0.55f, 0.47f, 0.20f);

    /// <summary>
    /// How far a blade's colors stray from the two above, from nothing up to the whole, as a share
    /// of their brightness and half of that in each channel.
    /// </summary>
    public float ColorVariation { get; init; } = 0.2f;

    /// <summary>
    /// The side of the square patches the grass is drawn in, in meters, each a mesh of its own that
    /// the camera culls as one.
    /// </summary>
    public float PatchSize { get; init; } = 8f;

    /// <summary>
    /// Whether the grass casts shadows, which many thin blades or small cut-out cards cost more
    /// than they show and whose thin edges flicker in the sun's shadow map as they sway. They
    /// receive shadows either way.
    /// </summary>
    public bool CastsShadows { get; init; }
}
