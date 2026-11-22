using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The blades of <see cref="GrassBlades"/>, the same from the same clump, five to nine to a clump,
/// each within its clump's reach and the ranges the paper gives, their coordinates in the boxes of
/// their looks, their tints and dry share, and a patch's arrays of the sizes their segments make,
/// facing the way their normals look, with the meadow and the wheat as settings.
/// </summary>
public sealed class GrassBladesTests
{
    private static readonly Vector2 Patch = new(4f, 3f);

    /// <summary>
    /// The same clump and settings give the same blades and another clump others, so a seed in a
    /// level means the same grass on every runtime.
    /// </summary>
    [Fact]
    public void TheSameClumpGrowsTheSameBladesAndAnotherClumpOthers()
    {
        var clumps = GrassScatter.Clumps(new GrassSettings(), Patch, 7);
        Assert.Equal(GrassBlades.Of(clumps[0], new GrassSettings()), GrassBlades.Of(clumps[0], new GrassSettings()));
        Assert.NotEqual(GrassBlades.Of(clumps[0], new GrassSettings()), GrassBlades.Of(clumps[1], new GrassSettings()));
    }

    /// <summary>
    /// Every clump of a patch holds five to nine blades and all of five to nine are met, each blade
    /// within the paper's ranges of width, segments, height, lean, curl and facing, on the ground,
    /// with colors that are not negative.
    /// </summary>
    [Fact]
    public void AClumpHoldsFiveToNineBladesWithinTheirRanges()
    {
        var settings = new GrassSettings();
        var counts = new HashSet<int>();
        foreach (var clump in GrassScatter.Clumps(settings, Patch, 7))
        {
            var blades = GrassBlades.Of(clump, settings);
            Assert.InRange(blades.Count, 5, 9);
            counts.Add(blades.Count);
            foreach (var blade in blades)
            {
                Assert.True(blade.Root.Y == 0f);
                Assert.InRange(blade.Width, 0.003f, 0.008f);
                Assert.InRange(blade.Look, 0, GrassBlades.Looks.Count - 1);
                Assert.InRange(blade.Segments, 3, 5);
                Assert.InRange(blade.Height, clump.Height * 0.33f, clump.Height);
                Assert.True(blade.Height <= clump.Height * 0.5f || blade.Height >= clump.Height * 0.6f);
                Assert.InRange(blade.Lean, 0f, 0.5f);
                Assert.InRange(blade.Curl, 0f, 0.9f);
                Assert.InRange(blade.Facing, 0f, MathF.Tau);
                Assert.True(blade.RootColor.X >= 0f && blade.RootColor.Y >= 0f && blade.RootColor.Z >= 0f);
                Assert.True(blade.TipColor.X >= 0f && blade.TipColor.Y >= 0f && blade.TipColor.Z >= 0f);
            }
        }

        Assert.Equal([5, 6, 7, 8, 9], counts.Order());
    }

    /// <summary>
    /// Every clump's first blade is tall, and about two in five of the others are low, a third to a
    /// half of the clump's height, so a patch has a body near the ground and tips above it.
    /// </summary>
    [Fact]
    public void AClumpHasATallBladeAndSomeLowOnes()
    {
        var settings = new GrassSettings();
        var (low, others) = (0, 0);
        foreach (var clump in GrassScatter.Clumps(settings, Patch, 7))
        {
            var blades = GrassBlades.Of(clump, settings);
            Assert.True(blades[0].Height >= clump.Height * 0.6f);
            foreach (var blade in blades.Skip(1))
            {
                others++;
                if (blade.Height <= clump.Height * 0.5f)
                    low++;
            }
        }

        Assert.InRange(low / (float)others, 0.3f, 0.5f);
    }

    /// <summary>
    /// A clump alone on a patch has every vertex of its blades within its reach of its root along
    /// the ground and no higher than its height, and its blades' roots in its foot.
    /// </summary>
    [Fact]
    public void ABladeStaysInsideItsClumpsReach()
    {
        foreach (var height in new[] { 0.3f, 0.9f })
        {
            var settings = new GrassSettings { Density = 1f, Height = height, Variation = 0.5f };
            for (uint seed = 0; seed < 40; seed++)
            {
                var clump = Assert.Single(GrassScatter.Clumps(settings, Vector2.One, seed));
                var reach = GrassBlades.Reach(clump);
                foreach (var position in GrassBlades.Write(settings, Vector2.One, seed, 5).Positions)
                {
                    Assert.True(new Vector2(position.X - clump.Root.X, position.Z - clump.Root.Z).Length() <= reach, $"seed {seed} height {height}");
                    Assert.InRange(position.Y, 0f, clump.Height);
                }
            }
        }
    }

    /// <summary>
    /// A patch's arrays are as long as its blades' segments make them, three vertices to a row and
    /// one to a tip and four triangles between rows and two at the tip, fewer at a smaller count of
    /// segments, the same above the most any blade has, every index naming a vertex.
    /// </summary>
    [Fact]
    public void ThePatchsArraysAreTheSizesItsSegmentsMake()
    {
        var settings = new GrassSettings();
        var clumps = GrassScatter.Clumps(settings, Patch, 7);
        var blades = clumps.SelectMany(clump => GrassBlades.Of(clump, settings)).ToList();
        var previous = int.MaxValue;
        foreach (var segments in new[] { 5, 4, 3, 2, 1 })
        {
            var mesh = GrassBlades.Write(settings, Patch, 7, segments);
            var rows = blades.Select(blade => Math.Min(blade.Segments, segments)).ToList();
            Assert.Equal(blades.Count, mesh.Blades);
            Assert.Equal(rows.Sum(row => 3 * row + 1), mesh.Positions.Count);
            Assert.Equal(mesh.Positions.Count, mesh.Normals.Count);
            Assert.Equal(mesh.Positions.Count, mesh.Uvs.Count);
            Assert.Equal(mesh.Positions.Count, mesh.Colors.Count);
            Assert.Equal(rows.Sum(row => 3 * (4 * row - 2)), mesh.Indices.Count);
            Assert.True(mesh.Indices.All(index => index < mesh.Positions.Count));
            Assert.True(mesh.Positions.Count < previous);
            previous = mesh.Positions.Count;
        }

        var most = GrassBlades.Write(settings, Patch, 7, 5);
        Assert.Equal(most.Positions, GrassBlades.Write(settings, Patch, 7, 50).Positions);
        Assert.Equal(most.Indices, GrassBlades.Write(settings, Patch, 7, 50).Indices);
    }

    /// <summary>
    /// The same arguments write the same arrays and another seed others.
    /// </summary>
    [Fact]
    public void TheSameSeedWritesTheSameArraysAndAnotherSeedOthers()
    {
        var settings = new GrassSettings();
        var once = GrassBlades.Write(settings, Patch, 3, 5);
        Assert.Equal(once.Positions, GrassBlades.Write(settings, Patch, 3, 5).Positions);
        Assert.Equal(once.Normals, GrassBlades.Write(settings, Patch, 3, 5).Normals);
        Assert.Equal(once.Uvs, GrassBlades.Write(settings, Patch, 3, 5).Uvs);
        Assert.Equal(once.Colors, GrassBlades.Write(settings, Patch, 3, 5).Colors);
        Assert.NotEqual(once.Positions, GrassBlades.Write(settings, Patch, 4, 5).Positions);
    }

    /// <summary>
    /// Every normal is a unit vector, every triangle is wound to face the way its vertices' normals
    /// look, a blade's edge normals are bent from its middle's across its width, and its colors run
    /// from its root color at its foot to its tip color at its point.
    /// </summary>
    [Fact]
    public void BladesFaceTheirNormalsAreRoundedAndTheirColorsRunFromRootToTip()
    {
        var settings = new GrassSettings { Density = 1f };
        var clump = Assert.Single(GrassScatter.Clumps(settings, Vector2.One, 2));
        var blades = GrassBlades.Of(clump, settings);
        var mesh = GrassBlades.Write(settings, Vector2.One, 2, 5);
        foreach (var normal in mesh.Normals)
            Assert.InRange(normal.Length(), 0.999f, 1.001f);

        for (var at = 0; at < mesh.Indices.Count; at += 3)
        {
            var (a, b, c) = (mesh.Positions[(int)mesh.Indices[at]], mesh.Positions[(int)mesh.Indices[at + 1]], mesh.Positions[(int)mesh.Indices[at + 2]]);
            var face = Vector3.Cross(b - a, c - a);
            var look = mesh.Normals[(int)mesh.Indices[at]] + mesh.Normals[(int)mesh.Indices[at + 1]] + mesh.Normals[(int)mesh.Indices[at + 2]];
            Assert.True(Vector3.Dot(face, look) > 0f, $"triangle {at / 3} faces away from its normals");
        }

        var first = 0;
        foreach (var blade in blades)
        {
            var rows = blade.Segments;
            var (left, middle, right) = (mesh.Normals[first], mesh.Normals[first + 1], mesh.Normals[first + 2]);
            Assert.True(Vector3.Dot(left, middle) < 0.9f && Vector3.Dot(right, middle) < 0.9f && Vector3.Dot(left, right) < Vector3.Dot(left, middle));
            Assert.Equal(blade.RootColor, mesh.Colors[first + 1]);
            Assert.Equal(blade.TipColor, mesh.Colors[first + 3 * rows]);
            Assert.Equal(blade.Root, mesh.Positions[first + 1]);
            first += 3 * rows + 1;
        }
    }

    /// <summary>
    /// Every vertex's coordinates lie in the box of its blade's look, its root at the bottom and
    /// its point at the top, the sides of the box at the edges, and the middle of a row halfway.
    /// </summary>
    [Fact]
    public void ABladesCoordinatesLieInTheBoxOfItsLookRootAtTheBottom()
    {
        var settings = new GrassSettings { Density = 1f };
        var clump = Assert.Single(GrassScatter.Clumps(settings, Vector2.One, 2));
        var mesh = GrassBlades.Write(settings, Vector2.One, 2, 5);
        var first = 0;
        foreach (var blade in GrassBlades.Of(clump, settings))
        {
            var (u, v, width, height) = GrassBlades.Looks[blade.Look];
            var last = first + 3 * blade.Segments;
            Assert.Equal(new Vector2(u, v + height), mesh.Uvs[first]);
            Assert.Equal(new Vector2(u + 0.5f * width, v + height), mesh.Uvs[first + 1]);
            Assert.Equal(new Vector2(u + width, v + height), mesh.Uvs[first + 2]);
            Assert.Equal(new Vector2(u + 0.5f * width, v), mesh.Uvs[last]);
            for (var at = first; at <= last; at++)
            {
                Assert.InRange(mesh.Uvs[at].X, u, u + width);
                Assert.InRange(mesh.Uvs[at].Y, v, v + height);
            }

            first = last + 1;
        }
    }

    /// <summary>
    /// The tint brings the look's mean color to the color wanted, so a dry blade's tip tint is
    /// strongest in red where a green blade's is in green, and the share of dry blades follows the
    /// settings' share.
    /// </summary>
    [Fact]
    public void TheTintsBringTheLooksToTheSettingsColorsAndTheDryShareFollowsTheSettings()
    {
        var settings = new GrassSettings { ColorVariation = 0f };
        var (dry, all) = (0, 0);
        foreach (var clump in GrassScatter.Clumps(settings, Patch, 7))
            foreach (var blade in GrassBlades.Of(clump, settings))
            {
                all++;
                Assert.Equal(blade.Dry, blade.TipColor.X > blade.TipColor.Y);
                dry += blade.Dry ? 1 : 0;
            }

        Assert.InRange(dry / (float)all, 0.05f, 0.16f);
        var none = settings with { DryShare = 0f };
        var every = settings with { DryShare = 1f };
        var clumps = GrassScatter.Clumps(settings, Patch, 7);
        Assert.All(clumps.SelectMany(clump => GrassBlades.Of(clump, none)), blade => Assert.False(blade.Dry));
        Assert.All(clumps.SelectMany(clump => GrassBlades.Of(clump, every)), blade => Assert.True(blade.Dry));
    }

    /// <summary>
    /// A segment count below one, a stretch of no size and a color variation outside nothing up to
    /// the whole are refused as the argument they came in.
    /// </summary>
    [Fact]
    public void ASegmentCountOrASettingOutsideWhatItMayBeIsRefused()
    {
        var settings = new GrassSettings();
        var clump = GrassScatter.Clumps(settings, Vector2.One, 0)[0];
        Assert.Equal("segments", Assert.Throws<ArgumentOutOfRangeException>(() => GrassBlades.Write(settings, Patch, 0, 0)).ParamName);
        Assert.Equal("size", Assert.Throws<ArgumentException>(() => GrassBlades.Write(settings, Vector2.Zero, 0, 5)).ParamName);
        Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Of(clump, settings with { ColorVariation = 1.5f })).ParamName);
        Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Write(settings with { ColorVariation = -0.1f }, Patch, 0, 5)).ParamName);
        Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Of(clump, settings with { DryShare = 1.5f })).ParamName);
        Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Write(settings with { DryShare = -0.1f }, Patch, 0, 5)).ParamName);
    }

    /// <summary>
    /// The settings left alone plant a green meadow 0.3 meters tall, which is the meadow preset,
    /// and the wheat is taller and paler, its tips lighter in red and in blue and less green than
    /// red.
    /// </summary>
    [Fact]
    public void TheDefaultIsAGreenMeadowAndTheWheatIsTallerAndPaler()
    {
        var (meadow, wheat) = (new GrassSettings(), GrassSettings.Wheat);
        Assert.Equal(meadow, GrassSettings.Meadow);
        Assert.Equal(0.3f, meadow.Height);
        Assert.True(meadow.TipColor.Y > meadow.TipColor.X && meadow.TipColor.Y > meadow.TipColor.Z);
        Assert.True(wheat.Height > meadow.Height);
        Assert.True(wheat.TipColor.X > meadow.TipColor.X && wheat.TipColor.Z > meadow.TipColor.Z);
        Assert.True(wheat.TipColor.X > wheat.TipColor.Y);
        Assert.Equal("wheat-grass", wheat.Set);
        Assert.NotEmpty(GrassScatter.Patches(new Vector2(8f, 8f), wheat));
    }
}
