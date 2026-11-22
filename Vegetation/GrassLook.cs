using System.Numerics;

namespace Vegetation;

// What a settings means for the blades, resolved once: the colors the kind and the season and the
// health give, how many blades are dry, how far they droop, how tall they stand and how wide each
// layer is. Everything here is a plain function of the settings, so a host that moves the season
// or the health moves every number with it.
internal readonly record struct GrassLook(
    Vector3 Root,
    Vector3 Tip,
    Vector3 Dry,
    float DryShare,
    float Droop,
    float Flat,
    float HeightScale,
    float FineShare,
    float FineLow,
    float FineHigh,
    float WidthLow,
    float WidthHigh)
{
    // The four seasons' numbers, spring first, read round a cycle: how tall the blades stand as a
    // share of the clump's height, how far they droop as a share of the settings' droop, how
    // flattened they lie and how many more are dry.
    private static readonly float[] SeasonHeight = [0.7f, 0.9f, 1f, 0.75f];
    private static readonly float[] SeasonDroop = [0.75f, 0.9f, 1f, 1f];
    private static readonly float[] SeasonFlat = [0f, 0f, 0f, 1f];
    private static readonly float[] SeasonDry = [0f, 0f, 0.02f, 0.6f];

    // The colors of each kind in each season as sRGB, the root's, the tip's and the dry blade's,
    // spring first.
    private static readonly (Vector3 Root, Vector3 Tip, Vector3 Dry)[][] Palettes =
    [
        // The lawn, bright fresh green that is lighter and more yellow toward its tips.
        [
            (new(0.20f, 0.40f, 0.08f), new(0.58f, 0.78f, 0.20f), new(0.62f, 0.55f, 0.28f)),
            (new(0.14f, 0.32f, 0.07f), new(0.46f, 0.66f, 0.18f), new(0.62f, 0.55f, 0.28f)),
            (new(0.18f, 0.38f, 0.08f), new(0.55f, 0.75f, 0.20f), new(0.62f, 0.55f, 0.28f)),
            (new(0.34f, 0.40f, 0.16f), new(0.66f, 0.68f, 0.38f), new(0.70f, 0.64f, 0.42f)),
        ],
        // The meadow, darker and toward olive.
        [
            (new(0.16f, 0.33f, 0.07f), new(0.50f, 0.68f, 0.20f), new(0.55f, 0.47f, 0.20f)),
            (new(0.12f, 0.27f, 0.06f), new(0.42f, 0.60f, 0.17f), new(0.55f, 0.47f, 0.20f)),
            (new(0.18f, 0.28f, 0.07f), new(0.52f, 0.55f, 0.18f), new(0.55f, 0.47f, 0.20f)),
            (new(0.36f, 0.36f, 0.17f), new(0.66f, 0.62f, 0.36f), new(0.68f, 0.60f, 0.36f)),
        ],
        // The wheat, green and gold in spring and pale gold from summer on.
        [
            (new(0.25f, 0.40f, 0.10f), new(0.60f, 0.70f, 0.25f), new(0.80f, 0.68f, 0.38f)),
            (new(0.50f, 0.42f, 0.20f), new(0.82f, 0.72f, 0.42f), new(0.80f, 0.68f, 0.38f)),
            (new(0.55f, 0.45f, 0.20f), new(0.85f, 0.74f, 0.42f), new(0.80f, 0.68f, 0.38f)),
            (new(0.60f, 0.52f, 0.30f), new(0.82f, 0.74f, 0.50f), new(0.78f, 0.70f, 0.50f)),
        ],
    ];

    // Refuses a number outside what the settings' paper allows, as the argument it came in.
    public static void Check(GrassSettings settings)
    {
        static bool Unit(float value) => value >= 0f && value <= 1f;
        if (!Unit(settings.ColorVariation))
            throw new ArgumentException("Blades stray in color from nothing up to the whole.", nameof(settings));
        if (!Unit(settings.DryShare))
            throw new ArgumentException("A share of the blades is dry from none up to all.", nameof(settings));
        if (!(Unit(settings.Health) && Unit(settings.Thickness) && Unit(settings.Droop) && Unit(settings.PatchVariation) && Unit(settings.UprightShare)))
            throw new ArgumentException("The health, the thickness, the droop, the patch variation and the upright share run from nothing up to one.", nameof(settings));
        if (!(settings.FineShare >= 0f && settings.FineShare < 1f))
            throw new ArgumentException("The fine layer's share runs from nothing up to less than the whole.", nameof(settings));
        if (!(settings.MoundSize > 0f) || !float.IsFinite(settings.MoundSize) || !float.IsFinite(settings.Season))
            throw new ArgumentException("The mounds have a size above nothing and the season is a number.", nameof(settings));
    }

    public static GrassLook Of(GrassSettings settings)
    {
        Check(settings);
        var (season, next, along) = Cycle(settings.Season);
        var palette = Palettes[(int)settings.Kind];
        var (root, tip, dry) = (Mix(palette[season].Root, palette[next].Root, along),
            Mix(palette[season].Tip, palette[next].Tip, along), Mix(palette[season].Dry, palette[next].Dry, along));
        var green = (settings.RootColor, settings.TipColor);
        var dryColor = TreeSpecies.Srgb(dry.X, dry.Y, dry.Z);
        var (rootColor, tipColor) = (green.RootColor ?? TreeSpecies.Srgb(root.X, root.Y, root.Z), green.TipColor ?? TreeSpecies.Srgb(tip.X, tip.Y, tip.Z));
        dryColor = settings.DryColor ?? dryColor;
        // Unhealthy grass is drawn toward the dry color even where it is green.
        var wither = 0.35f * (1f - settings.Health);
        rootColor = Vector3.Lerp(rootColor, dryColor * 0.8f, wither);
        tipColor = Vector3.Lerp(tipColor, dryColor, wither);
        var dryShare = Math.Clamp(settings.DryShare + Mix(SeasonDry, season, next, along) + 0.85f * (1f - settings.Health), 0f, 1f);
        var (fineLow, fineHigh, low, high) = settings.Kind switch
        {
            GrassKind.Lawn => (0.006f, 0.010f, 0.007f, 0.011f),
            GrassKind.Meadow => (0.003f, 0.005f, 0.003f, 0.008f),
            _ => (0.004f, 0.006f, 0.005f, 0.009f),
        };
        return new GrassLook(
            rootColor, tipColor, dryColor, dryShare,
            Math.Clamp(settings.Droop * Mix(SeasonDroop, season, next, along), 0f, 1f),
            Mix(SeasonFlat, season, next, along), Mix(SeasonHeight, season, next, along),
            settings.FineShare, fineLow, fineHigh, low, high);
    }

    // The two seasons a number falls between, round the cycle of four, and how far along.
    private static (int Season, int Next, float Along) Cycle(float season)
    {
        var wrapped = season - MathF.Floor(season / 4f) * 4f;
        var whole = Math.Min((int)MathF.Floor(wrapped), 3);
        return (whole, (whole + 1) % 4, wrapped - whole);
    }

    private static float Mix(float[] values, int season, int next, float along) =>
        values[season] + (values[next] - values[season]) * along;

    private static Vector3 Mix(Vector3 from, Vector3 to, float along) => Vector3.Lerp(from, to, along);
}
