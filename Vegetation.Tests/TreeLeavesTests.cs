using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// A grown tree's leaves by
/// <see cref="TreeLeaves.Build(TreeSkeleton, TreeSpecies, uint, TreeDetail)"/>, the same as the
/// fixture an engine's build of them wrote, the same by seed, as many as the paper's within their
/// bound, standing on the twigs and facing up and out, shaded as a crown, swaying with their limbs,
/// and a fern's as fronds along its stems.
/// </summary>
public sealed class TreeLeavesTests
{
    private static readonly TreeSpecies Aspen = TreeSpecies.QuakingAspen;

    private static readonly Dictionary<string, TreeSpecies> Presets = new()
    {
        ["QuakingAspen"] = TreeSpecies.QuakingAspen, ["BlackTupelo"] = TreeSpecies.BlackTupelo,
        ["WeepingWillow"] = TreeSpecies.WeepingWillow, ["CaliforniaBlackOak"] = TreeSpecies.CaliforniaBlackOak,
        ["Bush"] = TreeSpecies.Bush, ["Fern"] = TreeSpecies.Fern,
    };

    /// <summary>
    /// Every array of the leaves of the six presets at four seeds and both levels hashes as the
    /// fixture <c>build/leaves-fixture</c> wrote from the engine's build of them before they moved,
    /// each difference named by its case and its array.
    /// </summary>
    [Fact]
    public void TheLeavesAreTheFixturesToTheBit()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryRoot(), "Vegetation.Tests", "references", "leaves.json")));
        var cases = fixture.RootElement.GetProperty("cases").EnumerateArray().ToList();
        Assert.Equal(6 * 4 * 2, cases.Count);

        var differ = new List<string>();
        foreach (var item in cases)
        {
            var (preset, seed, detail) = (item.GetProperty("preset").GetString()!, item.GetProperty("seed").GetUInt32(), Enum.Parse<TreeDetail>(item.GetProperty("detail").GetString()!));
            var species = Presets[preset];
            var mesh = TreeLeaves.Build(TreeSkeleton.Grow(species, seed), species, seed, detail);
            var made = new Dictionary<string, string>
            {
                ["positions"] = Hash(mesh.Positions), ["normals"] = Hash(mesh.Normals), ["uvs"] = Hash(mesh.Uvs),
                ["sky"] = Hash(mesh.Sky ?? []), ["out"] = Hash(mesh.Out ?? []),
                ["limb"] = Hash(mesh.Limb), ["own"] = Hash(mesh.Own), ["indices"] = Hash(mesh.Indices), ["twigs"] = Hash(mesh.Twigs),
            };
            if (item.GetProperty("vertices").GetInt32() != mesh.Positions.Count) differ.Add($"{preset} {seed} {detail} has {mesh.Positions.Count} vertices");
            differ.AddRange(made.Where(array => item.GetProperty(array.Key).GetString() != array.Value).Select(array => $"{preset} {seed} {detail} {array.Key}"));
        }

        Assert.True(differ.Count == 0, string.Join(Environment.NewLine, differ));
    }

    /// <summary>The same seed places the same leaves, and another seed others.</summary>
    [Fact]
    public void ASeedPlacesTheSameLeavesEachTime()
    {
        var skeleton = TreeSkeleton.Grow(Aspen, 4);
        var first = TreeLeaves.Build(skeleton, Aspen, 4);
        var again = TreeLeaves.Build(skeleton, Aspen, 4);
        Assert.Equal(first.Positions, again.Positions);
        Assert.Equal(first.Uvs, again.Uvs);
        Assert.NotEqual(first.Positions, TreeLeaves.Build(skeleton, Aspen, 5).Positions);
    }

    /// <summary>
    /// The reduced level is every other card at its stalk, twice the area, with its sky, its way
    /// out, its sway and its twig, a fern's fronds are kept whole, and a level that is not one of
    /// <see cref="TreeDetail"/>'s is refused.
    /// </summary>
    [Fact]
    public void AReducedCrownIsEveryOtherLeafAtItsPlaceTwiceTheArea()
    {
        var skeleton = TreeSkeleton.Grow(Aspen, 4);
        var full = TreeLeaves.Build(skeleton, Aspen, 4);
        var reduced = TreeLeaves.Build(skeleton, Aspen, 4, TreeDetail.Reduced);
        var cards = full.Positions.Count / 4;
        Assert.Equal(cards / 2, reduced.Positions.Count / 4);
        Assert.Equal(cards / 2 * 6, reduced.Indices.Count);
        Assert.Equal(cards / 2, reduced.Twigs.Count);

        for (var card = 0; card < cards / 2; card++)
        {
            Assert.True((Stalk(reduced, card) - Stalk(full, 2 * card)).Length() < 1e-4f, $"card {card} stands off its leaf's stalk");
            Assert.InRange(Area(reduced, card) / Area(full, 2 * card), 1.999f, 2.001f);
            Assert.Equal(full.Sky![8 * card], reduced.Sky![4 * card]);
            Assert.Equal(full.Out![8 * card], reduced.Out![4 * card]);
            Assert.Equal(full.Own[8 * card], reduced.Own[4 * card]);
            Assert.Equal(full.Twigs[2 * card], reduced.Twigs[card]);
        }

        var fern = TreeSkeleton.Grow(TreeSpecies.Fern, 4);
        Assert.Equal(TreeLeaves.Build(fern, TreeSpecies.Fern, 4).Positions, TreeLeaves.Build(fern, TreeSpecies.Fern, 4, TreeDetail.Reduced).Positions);
        Assert.Equal("detail", Assert.Throws<ArgumentOutOfRangeException>(() => TreeLeaves.Build(skeleton, Aspen, 4, (TreeDetail)7)).ParamName);
    }

    /// <summary>
    /// Deep in each preset's crown a leaf sees less of the sky than one on its upper rim, every sky
    /// from 0 to 1, and the way out each card carries points out of the crown on its rim.
    /// </summary>
    [Fact]
    public void ALeafDeepInACrownSeesLessOfTheSkyThanOneOnItsRimAndCarriesTheWayOut()
    {
        foreach (var species in new[] { Aspen, TreeSpecies.BlackTupelo, TreeSpecies.WeepingWillow, TreeSpecies.CaliforniaBlackOak, TreeSpecies.Bush })
        {
            var leaves = TreeLeaves.Build(TreeSkeleton.Grow(species, 2), species, 2);
            var cards = leaves.Positions.Count / 4;
            var middles = Enumerable.Range(0, cards).Select(card => (leaves.Positions[card * 4] + leaves.Positions[card * 4 + 1] + leaves.Positions[card * 4 + 2] + leaves.Positions[card * 4 + 3]) * 0.25f).ToArray();
            var (low, high) = (middles.Aggregate(Vector3.Min), middles.Aggregate(Vector3.Max));
            var (middle, half) = ((low + high) * 0.5f, (high - low) * 0.5f);

            // How far out a leaf stands, one at the crown's bounding ellipsoid.
            float Out(Vector3 place) => ((place - middle) / half).Length();
            var sky = Enumerable.Range(0, cards).Select(card => leaves.Sky![card * 4]).ToArray();
            var deep = Enumerable.Range(0, cards).Where(card => Out(middles[card]) < 0.35f).ToArray();
            var rim = Enumerable.Range(0, cards).Where(card => Out(middles[card]) > 0.85f).ToArray();
            var upperRim = rim.Where(card => middles[card].Y > middle.Y).ToArray();

            Assert.All(sky, seen => Assert.InRange(seen, 0f, 1f));
            var (deepSky, rimSky) = (deep.Average(card => sky[card]), upperRim.Average(card => sky[card]));
            Assert.True(rimSky - deepSky >= 0.24f, $"{species.LeafSet}'s deep leaves see {deepSky:F2} of the sky and its upper rim's {rimSky:F2}");
            var outward = rim.Count(card => Vector3.Dot(leaves.Out![card * 4], middles[card] - middle) > 0f);
            Assert.True(outward >= rim.Length * 85 / 100, $"{outward} of {species.LeafSet}'s {rim.Length} rim leaves carry a way out of the crown");
        }
    }

    /// <summary>
    /// The aspen carries the paper's leaves within a fifth, the willow its cards' bound of 30,000
    /// with each drawn larger, and a species of no leaves none.
    /// </summary>
    [Fact]
    public void ATreeCarriesThePapersLeavesUpToTheirBound()
    {
        // The aspen's paper carries 25 leaves on each of its 50 branches' 30 twigs, at six tenths.
        var aspen = TreeLeaves.Build(TreeSkeleton.Grow(Aspen, 3), Aspen, 3);
        Assert.InRange(aspen.Indices.Count / 6, 0.8 * 25 * 50 * 30 * 0.6, 1.2 * 25 * 50 * 30 * 0.6);

        var willow = TreeLeaves.Build(TreeSkeleton.Grow(TreeSpecies.WeepingWillow, 3), TreeSpecies.WeepingWillow, 3);
        Assert.InRange(willow.Indices.Count / 6, 20_000, 31_000);

        var bare = TreeLeaves.Build(TreeSkeleton.Grow(Aspen with { Leaves = 0 }, 3), Aspen with { Leaves = 0 }, 3);
        Assert.Empty(bare.Positions);
        Assert.Empty(bare.Indices);
    }

    /// <summary>Every preset's cards, or its fronds' strips, keep to the bound of 30,000.</summary>
    [Fact]
    public void EveryPresetsLeavesKeepToTheirBound()
    {
        foreach (var (name, species) in Presets)
        {
            var skeleton = TreeSkeleton.Grow(species, 2);
            var leaves = TreeLeaves.Build(skeleton, species, 2);
            var children = new int[skeleton.Nodes.Count];
            for (var i = 1; i < skeleton.Nodes.Count; i++) children[skeleton.Nodes[i].Parent]++;
            var twigs = children.Count(count => count == 0);

            // A card is a leaf of four corners, and a frond strip two corners a point of its stem.
            var cards = species.Leaves > 0 ? leaves.Positions.Count / 4 : twigs;
            Assert.True(cards <= 30_000, $"{name} has {cards} cards");
        }
    }

    /// <summary>
    /// Each card's stalk stands near a point of the tree, every card faces the side of up and away
    /// from the trunk and most face up, and every texture coordinate is inside the atlas.
    /// </summary>
    [Fact]
    public void LeavesStandOnTheTwigsAndFaceUpAndOut()
    {
        var skeleton = TreeSkeleton.Grow(Aspen, 2);
        var leaves = TreeLeaves.Build(skeleton, Aspen, 2);
        var points = skeleton.Nodes.Select(node => node.Place).ToArray();

        // Within the spread and a leaf's length, at the most enlarged, of a point of the tree.
        var reach = Aspen.LeafScale * 1.6f * 1.1f * 2.5f + 0.5f;
        var upward = 0;
        for (var card = 0; card < leaves.Positions.Count / 4; card++)
        {
            var stalk = Stalk(leaves, card);
            Assert.True(points.Any(point => (point - stalk).LengthSquared() < reach * reach), $"card {card} stands at {stalk}, off the tree");

            // Half turned there by the aspen's bend of one half and half as they leave the twig.
            var normal = leaves.Normals[4 * card];
            var outward = new Vector3(stalk.X, 0f, stalk.Z);
            var light = Vector3.UnitY + (outward.LengthSquared() > 1e-6f ? Vector3.Normalize(outward) * 0.5f : Vector3.Zero);
            Assert.True(Vector3.Dot(normal, light) >= -1e-4f, $"card {card} faces {normal}, away from the light");
            if (normal.Y > 0f) upward++;
        }

        Assert.True(upward > 0.55 * leaves.Positions.Count / 4, $"{upward} of {leaves.Positions.Count / 4} cards face up");
        Assert.All(leaves.Uvs, uv => Assert.True(uv.X is >= 0f and <= 1f && uv.Y is >= 0f and <= 1f, $"{uv} is outside the atlas"));
    }

    /// <summary>
    /// A fern's fronds are a strip a stem tip, two corners at each point of its stem and two
    /// triangles between points, unshaded as a crown and with no cards' twigs.
    /// </summary>
    [Fact]
    public void AFernsFrondsRunTheLengthOfItsStems()
    {
        var fern = TreeSpecies.Fern;
        var skeleton = TreeSkeleton.Grow(fern, 1);
        var children = new int[skeleton.Nodes.Count];
        for (var i = 1; i < skeleton.Nodes.Count; i++) children[skeleton.Nodes[i].Parent]++;
        var tips = children.Count(count => count == 0);

        var fronds = TreeLeaves.Build(skeleton, fern, 1);
        Assert.Equal(14, tips);
        var corners = fronds.Positions.Count / 2;
        var lengths = fronds.Indices.Count / 6;
        Assert.Equal(tips, corners - lengths);
        Assert.True(lengths > 10 * tips, $"the fronds run {lengths} lengths of their stems");
        Assert.Null(fronds.Sky);
        Assert.Null(fronds.Out);
        Assert.Empty(fronds.Twigs);
    }

    /// <summary>A species' leaf controls outside what they may be are refused.</summary>
    [Fact]
    public void LeafControlsOutOfRangeAreRefused()
    {
        var skeleton = TreeSkeleton.Grow(Aspen, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => TreeLeaves.Build(skeleton, Aspen with { LeafScale = 0f }, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TreeLeaves.Build(skeleton, Aspen with { LeafBend = 2f }, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TreeLeaves.Build(skeleton, Aspen with { LeafShape = (TreeLeafShape)99 }, 1));
    }

    /// <summary>
    /// A tree's limbs are its sixteen thickest stems, the trunk swings with none, every other stem
    /// with a limb that pivots where it leaves its parent, and each leaf's own pivot is its stalk
    /// with a phase from 0 up to 1.
    /// </summary>
    [Fact]
    public void EachLeafSwaysWithItsLimbAboutItsStalk()
    {
        var skeleton = TreeSkeleton.Grow(Aspen, 2);
        var limbs = TreeLimbs.Plan(skeleton);
        Assert.Equal(TreeLimbs.Limbs, limbs.Kept.Count);
        Assert.Contains(0, limbs.Kept);
        Assert.Equal(-1, limbs.LimbOf(0));
        foreach (var start in limbs.Starts.Skip(1))
        {
            var limb = limbs.LimbOf(start);
            Assert.True(limbs.Kept.Contains(limb) || limbs.OwnerOf(start) == 0, $"the stem at {start} swings with {limb}, no limb");
            var pivot = limbs.Pivot(limb, Vector3.Zero);
            Assert.Equal(skeleton.Nodes[skeleton.Nodes[limb].Parent].Place, new Vector3(pivot.X, pivot.Y, pivot.Z));
            Assert.InRange(pivot.W, 0f, 1f);
        }

        var leaves = TreeLeaves.Build(skeleton, Aspen, 2);
        for (var card = 0; card < leaves.Positions.Count / 4; card++)
        {
            var own = leaves.Own[4 * card];
            Assert.True((new Vector3(own.X, own.Y, own.Z) - Stalk(leaves, card)).Length() < 1e-4f, $"card {card} flutters about {own}, off its stalk");
            Assert.InRange(own.W, 0f, 1f);
        }
    }

    private static Vector3 Stalk(TreeLeafMesh leaves, int card) => (leaves.Positions[card * 4] + leaves.Positions[card * 4 + 1]) * 0.5f;

    private static float Area(TreeLeafMesh leaves, int card) =>
        Vector3.Cross(leaves.Positions[card * 4 + 1] - leaves.Positions[card * 4], leaves.Positions[card * 4 + 3] - leaves.Positions[card * 4]).Length();

    private static string Hash<T>(IReadOnlyList<T> values) where T : unmanaged =>
        Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(values.ToArray().AsSpan())));

    private static string RepositoryRoot()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "Vegetation.slnx")))
                return folder.FullName;
        }
        throw new DirectoryNotFoundException($"No folder above {AppContext.BaseDirectory} holds Vegetation.slnx.");
    }
}
