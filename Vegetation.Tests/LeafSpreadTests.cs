using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The leaves follow a twig's length, a long twig carrying as many a meter as a typical one, and
/// the weeping willow weeps, its outer stems falling and its rim's leaves hanging, where a crown
/// that does not weep holds both up.
/// </summary>
public sealed class LeafSpreadTests
{
    private static readonly TreeSpecies[] Leafy = [TreeSpecies.QuakingAspen, TreeSpecies.BlackTupelo, TreeSpecies.WeepingWillow, TreeSpecies.CaliforniaBlackOak, TreeSpecies.Bush];

    /// <summary>
    /// At seeds 1 to 4 of each preset with leaves, the longest quarter of the twigs carries on the
    /// median no fewer cards a meter than the twigs about the median length, nine tenths as many at
    /// the least, so a long hanging twig is leafed along its length rather than bare.
    /// </summary>
    [Fact]
    public void ALongTwigCarriesAsManyLeavesAMeterAsATypicalOne()
    {
        var readings = new List<string>();
        var held = true;
        foreach (var species in Leafy)
        foreach (var seed in (uint[])[1, 2, 3, 4])
        {
            var skeleton = TreeSkeleton.Grow(species, seed);
            var leaves = TreeLeaves.Build(skeleton, species, seed);
            var cards = leaves.Twigs.CountBy(tip => tip).ToDictionary(pair => pair.Key, pair => pair.Value);
            var twigs = Twigs(skeleton).Select(twig => (twig.Length, PerMeter: cards.GetValueOrDefault(twig.Tip) / twig.Length)).OrderBy(twig => twig.Length).ToList();
            var typical = Median(twigs.Skip(twigs.Count * 2 / 5).Take(Math.Max(1, twigs.Count / 5)).Select(twig => twig.PerMeter));
            var longest = Median(twigs.Skip(twigs.Count * 3 / 4).Select(twig => twig.PerMeter));
            readings.Add($"{species.LeafSet} {seed}, {longest:F1} cards a meter on its longest twigs, {typical:F1} on its typical ones");
            held &= longest >= 0.9f * typical;
        }

        Assert.True(held, string.Join(Environment.NewLine, readings));
    }

    /// <summary>
    /// The weeping willow weeps at seeds 1 to 4, its long outer stems, the longest quarter of those
    /// starting in the outer half of its reach, falling on the median by more than three tenths of
    /// their length, and four fifths of its rim's cards, those in the outer three tenths of its
    /// reach, pointing down from stalk to tip, where the California black oak's rise and no more
    /// than two fifths of its rim's cards point down.
    /// </summary>
    [Fact]
    public void TheWeepingWillowsStemsFallAndItsRimsLeavesHang()
    {
        var readings = new List<string>();
        var weeps = true;
        foreach (var seed in (uint[])[1, 2, 3, 4])
        {
            var (willowFall, willowDown) = Weep(TreeSpecies.WeepingWillow, seed);
            var (oakFall, oakDown) = Weep(TreeSpecies.CaliforniaBlackOak, seed);
            readings.Add($"seed {seed}, the willow's stems fall {willowFall:F2} and {willowDown:P0} of its rim's cards point down, the oak's {oakFall:F2} and {oakDown:P0}");
            weeps &= willowFall > 0.3f && willowDown >= 0.8f && oakFall < 0f && oakDown <= 0.4f;
        }

        Assert.True(weeps, string.Join(Environment.NewLine, readings));
    }

    private static (float Fall, float Down) Weep(TreeSpecies species, uint seed)
    {
        var skeleton = TreeSkeleton.Grow(species, seed);
        var nodes = skeleton.Nodes;
        var carried = new int[nodes.Count];
        Array.Fill(carried, -1);
        for (var i = 1; i < nodes.Count; i++)
        {
            var parent = nodes[i].Parent;
            if (carried[parent] < 0 || nodes[i].Radius > nodes[carried[parent]].Radius) carried[parent] = i;
        }

        var reach = nodes.Max(node => Reach(node.Place));
        var stems = new List<(float Reach, float Length, float Fall)>();
        for (var start = 1; start < nodes.Count; start++)
        {
            if (carried[nodes[start].Parent] == start) continue;
            var (from, at, length) = (nodes[nodes[start].Parent].Place, start, 0f);
            var previous = from;
            while (true)
            {
                length += Vector3.Distance(previous, nodes[at].Place);
                previous = nodes[at].Place;
                if (carried[at] < 0) break;
                at = carried[at];
            }

            if (length > 0f) stems.Add((Reach(from), length, (from.Y - previous.Y) / length));
        }

        var longest = stems.Select(stem => stem.Length).Order().ElementAt(stems.Count * 3 / 4);
        var fall = Median(stems.Where(stem => stem.Reach > reach / 2f && stem.Length >= longest).Select(stem => stem.Fall));

        var leaves = TreeLeaves.Build(skeleton, species, seed);
        var (rim, down) = (0, 0);
        for (var card = 0; card < leaves.Positions.Count / 4; card++)
        {
            var stalk = (leaves.Positions[4 * card] + leaves.Positions[4 * card + 1]) * 0.5f;
            if (Reach(stalk) < 0.7f * reach) continue;
            var tip = (leaves.Positions[4 * card + 2] + leaves.Positions[4 * card + 3]) * 0.5f;
            rim++;
            if (tip.Y < stalk.Y) down++;
        }

        return (fall, (float)down / rim);
    }

    // The twigs, the stems that carry no other, each its tip and its length from the point it
    // leaves its parent, as the leaves are placed along them.
    private static IEnumerable<(int Tip, float Length)> Twigs(TreeSkeleton skeleton)
    {
        var nodes = skeleton.Nodes;
        var children = new int[nodes.Count];
        for (var i = 1; i < nodes.Count; i++) children[nodes[i].Parent]++;
        for (var tip = 1; tip < nodes.Count; tip++)
        {
            if (children[tip] != 0) continue;
            var (at, length) = (tip, 0f);
            while (true)
            {
                var parent = nodes[at].Parent;
                length += Vector3.Distance(nodes[at].Place, nodes[parent].Place);
                if (parent == 0 || children[parent] != 1) break;
                at = parent;
            }

            if (length > 0f) yield return (tip, length);
        }
    }

    private static float Reach(Vector3 place) => MathF.Sqrt(place.X * place.X + place.Z * place.Z);

    private static float Median(IEnumerable<float> values)
    {
        var sorted = values.Order().ToList();
        return sorted.Count == 0 ? float.NaN : sorted[sorted.Count / 2];
    }
}
