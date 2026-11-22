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
/// A settings left as it is plants the <see cref="Lawn"/>, a bright green grass left unmown, its
/// longest blades 0.3 meters, limp and crossing over a closed mat of fine short blades, in
/// autumn, healthy and thick. <see cref="Meadow"/> is the upright grass and <see cref="Wheat"/>
/// the taller and paler one. The blades are the near level of detail, cut from the photographed
/// blades of <see cref="GrassBlades.Set"/> and tinted from the root color to the tip color,
/// <see cref="DryShare"/> of them dry and yellowed, and the crossed cards of <see cref="Set"/> are
/// the far level of detail.
/// </para>
/// <para>
/// The kind, the season, the health and the thickness are the stretch's condition. The kind picks
/// the palette, and the season, health and thickness are numbers a host sets and blends, as
/// <see cref="GrassCondition"/> names their ends, which move the blades' length, droop, color and
/// dry share and how many blades stand and where the soil shows.
/// </para>
/// </remarks>
public sealed record GrassSettings
{
    /// <summary>
    /// Settings with every value at the lawn's, a lawn in autumn, healthy and thick, unless given.
    /// </summary>
    public GrassSettings()
    {
    }

    /// <summary>The lawn grass, what settings left alone plant.</summary>
    public static GrassSettings Lawn { get; } = new();

    /// <summary>
    /// A meadow of upright green grass 0.3 meters tall, in summer, its blades bending a little and
    /// standing as a clump's fountain with a lower body under it.
    /// </summary>
    public static GrassSettings Meadow { get; } = new()
    {
        Kind = GrassKind.Meadow,
        Season = GrassCondition.Summer,
        Density = 350f,
        Droop = 0.1f,
        PatchVariation = 0.15f,
        FineShare = 0.4f,
        UprightShare = 0.3f,
        DryShare = 0.1f,
    };

    /// <summary>
    /// A field of wheat, taller than the meadow and paler, standing a little thinner and bending
    /// at the tip.
    /// </summary>
    public static GrassSettings Wheat { get; } = new()
    {
        Kind = GrassKind.Wheat,
        Season = GrassCondition.Summer,
        Density = 200f,
        Height = 0.9f,
        Droop = 0.35f,
        PatchVariation = 0.15f,
        FineShare = 0.3f,
        UprightShare = 0.3f,
        DryShare = 0.15f,
    };

    /// <summary>
    /// The set of the texture collection the far cards are cut from, one of the kind
    /// <see cref="TreeTextureKind.Grass"/>, <c>wheat-grass</c> unless given.
    /// </summary>
    public string Set { get; init; } = "wheat-grass";

    /// <summary>The kind of grass, picking its palette and widths, a lawn unless given.</summary>
    public GrassKind Kind { get; init; } = GrassKind.Lawn;

    /// <summary>
    /// The season, a number round a cycle of four, spring at 0, summer at 1, autumn at 2 and
    /// winter at 3 as <see cref="GrassCondition"/> names them, autumn unless given.
    /// </summary>
    /// <remarks>
    /// Spring is fresh and short, summer deep green, autumn long and limp with a few blades
    /// yellowing, and winter pale, flattened and mostly dry. A number between two blends them.
    /// </remarks>
    public float Season { get; init; } = GrassCondition.Autumn;

    /// <summary>
    /// The health, from 0 parched up to 1 lush, which sets how many blades are dry and yellowed
    /// and how far the green is drawn toward the dry color, lush unless given.
    /// </summary>
    public float Health { get; init; } = GrassCondition.Lush;

    /// <summary>
    /// The thickness, from 0 sparse with soil showing between a few tufts up to 1 a closed mat,
    /// closed unless given.
    /// </summary>
    public float Thickness { get; init; } = GrassCondition.Closed;

    /// <summary>
    /// How many clumps stand on a square meter, each some twenty blades of a short fine layer and
    /// a longer coarse one for the lawn, or two cards crossed.
    /// </summary>
    public float Density { get; init; } = 800f;

    /// <summary>
    /// How tall a clump stands on average, in meters, its longest blade that long along its curve
    /// and its far card as wide as the set's picture is for that height.
    /// </summary>
    public float Height { get; init; } = 0.3f;

    /// <summary>
    /// How far a clump's height strays from the average either way, as a share of it.
    /// </summary>
    public float Variation { get; init; } = 0.3f;

    /// <summary>The seed the clumps are scattered, turned, sized and swayed by.</summary>
    public uint Seed { get; init; }

    /// <summary>
    /// How far the blades bend over, from 0 standing straight up to 1 the lawn's limp arch whose
    /// outer half lies near level, a bend that grows with a blade's length and its distance from
    /// its clump's middle, 1 unless given.
    /// </summary>
    public float Droop { get; init; } = 1f;

    /// <summary>
    /// How far the height and the coarseness of the grass vary from patch to patch, from nothing
    /// up to the whole, a slow noise over the place that leaves lush mounds beside shorter
    /// stretches, 0.5 unless given.
    /// </summary>
    public float PatchVariation { get; init; } = 0.5f;

    /// <summary>
    /// The size of the mounds the patch variation makes, in meters, a half of it to twice it
    /// across, 1.2 unless given.
    /// </summary>
    public float MoundSize { get; init; } = 1.2f;

    /// <summary>
    /// The share of a clump's blades that are the fine short layer under the coarse tufts, from
    /// nothing up to under the whole, 0.85 unless given.
    /// </summary>
    public float FineShare { get; init; } = 0.85f;

    /// <summary>
    /// The share of the blades that stand upright whatever the droop, the few, 0.1 unless given.
    /// </summary>
    public float UprightShare { get; init; } = 0.1f;

    /// <summary>
    /// The color a green blade's picture is tinted to at its root, in linear sRGB, or the kind's
    /// own deep green where it is null.
    /// </summary>
    public Vector3? RootColor { get; init; }

    /// <summary>
    /// The color a green blade's picture is tinted to at its tip, in linear sRGB, or the kind's
    /// own lighter yellow green where it is null.
    /// </summary>
    public Vector3? TipColor { get; init; }

    /// <summary>
    /// The share of the blades that are dry and yellowed in summer and lush health, from nothing
    /// up to the whole, which take <see cref="DryColor"/> in place of the two colors above, the
    /// season and the health adding theirs, 0.04 unless given.
    /// </summary>
    public float DryShare { get; init; } = 0.04f;

    /// <summary>
    /// The color of a dry blade at its tip, in linear sRGB, and four fifths of it at its root, or
    /// the kind's own straw where it is null.
    /// </summary>
    public Vector3? DryColor { get; init; }

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

    /// <summary>Blends two settings by a share, the numbers by their value.</summary>
    /// <param name="from">The settings at a share of nothing.</param>
    /// <param name="to">The settings at a share of one.</param>
    /// <param name="share">How far from <paramref name="from"/> to <paramref name="to"/>.</param>
    /// <returns>
    /// Settings whose season, health, thickness, density, height, variation, droop, patch
    /// variation, mound size, layer shares, dry share and color variation are between the two, and
    /// whose colors are between where both give one, the kind, the set, the seed and the patch size
    /// those of <paramref name="from"/> below a share of a half and of <paramref name="to"/> from
    /// it.
    /// </returns>
    /// <remarks>
    /// The season blends as a plain number, so a blend from winter at 3 to spring at 0 passes
    /// through autumn and summer, and a host that needs it through the turn of the year gives the
    /// spring as 4, which the season reads round the cycle.
    /// </remarks>
    public static GrassSettings Blend(GrassSettings from, GrassSettings to, float share)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var near = share < 0.5f ? from : to;
        return near with
        {
            Season = Mix(from.Season, to.Season, share),
            Health = Mix(from.Health, to.Health, share),
            Thickness = Mix(from.Thickness, to.Thickness, share),
            Density = Mix(from.Density, to.Density, share),
            Height = Mix(from.Height, to.Height, share),
            Variation = Mix(from.Variation, to.Variation, share),
            Droop = Mix(from.Droop, to.Droop, share),
            PatchVariation = Mix(from.PatchVariation, to.PatchVariation, share),
            MoundSize = Mix(from.MoundSize, to.MoundSize, share),
            FineShare = Mix(from.FineShare, to.FineShare, share),
            UprightShare = Mix(from.UprightShare, to.UprightShare, share),
            DryShare = Mix(from.DryShare, to.DryShare, share),
            ColorVariation = Mix(from.ColorVariation, to.ColorVariation, share),
            RootColor = Mix(from.RootColor, to.RootColor, share, near.RootColor),
            TipColor = Mix(from.TipColor, to.TipColor, share, near.TipColor),
            DryColor = Mix(from.DryColor, to.DryColor, share, near.DryColor),
        };
    }

    private static float Mix(float from, float to, float share) => from * (1f - share) + to * share;

    private static Vector3? Mix(Vector3? from, Vector3? to, float share, Vector3? otherwise) =>
        from is { } a && to is { } b ? Vector3.Lerp(a, b, share) : otherwise;
}
