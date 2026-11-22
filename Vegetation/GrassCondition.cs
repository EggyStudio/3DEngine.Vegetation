namespace Vegetation;

/// <summary>
/// The named values of a stretch's season, health and thickness, numbers a host sets as they are
/// or blends between, as <see cref="GrassSettings.Season"/>, <see cref="GrassSettings.Health"/>
/// and <see cref="GrassSettings.Thickness"/> take them.
/// </summary>
/// <remarks>
/// The season is a number round a cycle of four, so 1.5 is between summer and autumn and 3.5 is
/// between winter and the spring after it, and the health and the thickness run from nothing to
/// one. Each number blends by its value, so a host eases a stretch from one named value to the next
/// by moving the number, and <see cref="GrassSettings.Blend"/> blends two whole settings.
/// </remarks>
public static class GrassCondition
{
    /// <summary>Spring, fresh and bright, its blades short and upright.</summary>
    public const float Spring = 0f;

    /// <summary>Summer, full and deep green, the blades at their height.</summary>
    public const float Summer = 1f;

    /// <summary>Autumn, the blades at their longest and limp, a few going yellow.</summary>
    public const float Autumn = 2f;

    /// <summary>Winter, pale and flattened, the blades lying and mostly dry.</summary>
    public const float Winter = 3f;

    /// <summary>The health of grass parched, many of its blades dry and yellowed.</summary>
    public const float Parched = 0f;

    /// <summary>The health of grass lush, green to the tips with few dry blades.</summary>
    public const float Lush = 1f;

    /// <summary>The thickness of a sparse stand with soil showing between its tufts.</summary>
    public const float Sparse = 0f;

    /// <summary>The thickness of a closed mat, no soil seen from above.</summary>
    public const float Closed = 1f;
}
