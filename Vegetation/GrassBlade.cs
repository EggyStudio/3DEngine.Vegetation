using System.Numerics;

namespace Vegetation;

/// <summary>
/// A blade of grass, a strip of nearly one width that narrows to a point over the last fifth of
/// its length along a curve, from <see cref="GrassBlades.Of"/>.
/// </summary>
/// <param name="Root">
/// The middle of its foot on the ground, at y 0, from its patch's middle, in meters.
/// </param>
/// <param name="Facing">
/// The way it leans, in radians about the up axis from X toward Z, from zero up to a whole turn.
/// </param>
/// <param name="Height">
/// How long it is along its curve, in meters, which is also as high as it can stand.
/// </param>
/// <param name="Width">
/// How wide it is over most of its length, in meters, from two and a half to nine millimeters
/// by its kind and its layer.
/// </param>
/// <param name="Lean">How far it leans from the up axis at its root, in radians.</param>
/// <param name="Curl">
/// How much further it bends over toward its facing from root to tip, in radians, so its tip leans
/// by <paramref name="Lean"/> and <paramref name="Curl"/> together, past a right angle where it
/// arches over and its tip points down.
/// </param>
/// <param name="Segments">
/// How many segments it is cut into along its curve at the most, from three to five, or three
/// for a fine blade.
/// </param>
/// <param name="Look">
/// Which of <see cref="GrassBlades.Looks"/> its picture is cut from, of three kinds of green
/// blade.
/// </param>
/// <param name="Dry">
/// Whether it is a dry, yellowed blade, which takes the settings' dry color.
/// </param>
/// <param name="Fine">
/// Whether it is of the short fine layer under the longer coarse tufts, which closes the
/// ground.
/// </param>
/// <param name="RootColor">
/// The tint of its picture at the root, a multiplier of the albedo in linear sRGB, which brings the
/// picture's mean to the settings' root color strayed by the seed.
/// </param>
/// <param name="TipColor">
/// The tint of its picture at the tip, as above for the settings' tip color.
/// </param>
/// <remarks>
/// The curve is a quadratic Bezier in the plane of its facing, whose first half leaves the root at
/// <paramref name="Lean"/> and whose second half arrives at the tip at <paramref name="Lean"/> plus
/// <paramref name="Curl"/>, the two arms of its control polygon half the blade's height each, so
/// the curve is the same however many segments it is cut into and a far patch's coarser blade is
/// the same blade.
/// </remarks>
public readonly record struct GrassBlade(
    Vector3 Root,
    float Facing,
    float Height,
    float Width,
    float Lean,
    float Curl,
    int Segments,
    int Look,
    bool Dry,
    bool Fine,
    Vector3 RootColor,
    Vector3 TipColor)
{
    /// <summary>
    /// How high its tip stands above the ground, in meters, which is under half of
    /// <see cref="Height"/> where it has drooped.
    /// </summary>
    public float TipHeight => Root.Y + 0.5f * Height * (MathF.Cos(Lean) + MathF.Cos(Lean + Curl));
}
