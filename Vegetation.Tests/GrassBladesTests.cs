using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The blades of <see cref="GrassBlades"/>, the same from the same clump, a clump's coarse blades
/// and its fine ones within the ranges the paper gives, the width's profile, the bend of the lawn,
/// the cover of its mat from above, the season, the health and the thickness each moving what they
/// should, their coordinates in the boxes of their looks, their tints and dry share, and a patch's
/// arrays of the sizes their segments make, facing the way their normals look, with the lawn, the
/// meadow and the wheat as settings.
/// </summary>
public sealed class GrassBladesTests
{
    private static readonly Vector2 Patch = new(1.5f, 1.5f);

    private static List<GrassBlade> Grow(GrassSettings settings, Vector2 size, uint seed, Vector2 middle = default) =>
        GrassScatter.Clumps(settings, size, seed).SelectMany(clump => GrassBlades.Of(clump, settings, middle)).ToList();

    // The share of vertical rays over the middle of a patch that pass between every blade and reach
    // the ground, from a grid of rays that leave out the patch's border, where blades of the next
    // patch would stand.
    private static float Missed(GrassMesh mesh, Vector2 size)
    {
        const float cell = 0.05f;
        var grid = new Dictionary<(int, int), List<int>>();
        for (var at = 0; at < mesh.Indices.Count; at += 3)
        {
            var (a, b, c) = (mesh.Positions[(int)mesh.Indices[at]], mesh.Positions[(int)mesh.Indices[at + 1]], mesh.Positions[(int)mesh.Indices[at + 2]]);
            var (x0, x1) = ((int)MathF.Floor(MathF.Min(a.X, MathF.Min(b.X, c.X)) / cell), (int)MathF.Floor(MathF.Max(a.X, MathF.Max(b.X, c.X)) / cell));
            var (z0, z1) = ((int)MathF.Floor(MathF.Min(a.Z, MathF.Min(b.Z, c.Z)) / cell), (int)MathF.Floor(MathF.Max(a.Z, MathF.Max(b.Z, c.Z)) / cell));
            for (var x = x0; x <= x1; x++)
                for (var z = z0; z <= z1; z++)
                {
                    if (!grid.TryGetValue((x, z), out var list))
                        grid[(x, z)] = list = [];
                    list.Add(at);
                }
        }

        var (missed, total) = (0, 0);
        const int steps = 64;
        for (var column = 0; column < steps; column++)
            for (var row = 0; row < steps; row++)
            {
                var (x, z) = (-0.3f + 0.6f * (column + 0.5f) / steps, -0.3f + 0.6f * (row + 0.5f) / steps);
                total++;
                var hit = false;
                if (grid.TryGetValue(((int)MathF.Floor(x / cell), (int)MathF.Floor(z / cell)), out var list))
                    foreach (var at in list)
                    {
                        var (a, b, c) = (mesh.Positions[(int)mesh.Indices[at]], mesh.Positions[(int)mesh.Indices[at + 1]], mesh.Positions[(int)mesh.Indices[at + 2]]);
                        var d1 = (x - b.X) * (a.Z - b.Z) - (a.X - b.X) * (z - b.Z);
                        var d2 = (x - c.X) * (b.Z - c.Z) - (b.X - c.X) * (z - c.Z);
                        var d3 = (x - a.X) * (c.Z - a.Z) - (c.X - a.X) * (z - a.Z);
                        if (!((d1 < 0f || d2 < 0f || d3 < 0f) && (d1 > 0f || d2 > 0f || d3 > 0f)))
                        {
                            hit = true;
                            break;
                        }
                    }

                if (!hit)
                    missed++;
            }

        return missed / (float)total;
    }

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
    /// Every clump of the lawn holds three to five coarse blades and fine ones after them, each
    /// within the paper's ranges of width, segments, height, lean, curl and facing, on the ground,
    /// with colors that are not negative, and all of three to five coarse blades are met.
    /// </summary>
    [Fact]
    public void AClumpHoldsThreeToFiveCoarseBladesAndFineOnesWithinTheirRanges()
    {
        var settings = new GrassSettings();
        var counts = new HashSet<int>();
        foreach (var clump in GrassScatter.Clumps(settings, Patch, 7))
        {
            var blades = GrassBlades.Of(clump, settings);
            var coarse = blades.TakeWhile(blade => !blade.Fine).Count();
            counts.Add(coarse);
            Assert.InRange(coarse, 3, 5);
            Assert.All(blades.Skip(coarse), blade => Assert.True(blade.Fine));
            foreach (var blade in blades)
            {
                Assert.True(blade.Root.Y == 0f);
                Assert.InRange(blade.Width, blade.Fine ? 0.005f : 0.006f, blade.Fine ? 0.0115f : 0.0127f);
                Assert.InRange(blade.Look, 0, GrassBlades.Looks.Count - 1);
                Assert.InRange(blade.Segments, blade.Fine ? 2 : 3, 5);
                Assert.InRange(blade.Height, clump.Height * 0.15f, clump.Height);
                Assert.True(blade.Fine ? blade.Height <= clump.Height * 0.5f : blade.Height >= clump.Height * 0.4f);
                Assert.InRange(blade.Lean, 0f, 1.6f);
                Assert.InRange(blade.Curl, 0f, 3f);
                Assert.InRange(blade.Facing, 0f, MathF.Tau);
                Assert.True(blade.RootColor.X >= 0f && blade.RootColor.Y >= 0f && blade.RootColor.Z >= 0f);
                Assert.True(blade.TipColor.X >= 0f && blade.TipColor.Y >= 0f && blade.TipColor.Z >= 0f);
            }
        }

        Assert.Equal([3, 4, 5], counts.Order());
    }

    /// <summary>
    /// The fine layer is the larger share of the blades, near the share the settings give, and a
    /// share of nothing leaves only coarse blades.
    /// </summary>
    [Fact]
    public void TheFineLayerIsTheShareOfTheBladesTheSettingsGive()
    {
        var blades = Grow(new GrassSettings(), Patch, 7);
        Assert.InRange(blades.Count(blade => blade.Fine) / (float)blades.Count, 0.7f, 0.86f);
        Assert.All(Grow(new GrassSettings { FineShare = 0f }, Patch, 7), blade => Assert.False(blade.Fine));
    }

    /// <summary>
    /// A blade is whole in width up to four fifths of its length and narrows in a straight line to
    /// nothing at its point, in the numbers and in the mesh, where the edges of a row stand
    /// <c>width</c> times that share and the cosine of the bend of the edges apart.
    /// </summary>
    [Fact]
    public void ABladeIsEquallyWideForMostOfItsLengthAndNarrowsOverTheLastFifth()
    {
        Assert.Equal(1f, GrassBlades.WidthShare(0f));
        Assert.Equal(1f, GrassBlades.WidthShare(0.8f));
        Assert.Equal(0.5f, GrassBlades.WidthShare(0.9f), 4);
        Assert.Equal(0f, GrassBlades.WidthShare(1f));
        Assert.Equal(0f, GrassBlades.WidthShare(1.2f));
        for (var along = 0.05f; along < 0.8f; along += 0.05f)
            Assert.Equal(1f, GrassBlades.WidthShare(along));

        var settings = new GrassSettings { Density = 1f };
        var mesh = GrassBlades.Write(settings, Vector2.One, 2, 5);
        var first = 0;
        foreach (var blade in Grow(settings, Vector2.One, 2))
        {
            var rows = blade.Segments;
            for (var row = 0; row < rows; row++)
            {
                var wide = (mesh.Positions[first + 3 * row + 2] - mesh.Positions[first + 3 * row]).Length();
                Assert.Equal(blade.Width * 0.8253356f * GrassBlades.WidthShare(row / (float)rows), wide, 3);
            }

            first += 3 * rows + 1;
        }
    }

    /// <summary>
    /// Nine in ten of the lawn's blades end with their tips under half their length in height,
    /// since they bend over, and only the few that the upright share keeps stand taller, where the
    /// upright meadow's settings leave most of the blades with their tips high.
    /// </summary>
    [Fact]
    public void MostOfTheLawnsTipsLieUnderHalfTheirLengthInHeight()
    {
        static float Low(IEnumerable<GrassBlade> blades) =>
            blades.Count(blade => blade.TipHeight < 0.5f * blade.Height) / (float)blades.Count();

        var lawn = Grow(new GrassSettings(), Patch, 7);
        Assert.True(Low(lawn) >= 0.85f, $"{Low(lawn)} of the lawn's blades are low");
        Assert.True(Low(lawn) < 1f);
        var upright = Grow(new GrassSettings { Droop = 0f, UprightShare = 0f }, Patch, 7);
        Assert.True(Low(upright) < 0.2f, $"{Low(upright)} of the upright blades are low");
        Assert.True(Low(Grow(new GrassSettings { UprightShare = 0.5f }, Patch, 7)) < Low(lawn));
    }

    /// <summary>
    /// Fewer than one vertical ray in fifty from above reaches the ground in a default patch, the
    /// underfill closing the ground, where a thin thickness leaves the soil showing.
    /// </summary>
    [Fact]
    public void NoMoreThanOneRayInFiftyFromAboveReachesTheGroundInADefaultPatch()
    {
        var settings = new GrassSettings();
        foreach (uint seed in new uint[] { 5, 6 })
        {
            var missed = Missed(GrassBlades.Write(settings, Patch, seed, 5), Patch);
            Assert.True(missed <= 0.02f, $"{missed} of the rays reach the ground at seed {seed}");
        }

        var sparse = Missed(GrassBlades.Write(settings with { Thickness = 0f }, Patch, 5, 5), Patch);
        var half = Missed(GrassBlades.Write(settings with { Thickness = 0.5f }, Patch, 5, 5), Patch);
        Assert.True(sparse > 0.2f && sparse > half && half > 0.02f, $"{sparse} and {half} of the rays reach sparse ground");
    }

    /// <summary>
    /// A blade's whole curve and its width stay inside its clump's reach along the ground and its
    /// height, for the fine blades with their wider foot and the coarse ones, at any droop.
    /// </summary>
    [Fact]
    public void ABladeStaysInsideItsClumpsReach()
    {
        foreach (var height in new[] { 0.3f, 0.9f })
            foreach (var droop in new[] { 0f, 1f })
            {
                var settings = new GrassSettings { Density = 1f, Height = height, Variation = 0.5f, Droop = droop };
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
    /// The thickness thins the blades, a closed mat holding the most, a half fewer and a sparse
    /// stretch bare in places with soil showing, none of it growing more as it thickens.
    /// </summary>
    [Fact]
    public void TheThicknessThinsTheBladesAndLeavesTheSoilShowing()
    {
        var counts = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f }
            .Select(thickness => Grow(new GrassSettings { Thickness = thickness }, Patch, 7).Count).ToArray();
        Assert.Equal(counts.Order(), counts);
        Assert.True(counts[0] < 0.2f * counts[4], $"{counts[0]} blades of {counts[4]}");
        var bare = GrassScatter.Clumps(new GrassSettings { Thickness = 0f }, Patch, 7)
            .Count(clump => GrassBlades.Of(clump, new GrassSettings { Thickness = 0f }).Count == 0);
        Assert.True(bare > 0);
        Assert.All(GrassScatter.Clumps(new GrassSettings(), Patch, 7), clump => Assert.NotEmpty(GrassBlades.Of(clump, new GrassSettings())));
    }

    /// <summary>
    /// The patch variation raises and lowers the grass in mounds about a meter across that run on
    /// over the patches' borders, where none leaves every clump the same, and a clump's blades are
    /// the same wherever its patch's middle puts it on the ground.
    /// </summary>
    [Fact]
    public void ThePatchVariationMakesMoundsThatRunOnOverTheBorders()
    {
        static float Spread(IEnumerable<GrassBlade> blades) =>
            blades.GroupBy(blade => ((int)MathF.Floor(blade.Root.X / 0.6f), (int)MathF.Floor(blade.Root.Z / 0.6f)))
                .Select(group => group.Where(blade => !blade.Fine).Average(blade => blade.Height)).ToArray() is var means
                ? means.Max() - means.Min() : 0f;

        var wide = new Vector2(4f, 4f);
        var plain = Spread(Grow(new GrassSettings { PatchVariation = 0f, Variation = 0f }, wide, 7));
        var varied = Spread(Grow(new GrassSettings { PatchVariation = 1f, Variation = 0f }, wide, 7));
        Assert.True(varied > 2f * plain + 0.02f, $"{varied} against {plain}");
        var settings = new GrassSettings();
        var clump = GrassScatter.Clumps(settings, Patch, 7)[0];
        Assert.Equal(GrassBlades.Of(clump, settings, new Vector2(3f, 1f)), GrassBlades.Of(clump, settings, new Vector2(3f, 1f)));
        Assert.NotEqual(GrassBlades.Of(clump, settings, new Vector2(3f, 1f)), GrassBlades.Of(clump, settings));
    }

    /// <summary>
    /// The seasons run spring short and fresh, summer deep green, autumn longest and winter pale,
    /// flattened and dry, a number between two blends them and the cycle closes at 4.
    /// </summary>
    [Fact]
    public void TheSeasonsMoveTheLengthTheColorAndTheDryShare()
    {
        static (float Height, float Dry, float Green) Of(float season)
        {
            var blades = Grow(new GrassSettings { Season = season, ColorVariation = 0f }, Patch, 7);
            var height = blades.Where(blade => !blade.Fine).Average(blade => blade.Height);
            var dry = blades.Count(blade => blade.Dry) / (float)blades.Count;
            var green = blades.Where(blade => !blade.Dry).Average(blade => blade.TipColor.Y / (blade.TipColor.X + 0.01f));
            return (height, dry, green);
        }

        var (spring, summer, autumn, winter) = (Of(GrassCondition.Spring), Of(GrassCondition.Summer), Of(GrassCondition.Autumn), Of(GrassCondition.Winter));
        Assert.True(spring.Height < summer.Height && summer.Height < autumn.Height && winter.Height < autumn.Height);
        Assert.True(winter.Dry > 0.4f && autumn.Dry < 0.15f && spring.Dry < 0.1f);
        Assert.True(winter.Green < autumn.Green, "winter blades are paler and yellower");
        var between = Of(2.5f);
        Assert.True(between.Height < autumn.Height && between.Height > winter.Height);
        Assert.Equal(Of(GrassCondition.Spring), Of(4f));
        Assert.Equal(Of(GrassCondition.Summer), Of(5f));
    }

    /// <summary>
    /// The health moves the dry and yellowed share from the settings' own at lush up to nearly all
    /// of the blades at parched, and draws the green that remains toward the dry color.
    /// </summary>
    [Fact]
    public void TheHealthMovesTheDryShareAndTheColor()
    {
        float Dry(float health) =>
            Grow(new GrassSettings { Health = health }, Patch, 7).Count(blade => blade.Dry) / (float)Grow(new GrassSettings { Health = health }, Patch, 7).Count;

        Assert.True(Dry(1f) < 0.12f && Dry(0f) > 0.8f && Dry(0.5f) > Dry(1f) && Dry(0.5f) < Dry(0f));
        var (lush, parched) = (new GrassSettings { Health = 1f, DryShare = 0f, ColorVariation = 0f }, new GrassSettings { Health = 0f, DryShare = 0f, ColorVariation = 0f });
        var clump = GrassScatter.Clumps(lush, Patch, 7)[0];
        var green = GrassBlades.Of(clump, lush).First(blade => !blade.Dry).TipColor;
        var drawn = GrassBlades.Of(clump, parched).First(blade => !blade.Dry).TipColor;
        Assert.True(drawn.X / drawn.Y > green.X / green.Y, "the parched green is yellower");
    }

    /// <summary>
    /// The settings left alone plant the lawn, in autumn, healthy and thick, and the meadow and the
    /// wheat are presets of their own kinds, the wheat taller and paler than the meadow, the
    /// meadow upright where the lawn is limp.
    /// </summary>
    [Fact]
    public void TheDefaultIsTheLawnAndTheMeadowAndTheWheatArePresets()
    {
        var (lawn, meadow, wheat) = (new GrassSettings(), GrassSettings.Meadow, GrassSettings.Wheat);
        Assert.Equal(lawn, GrassSettings.Lawn);
        Assert.Equal((GrassKind.Lawn, GrassCondition.Autumn, GrassCondition.Lush, GrassCondition.Closed), (lawn.Kind, lawn.Season, lawn.Health, lawn.Thickness));
        Assert.Equal(0.3f, lawn.Height);
        Assert.Equal(GrassKind.Meadow, meadow.Kind);
        Assert.Equal(GrassKind.Wheat, wheat.Kind);
        Assert.True(wheat.Height > meadow.Height);
        Assert.True(meadow.Droop < lawn.Droop && meadow.Density < lawn.Density);
        Assert.Equal("wheat-grass", wheat.Set);
        Assert.NotEmpty(GrassScatter.Patches(new Vector2(8f, 8f), wheat));
        float Pale(GrassSettings settings) => Grow(settings with { Density = 20f }, Patch, 7).Where(blade => !blade.Dry).Average(blade => blade.TipColor.X + blade.TipColor.Z);
        Assert.True(Pale(wheat with { Season = 1f }) > Pale(meadow with { Season = 1f }));
    }

    /// <summary>
    /// A blend of two settings is between them in every number, the colors only where both give
    /// one, and the kind, the set and the seed those of the nearer.
    /// </summary>
    [Fact]
    public void ABlendIsBetweenTheTwoSettings()
    {
        var (from, to) = (GrassSettings.Lawn with { Seed = 1, RootColor = Vector3.Zero }, GrassSettings.Wheat with { Seed = 2, RootColor = Vector3.One });
        var half = GrassSettings.Blend(from, to, 0.5f);
        Assert.Equal(0.5f * (from.Height + to.Height), half.Height, 4);
        Assert.Equal(0.5f * (from.Season + to.Season), half.Season, 5);
        Assert.Equal(0.5f * (from.Density + to.Density), half.Density, 3);
        Assert.Equal(new Vector3(0.5f), half.RootColor);
        Assert.Null(half.TipColor);
        Assert.Equal((GrassKind.Wheat, 2u), (half.Kind, half.Seed));
        Assert.Equal((GrassKind.Lawn, 1u), (GrassSettings.Blend(from, to, 0.25f).Kind, GrassSettings.Blend(from, to, 0.25f).Seed));
        Assert.Equal(from.Droop, GrassSettings.Blend(from, to, 0f).Droop);
        Assert.Equal(to.Droop, GrassSettings.Blend(from, to, 1f).Droop);
        Assert.Throws<ArgumentNullException>(() => GrassSettings.Blend(null!, to, 0.5f));
    }

    /// <summary>
    /// A patch's arrays are as long as its blades' segments make them, three vertices to a row and
    /// one to a tip and four triangles between rows and two at the tip, fewer at a smaller count of
    /// segments, the same above the most any blade has, every index naming a vertex.
    /// </summary>
    [Fact]
    public void ThePatchsArraysAreTheSizesItsSegmentsMake()
    {
        var settings = new GrassSettings { Density = 100f };
        var blades = Grow(settings, Patch, 7);
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
        var settings = new GrassSettings { Density = 100f };
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
    /// A blade that has arched over shows its face to the sky, its normal up and not down, where
    /// it rises from the ground the face looks out sideways.
    /// </summary>
    [Fact]
    public void ALimpBladesFaceLooksUpWhereItHasArchedOver()
    {
        var settings = new GrassSettings { Density = 1f };
        var clump = Assert.Single(GrassScatter.Clumps(settings, Vector2.One, 2));
        var mesh = GrassBlades.Write(settings, Vector2.One, 2, 5);
        var first = 0;
        foreach (var blade in GrassBlades.Of(clump, settings))
        {
            var tip = first + 3 * blade.Segments;
            Assert.True(mesh.Normals[tip].Y > 0.3f, $"a blade's tip faces {mesh.Normals[tip]}");
            first = tip + 1;
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
        var settings = new GrassSettings { ColorVariation = 0f, Season = GrassCondition.Summer };
        var blades = Grow(settings, Patch, 7);
        Assert.All(blades, blade => Assert.Equal(blade.Dry, blade.TipColor.X > blade.TipColor.Y));
        Assert.InRange(blades.Count(blade => blade.Dry) / (float)blades.Count, 0.01f, 0.1f);
        var none = settings with { DryShare = 0f };
        var every = settings with { DryShare = 1f };
        Assert.All(Grow(none, Patch, 7), blade => Assert.False(blade.Dry));
        Assert.All(Grow(every, Patch, 7), blade => Assert.True(blade.Dry));
    }

    /// <summary>
    /// A segment count below one, a stretch of no size and a number outside what a setting allows
    /// are refused as the argument they came in.
    /// </summary>
    [Fact]
    public void ASegmentCountOrASettingOutsideWhatItMayBeIsRefused()
    {
        var settings = new GrassSettings();
        var clump = GrassScatter.Clumps(new GrassSettings { Density = 1f }, Vector2.One, 0)[0];
        Assert.Equal("segments", Assert.Throws<ArgumentOutOfRangeException>(() => GrassBlades.Write(settings, Patch, 0, 0)).ParamName);
        Assert.Equal("size", Assert.Throws<ArgumentException>(() => GrassBlades.Write(settings, Vector2.Zero, 0, 5)).ParamName);
        foreach (var refused in new[]
        {
            settings with { ColorVariation = 1.5f }, settings with { ColorVariation = -0.1f },
            settings with { DryShare = 1.5f }, settings with { DryShare = -0.1f },
            settings with { Health = 1.5f }, settings with { Thickness = -0.5f },
            settings with { Droop = 2f }, settings with { PatchVariation = -1f },
            settings with { UprightShare = 1.1f }, settings with { FineShare = 1f },
            settings with { MoundSize = 0f }, settings with { Season = float.NaN },
        })
        {
            Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Of(clump, refused)).ParamName);
            Assert.Equal("settings", Assert.Throws<ArgumentException>(() => GrassBlades.Write(refused, Vector2.One, 0, 5)).ParamName);
        }
    }
}
