using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// A tree's stems as tubes need them, from <see cref="TreeStems"/>, each wrapped by its bark's
/// repeat so its texels keep their shape the length of the stem.
/// </summary>
public sealed class StemTests
{
    private static readonly TreeSpecies[] Presets = [TreeSpecies.QuakingAspen, TreeSpecies.BlackTupelo, TreeSpecies.WeepingWillow, TreeSpecies.CaliforniaBlackOak, TreeSpecies.Bush, TreeSpecies.Fern];

    /// <summary>
    /// Along every segment of every stem of the six presets at seeds 1 and 2, a texel of the bark
    /// is squeezed by no more than a hundredth from the shape its picture gives it, the length one
    /// repeat runs along against the circumference over the count round, in the picture's
    /// proportion of its repeat in meters.
    /// </summary>
    [Fact]
    public void EveryStemKeepsItsBarksTexelsTheirShapeAlongItsLength()
    {
        var worst = (Squeeze: 1f, Where: "");
        foreach (var species in Presets)
        foreach (var seed in (uint[])[1, 2])
        {
            var repeat = species.BarkSet is { } name && TreeTextures.Find(name) is { Meters: { } meters } ? meters : (0.5f, 2f);
            foreach (var stem in TreeStems.Build(TreeSkeleton.Grow(species, seed), species))
            {
                Assert.True(stem.Around >= 1);
                Assert.Equal(stem.Points.Count, stem.Radii.Count);
                Assert.Equal(stem.Points.Count, stem.Along.Count);
                for (var i = 1; i < stem.Points.Count; i++)
                {
                    var length = Vector3.Distance(stem.Points[i], stem.Points[i - 1]);
                    var advance = stem.Along[i] - stem.Along[i - 1];
                    if (length <= 0f) continue;
                    var radius = 0.5f * (stem.Radii[i] + stem.Radii[i - 1]);
                    var round = MathF.Tau * radius / stem.Around / repeat.Item1;
                    var along = length / advance / repeat.Item2;
                    var squeeze = MathF.Max(round / along, along / round);
                    if (squeeze > worst.Squeeze) worst = (squeeze, $"{species.BarkSet ?? "TreeLooks' bark"} at seed {seed}, a stem of {stem.Points.Count} points round {stem.Around}");
                }
            }
        }

        Assert.True(worst.Squeeze <= 1.01f, $"A texel is squeezed {worst.Squeeze:F3} times on {worst.Where}.");
    }

    /// <summary>
    /// A repeat that is not a positive number of meters is refused as the repeat argument.
    /// </summary>
    [Fact]
    public void ARepeatThatIsNotPositiveIsRefused()
    {
        var skeleton = TreeSkeleton.Grow(TreeSpecies.Bush, 1);
        Assert.Equal("repeat", Assert.Throws<ArgumentOutOfRangeException>(() => TreeStems.Build(skeleton, (0f, 1f))).ParamName);
        Assert.Equal("repeat", Assert.Throws<ArgumentOutOfRangeException>(() => TreeStems.Build(skeleton, (1f, float.NaN))).ParamName);
    }
}
