namespace Vegetation;

/// <summary>
/// The kind of grass a stretch is planted with, which picks its palette and the widths of its
/// blades, as <see cref="GrassSettings.Kind"/> names it.
/// </summary>
/// <remarks>
/// The kind does not set the habit, which <see cref="GrassSettings.Lawn"/>,
/// <see cref="GrassSettings.Meadow"/> and <see cref="GrassSettings.Wheat"/> set with their numbers
/// beside it, so a host changes a lawn's droop or height with a <c>with</c> and the kind stays what
/// colors it.
/// </remarks>
public enum GrassKind
{
    /// <summary>A fine meadow grass of a lawn left unmown, bright green and limp.</summary>
    Lawn,

    /// <summary>A meadow grass that stands upright, darker and toward olive.</summary>
    Meadow,

    /// <summary>A tall grass gone pale gold, the wheat of a field.</summary>
    Wheat,
}
