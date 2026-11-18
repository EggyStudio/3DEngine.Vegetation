using System.Numerics;

namespace Vegetation;

/// <summary>
/// The leaves of a grown tree, a card a leaf along its twigs, cut from the species' leaf set or
/// <see cref="TreeLooks"/>' atlas, or a frond along each stem of the last level where the species'
/// leaves are fronds.
/// </summary>
/// <remarks>
/// <para>
/// A twig is a stem that carries no other, from the point it leaves its parent to its tip. Weber
/// and Penn's tree carries the species' <see cref="TreeSpecies.Leaves"/> on every stem of its last
/// level, as many stems as its levels' branches multiplied make, and a grown tree has fewer twigs
/// where light and room stopped its buds, so the paper's count of leaves is shared among the twigs
/// there are, at most 30,000 a tree, each leaf larger, up to 1.6 times, where the most leaves the
/// cards hold is fewer than the paper's, so the crown covers what the paper's covers, as its
/// quality factor does. A twig shorter than most carries fewer, more of them toward its tip, each
/// leaving at the leaf level's down angle and turned round the twig from the one before, standing
/// off the twig by up to one and a half of its lengths, as a leaf on one of the twig's finer
/// twiglets does, and turned by <see cref="TreeSpecies.LeafBend"/> to face up and away from the
/// trunk. A card is the leaf's own picture, a box of its atlas, its stalk at the twig, as long as
/// <see cref="TreeSpecies.LeafScale"/> a tenth either way at random and as wide as its picture's
/// proportions make it.
/// </para>
/// <para>
/// Where the species' leaves are fronds, each stem of the last level is drawn as a strip along its
/// whole length, as wide as the leaf scale, one picture of the atlas running from its foot to its
/// tip, so a fern's or a palm's frond bends as its stem does.
/// </para>
/// <para>
/// The cards are shaded as a crown by <see cref="TreeCrown.Shade"/>, and each vertex carries its
/// sway, its limb's pivot from <see cref="TreeLimbs"/> and its own, a card's stalk or a frond's
/// foot. The numbers are drawn from <see cref="Pcg"/> in a fixed order, so a seed places the same
/// leaves on every runtime.
/// </para>
/// </remarks>
public static class TreeLeaves
{
    // The most leaves a tree's cards hold, past which each is drawn larger to cover the same area,
    // and the most larger it is drawn.
    private const int MostLeaves = 30_000;
    private const float MostLarger = 1.6f;

    // How far a leaf stands off its twig at most, in its own lengths.
    private const float Spread = 1.5f;

    /// <summary>Makes the leaves of a grown tree, every leaf of it.</summary>
    /// <param name="skeleton">
    /// The tree, as <see cref="TreeSkeleton.Grow(TreeSpecies, uint)"/> grew it.
    /// </param>
    /// <param name="species">
    /// Its species, whose leaves' count, size, angles and set the cards follow.
    /// </param>
    /// <param name="seed">
    /// The seed the tree grew from, which places the leaves the same way each time.
    /// </param>
    /// <returns>The leaves' arrays, empty where the species has no leaves.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A control of the species is outside what it may be, which the message names.
    /// </exception>
    public static TreeLeafMesh Build(TreeSkeleton skeleton, TreeSpecies species, uint seed) => Build(skeleton, species, seed, TreeDetail.Full);

    /// <summary>Makes the leaves of a grown tree at a level of detail.</summary>
    /// <param name="skeleton">
    /// The tree, as <see cref="TreeSkeleton.Grow(TreeSpecies, uint)"/> grew it.
    /// </param>
    /// <param name="species">
    /// Its species, whose leaves' count, size, angles and set the cards follow.
    /// </param>
    /// <param name="seed">
    /// The seed the tree grew from, which places the leaves the same way each time.
    /// </param>
    /// <param name="detail">
    /// The level, every leaf, or the reduced one a tree further off is drawn in.
    /// </param>
    /// <returns>The leaves' arrays, empty where the species has no leaves.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A control of the species is outside what it may be, or the level is not one of
    /// <see cref="TreeDetail"/>'s.
    /// </exception>
    /// <remarks>
    /// The reduced level is every other card of the full one, at the same place, each drawn the
    /// square root of two larger about its stalk, so the crown covers as much with half the cards
    /// and the two levels show the same leaves as a tree is dithered from one to the other. Fronds
    /// are kept whole, since a frond is a fern's whole leaf.
    /// </remarks>
    public static TreeLeafMesh Build(TreeSkeleton skeleton, TreeSpecies species, uint seed, TreeDetail detail)
    {
        ArgumentNullException.ThrowIfNull(skeleton);
        ArgumentNullException.ThrowIfNull(species);
        species.Check();
        if (!Enum.IsDefined(detail)) throw new ArgumentOutOfRangeException(nameof(detail), detail, "A level of detail is one of TreeDetail's.");

        var cards = MakeCards(skeleton, species, seed);
        cards.Sway(TreeLimbs.Plan(skeleton));
        return detail == TreeDetail.Reduced && species.Leaves > 0 ? cards.Halved() : cards.Whole();
    }

    private static Cards MakeCards(TreeSkeleton skeleton, TreeSpecies species, uint seed)
    {
        var boxes = LeafBoxes(species, out var aspect);
        var cards = new Cards();
        if (species.Leaves == 0) return cards;

        var random = new Pcg((ulong)seed ^ 0x9E3779B97F4A7C15ul);
        var twigs = Twigs(skeleton);
        var lengths = twigs.Select(twig => Length(twig.Points)).OrderBy(length => length).ToArray();
        var typical = lengths.Length > 0 ? lengths[lengths.Length / 2] : 1f;

        // The paper's count of leaves, shared among the twigs there are, and each leaf's scale.
        var papers = (float)species.Leaves * MathF.Max(PapersTwigs(species), twigs.Count);
        var drawn = MathF.Min(papers, MostLeaves);
        var larger = Math.Clamp(MathF.Sqrt(papers / MathF.Max(drawn, 1f)), 1f, MostLarger);

        // Shared by each twig's length against most twigs', a short one taking fewer, so the leaves
        // drawn come to the count.
        var weights = twigs.Select(twig => Math.Clamp(Length(twig.Points) / typical, 0.3f, 1f)).ToArray();
        var perWeight = weights.Length > 0 ? drawn / weights.Sum() : 0f;
        var owed = 0f;

        for (var t = 0; t < twigs.Count; t++)
        {
            var twig = twigs[t].Points;
            cards.Tip = twigs[t].Tip;
            if (species.Leaves < 0)
            {
                for (var i = 0; i < -species.Leaves; i++) cards.Frond(twig, boxes[random.Below(boxes.Count)], species.LeafScale, aspect, MathF.PI * i / -species.Leaves);
                continue;
            }

            var length = Length(twig);
            owed += perWeight * weights[t];
            var count = (int)MathF.Floor(owed);
            owed -= count;
            var turn = random.Unit() * MathF.Tau;
            var level = species.LeafLevel;
            for (var k = 0; k < count; k++)
            {
                // Toward the tip, as the paper's leaves gather where the light is.
                var along = MathF.Pow((k + 0.5f + 0.4f * random.Signed()) / count, 0.75f) * length;
                var (place, way) = At(twig, along);

                turn += Radians(level.Rotate + level.RotateVariation * random.Signed());
                var round = Rotate(Square(way), way, turn);
                var down = Radians(level.DownAngle + level.DownAngleVariation * random.Signed());
                var leaf = (way * MathF.Cos(down) + round * MathF.Sin(down)).Normalized;

                // Off the twig by up to one and a half of its own lengths, as a leaf on one of the
                // twig's own twiglets, finer than the growth grows, stands off it.
                var box = boxes[random.Below(boxes.Count)];
                var size = species.LeafScale * larger * (1f + 0.1f * random.Signed());
                var stalk = place + Rotate(Square(way), way, random.Unit() * MathF.Tau) * (Spread * size * MathF.Sqrt(random.Unit()));

                // The card's face turned from facing round the twig toward up and out from the
                // trunk.
                var outward = new Vector3(stalk.X, 0f, stalk.Z);
                var light = (Vector3.UnitY + (LengthSquared(outward) > 1e-6f ? outward.Normalized * 0.5f : Vector3.Zero)).Normalized;
                var face = Cross(leaf, Cross(round, leaf)).Normalized;
                var wanted = light - leaf * Dot(light, leaf);
                if (LengthSquared(wanted) > 1e-6f) face = (face * (1f - species.LeafBend) + wanted.Normalized * species.LeafBend).Normalized;
                if (Dot(face, light) < 0f) face = -face;

                cards.Card(stalk, leaf, face, box, size, size * box.Width * aspect / box.Height);
            }
        }

        if (species.Leaves > 0) cards.ShadeBySky();
        return cards;
    }

    // How many stems the paper's tree of a species has at its last level, its trunks' count times
    // each level's branches, those past the first at six tenths, about the share of its most the
    // paper gives a stem that is not the longest of its level.
    private static float PapersTwigs(TreeSpecies species)
    {
        var levels = species.Levels;
        var stems = (float)(species.BaseSplits + 1);
        for (var i = 1; i < levels.Count; i++) stems *= levels[i].Branches * (i > 1 ? 0.6f : 1f);
        return stems;
    }

    // The boxes of the species' leaf set, or of the module's atlas, and the atlas's width against
    // its height.
    private static IReadOnlyList<(float U, float V, float Width, float Height)> LeafBoxes(TreeSpecies species, out float aspect)
    {
        if (species.LeafSet is { } name && TreeTextures.Find(name) is { Kind: TreeTextureKind.Leaves } set)
        {
            aspect = set.Size is var (width, height) ? (float)width / height : 1f;
            return set.LeafBoxes;
        }

        aspect = 1f;
        return TreeLooks.LeafBoxes;
    }

    // The twigs, each its tip's node and its points from the point it leaves its parent to its
    // tip, through every point between, which carries no other stem.
    private static List<(int Tip, Vector3[] Points)> Twigs(TreeSkeleton skeleton)
    {
        var nodes = skeleton.Nodes;
        var children = new int[nodes.Count];
        for (var i = 1; i < nodes.Count; i++) children[nodes[i].Parent]++;

        var twigs = new List<(int, Vector3[])>();
        for (var tip = 1; tip < nodes.Count; tip++)
        {
            if (children[tip] != 0) continue;
            var points = new List<Vector3> { nodes[tip].Place };
            var at = nodes[tip].Parent;
            while (true)
            {
                points.Add(nodes[at].Place);
                if (at == 0 || children[at] != 1) break;
                at = nodes[at].Parent;
            }

            points.Reverse();
            twigs.Add((tip, [.. points]));
        }

        return twigs;
    }

    private static float Length(Vector3[] twig)
    {
        var length = 0f;
        for (var i = 1; i < twig.Length; i++) length += Length(twig[i] - twig[i - 1]);
        return length;
    }

    // The place a distance along a twig and the way it runs there.
    private static (Vector3 Place, Vector3 Way) At(Vector3[] twig, float along)
    {
        for (var i = 1; i < twig.Length; i++)
        {
            var step = twig[i] - twig[i - 1];
            var length = Length(step);
            if (along <= length || i == twig.Length - 1)
            {
                var t = length > 0f ? Math.Clamp(along / length, 0f, 1f) : 0f;
                return (twig[i - 1] + step * t, step.Normalized);
            }

            along -= length;
        }

        return (twig[0], Vector3.UnitY);
    }

    private static Vector3 Square(Vector3 way) => Cross(way, MathF.Abs(way.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX).Normalized;

    private static Vector3 Rotate(Vector3 vector, Vector3 axis, float angle)
    {
        var (sin, cos) = MathF.SinCos(angle);
        return vector * cos + Cross(axis, vector) * sin + axis * (Dot(axis, vector) * (1f - cos));
    }

    private static float Radians(float degrees) => degrees * (MathF.PI / 180f);

    // The products and sums written out a term at a time, rather than through System.Numerics'
    // dot and cross products, whose order of operations may round apart from one runtime to the
    // next, so a seed places the same leaves everywhere.
    private static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    private static Vector3 Cross(Vector3 a, Vector3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    private static float LengthSquared(Vector3 v) => v.X * v.X + v.Y * v.Y + v.Z * v.Z;

    private static float Length(Vector3 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

    // The cards' vertices and triangles as they are made.
    private sealed class Cards
    {
        private readonly List<Vector3> _positions = [];
        private readonly List<Vector3> _normals = [];
        private readonly List<Vector2> _uvs = [];
        private readonly List<uint> _indices = [];
        private readonly List<int> _tips = [];
        private float[]? _sky;
        private Vector3[]? _out;

        // Each vertex's leaf, the place it flutters about and its twig's tip, and once the limbs
        // are known its two pivots.
        private readonly List<int> _leafOf = [];
        private readonly List<Vector3> _held = [];
        private readonly List<int> _tipOf = [];
        private Vector4[] _limb = [];
        private Vector4[] _own = [];
        private int _leaf;
        private Vector3 _holding;

        // The node of the tip of the twig the cards made next stand on.
        public int Tip { get; set; }

        // A leaf's card, its stalk at a place, running along a way and facing a way, as long and as
        // wide as given, wound counterclockwise seen from its face.
        public void Card(Vector3 place, Vector3 leaf, Vector3 face, (float U, float V, float Width, float Height) box, float length, float width)
        {
            var side = Cross(leaf, face).Normalized * (width * 0.5f);
            var first = (uint)_positions.Count;
            (_leaf, _holding) = (_tips.Count, place);
            Vertex(place - side, face, box.U, box.V + box.Height);
            Vertex(place + side, face, box.U + box.Width, box.V + box.Height);
            Vertex(place + side + leaf * length, face, box.U + box.Width, box.V);
            Vertex(place - side + leaf * length, face, box.U, box.V);
            _indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
            _tips.Add(Tip);
        }

        // A frond along a stem, a strip as wide as given from its foot to its tip, its picture's
        // length along the stem, its long side turned along the strip where it lies across the
        // atlas.
        public void Frond(Vector3[] stem, (float U, float V, float Width, float Height) box, float width, float aspect, float twist)
        {
            var length = Length(stem);
            if (length <= 0f) return;
            var lying = box.Width * aspect > box.Height;
            var first = (uint)_positions.Count;
            (_leaf, _holding) = (_leafOf.Count > 0 ? _leafOf[^1] + 1 : 0, stem[0]);
            var along = 0f;
            for (var i = 0; i < stem.Length; i++)
            {
                if (i > 0) along += Length(stem[i] - stem[i - 1]);
                var way = (stem[Math.Min(i + 1, stem.Length - 1)] - stem[Math.Max(i - 1, 0)]).Normalized;
                var level = Cross(way, Vector3.UnitY);
                var side = Rotate(LengthSquared(level) > 1e-6f ? level.Normalized : Square(way), way, twist) * (width * 0.5f);
                if (Cross(side, way).Y < 0f) side = -side;
                var face = Cross(side, way).Normalized;

                var t = along / length;
                if (lying)
                {
                    Vertex(stem[i] - side, face, box.U + t * box.Width, box.V + box.Height);
                    Vertex(stem[i] + side, face, box.U + t * box.Width, box.V);
                }
                else
                {
                    Vertex(stem[i] - side, face, box.U, box.V + box.Height * (1f - t));
                    Vertex(stem[i] + side, face, box.U + box.Width, box.V + box.Height * (1f - t));
                }

                if (i == 0) continue;
                var at = first + (uint)(2 * i);
                _indices.AddRange([at - 2, at - 1, at + 1, at - 2, at + 1, at]);
            }
        }

        private void Vertex(Vector3 place, Vector3 normal, float u, float v)
        {
            _positions.Add(place);
            _normals.Add(normal);
            _uvs.Add(new Vector2(u, v));
            _leafOf.Add(_leaf);
            _held.Add(_holding);
            _tipOf.Add(Tip);
        }

        // Gives each vertex the pivot of the limb its twig grows from and the pivot its leaf
        // flutters about, a card's stalk or a frond's foot, each leaf with a phase of its own. A
        // leaf on the trunk itself swings with no limb, its stalk standing for the pivot.
        public void Sway(TreeLimbs limbs)
        {
            _limb = new Vector4[_positions.Count];
            _own = new Vector4[_positions.Count];
            for (var i = 0; i < _positions.Count; i++)
            {
                var held = _held[i];
                _limb[i] = limbs.Pivot(limbs.LimbOf(limbs.StemOf(_tipOf[i])), held);
                _own[i] = new Vector4(held.X, held.Y, held.Z, TreeLimbs.Phase(0x5EED + _leafOf[i]));
            }
        }

        // Gives each card the share of the sky it sees through the crown and the way out of the
        // crown at it, from its middle and its area.
        public void ShadeBySky()
        {
            var cards = _positions.Count / 4;
            var places = new Vector3[cards];
            var areas = new float[cards];
            for (var card = 0; card < cards; card++)
            {
                var corner = _positions[card * 4];
                places[card] = (corner + _positions[card * 4 + 1] + _positions[card * 4 + 2] + _positions[card * 4 + 3]) * 0.25f;
                areas[card] = Length(Cross(_positions[card * 4 + 1] - corner, _positions[card * 4 + 3] - corner));
            }

            var (sky, outward) = TreeCrown.Shade(places, areas);
            _sky = new float[_positions.Count];
            _out = new Vector3[_positions.Count];
            for (var card = 0; card < cards; card++)
            {
                for (var corner = card * 4; corner < card * 4 + 4; corner++)
                    (_sky[corner], _out[corner]) = (sky[card], outward[card]);
            }
        }

        public TreeLeafMesh Whole() => new([.. _positions], [.. _normals], [.. _uvs], _sky, _out, _limb, _own, [.. _indices], [.. _tips]);

        // Every other card, each the square root of two larger about its stalk, the middle of its
        // first two corners, so half the cards cover what all of them do.
        public TreeLeafMesh Halved()
        {
            var kept = _positions.Count / 8;
            var positions = new Vector3[kept * 4];
            var normals = new Vector3[kept * 4];
            var uvs = new Vector2[kept * 4];
            var sky = _sky is null ? null : new float[kept * 4];
            var outward = _out is null ? null : new Vector3[kept * 4];
            var limb = new Vector4[kept * 4];
            var own = new Vector4[kept * 4];
            var indices = new uint[kept * 6];
            var tips = new int[kept];
            for (var card = 0; card < kept; card++)
            {
                var from = card * 8;
                var stalk = (_positions[from] + _positions[from + 1]) * 0.5f;
                for (var corner = 0; corner < 4; corner++)
                {
                    var (to, of) = (card * 4 + corner, from + corner);
                    positions[to] = stalk + (_positions[of] - stalk) * MathF.Sqrt(2f);
                    (normals[to], uvs[to], limb[to], own[to]) = (_normals[of], _uvs[of], _limb[of], _own[of]);
                    if (sky is not null) sky[to] = _sky![of];
                    if (outward is not null) outward[to] = _out![of];
                }

                var first = (uint)(card * 4);
                uint[] quad = [first, first + 1, first + 2, first, first + 2, first + 3];
                quad.CopyTo(indices, card * 6);
                tips[card] = _tips[card * 2];
            }

            return new TreeLeafMesh(positions, normals, uvs, sky, outward, limb, own, indices, tips);
        }
    }
}
