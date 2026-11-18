using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The sky each leaf of a crown sees and its way out, from <see cref="TreeCrown.Shade"/>, over a
/// ball of leaves, with the leaves refused.
/// </summary>
public sealed class TreeCrownTests
{
    // A ball of leaves two meters across its radius, a leaf every quarter meter inside it, each of
    // a fiftieth of a square meter, about the presets' density.
    private static readonly Vector3[] Ball = MakeBall();

    /// <summary>
    /// Every leaf sees a share of the sky from 0 to 1, the leaf at the ball's middle less than any
    /// on the upper half of its rim, and a leaf at the top more than one at the bottom, which the
    /// crown's whole depth stands over.
    /// </summary>
    [Fact]
    public void ALeafDeepInTheCrownSeesLessOfTheSky()
    {
        var (sky, _) = TreeCrown.Shade(Ball, Areas(Ball.Length));
        Assert.Equal(Ball.Length, sky.Length);
        Assert.All(sky, seen => Assert.InRange(seen, 0f, 1f));

        var middle = At(Vector3.Zero);
        var rim = Enumerable.Range(0, Ball.Length).Where(i => Ball[i].Length() > 1.85f && Ball[i].Y > 0f).ToArray();
        Assert.True(sky[middle] < rim.Min(i => sky[i]), $"The middle sees {sky[middle]}, the rim at least {rim.Min(i => sky[i])}.");
        Assert.True(sky[At(new Vector3(0f, 2f, 0f))] > sky[At(new Vector3(0f, -2f, 0f))]);
    }

    /// <summary>
    /// Each leaf's way out is at unit length, and on the ball's rim it points away from the
    /// middle, within a little over 45 degrees.
    /// </summary>
    [Fact]
    public void ALeafsWayOutPointsOutOfTheCrown()
    {
        var (_, outward) = TreeCrown.Shade(Ball, Areas(Ball.Length));
        Assert.All(outward, way => Assert.InRange(way.Length(), 0.999f, 1.001f));
        for (var i = 0; i < Ball.Length; i++)
        {
            if (Ball[i].Length() < 1.85f) continue;
            var along = Vector3.Dot(outward[i], Vector3.Normalize(Ball[i]));
            Assert.True(along > 0.7f, $"The leaf at {Ball[i]} looks out along {outward[i]}, {along} of the way from the middle.");
        }
    }

    /// <summary>
    /// The same leaves shade the same to the bit, and no leaves give two empty arrays.
    /// </summary>
    [Fact]
    public void TheShadeIsTheSameEachTime()
    {
        var first = TreeCrown.Shade(Ball, Areas(Ball.Length));
        var second = TreeCrown.Shade(Ball, Areas(Ball.Length));
        Assert.Equal(first.Sky, second.Sky);
        Assert.Equal(first.Out, second.Out);

        var (noSky, noWays) = TreeCrown.Shade([], []);
        Assert.Empty(noSky);
        Assert.Empty(noWays);
    }

    /// <summary>
    /// A lone leaf, which no crown of a tree is, looks out at unit length, straight up at the
    /// origin, where nothing rounds, and elsewhere leaning toward the low corner its grid is laid
    /// from along each axis where rounding drops it into the voxel below its own, as at 120 meters
    /// from the origin it does along all three; and it sees a share of the sky from 0 to 1.
    /// </summary>
    [Theory]
    [InlineData(0f, 0f, 0f)]
    [InlineData(3f, 5f, -2f)]
    [InlineData(-120.5f, 0.25f, 77f)]
    public void ALoneLeafLooksOutTowardItsGridsLowCorner(float x, float y, float z)
    {
        var (sky, outward) = TreeCrown.Shade([new Vector3(x, y, z)], [0.02f]);
        Assert.InRange(Assert.Single(sky), 0f, 1f);
        var way = Assert.Single(outward);
        Assert.InRange(way.Length(), 0.999f, 1.001f);
        Assert.True(way.X <= 1e-6f && way.Z <= 1e-6f, $"A lone leaf at {(x, y, z)} looks out along {way}.");
        if ((x, y, z) == (0f, 0f, 0f)) Assert.Equal(Vector3.UnitY, way);
    }

    /// <summary>
    /// A place that is not finite is refused as the places argument, and areas not as many as the
    /// places, or one below zero or not finite, as the areas argument.
    /// </summary>
    [Fact]
    public void LeavesThatAreNotLeavesAreRefused()
    {
        Assert.Equal("places", Assert.Throws<ArgumentException>(() => TreeCrown.Shade([new Vector3(float.NaN, 0f, 0f)], [0.02f])).ParamName);
        Assert.Equal("places", Assert.Throws<ArgumentException>(() => TreeCrown.Shade([new Vector3(0f, float.PositiveInfinity, 0f)], [0.02f])).ParamName);
        Assert.Equal("areas", Assert.Throws<ArgumentException>(() => TreeCrown.Shade([Vector3.Zero, Vector3.One], [0.02f])).ParamName);
        Assert.Equal("areas", Assert.Throws<ArgumentException>(() => TreeCrown.Shade([Vector3.Zero], [-0.02f])).ParamName);
        Assert.Equal("areas", Assert.Throws<ArgumentException>(() => TreeCrown.Shade([Vector3.Zero], [float.NaN])).ParamName);
    }

    private static Vector3[] MakeBall()
    {
        var ball = new List<Vector3>();
        for (var x = -8; x <= 8; x++)
        for (var y = -8; y <= 8; y++)
        for (var z = -8; z <= 8; z++)
            if (x * x + y * y + z * z <= 64) ball.Add(new Vector3(x, y, z) * 0.25f);
        return [.. ball];
    }

    private static float[] Areas(int count) => [.. Enumerable.Repeat(0.02f, count)];

    private static int At(Vector3 place) => Array.FindIndex(Ball, leaf => Vector3.Distance(leaf, place) < 1e-4f);
}
