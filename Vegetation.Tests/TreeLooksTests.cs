using System.Security.Cryptography;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The bark's pictures and the leaves' atlas <see cref="TreeLooks"/> makes, the bark tiling and the
/// same on every system, each leaf in its box stalk down, the shapes apart and the widths refused.
/// </summary>
public sealed class TreeLooksTests
{
    private const int Cell = TreeLooks.LeafSize / 2;

    /// <summary>
    /// The bark's three pictures are opaque, the color gray, the depth the gray turned over and the
    /// normal facing out, and they hash the same on every system, since the bark is worked out by
    /// arithmetic alone, which IEEE 754 rounds the same everywhere.
    /// </summary>
    [Fact]
    public void TheBarkIsGrayDeepAndTheSameEverywhere()
    {
        var (color, normal, depth) = TreeLooks.Bark();
        var texels = TreeLooks.BarkWidth * TreeLooks.BarkHeight;
        Assert.All([color, normal, depth], picture => Assert.Equal(4 * texels, picture.Length));
        for (var at = 0; at < texels; at++)
        {
            var gray = color[4 * at];
            Assert.Equal((gray, gray, (byte)255), (color[4 * at + 1], color[4 * at + 2], color[4 * at + 3]));
            var deep = (byte)(255 - gray);
            Assert.Equal((deep, deep, deep, (byte)255), (depth[4 * at], depth[4 * at + 1], depth[4 * at + 2], depth[4 * at + 3]));
            Assert.InRange(normal[4 * at + 2], 128, 255);
            Assert.Equal(255, normal[4 * at + 3]);
        }

        Assert.Equal("C259135C1574A3BF7E26F2315AD4640B2DC4874F7EE46B76CB22CABFEC200949", Hash(color, normal, depth));
        Assert.Equal(Hash(color, normal, depth), Hash(TreeLooks.Bark().Color, TreeLooks.Bark().Normal, TreeLooks.Bark().Depth));
    }

    /// <summary>
    /// The bark tiles both ways, the step from its last column to its first and from its last row
    /// to its first no larger than the largest step between two neighbors inside it.
    /// </summary>
    [Fact]
    public void TheBarkTilesBothWays()
    {
        var (color, _, _) = TreeLooks.Bark();
        int Gray(int x, int y) => color[4 * (x + TreeLooks.BarkWidth * y)];
        var (inside, seam) = (0, 0);
        for (var y = 0; y < TreeLooks.BarkHeight; y++)
        for (var x = 0; x < TreeLooks.BarkWidth; x++)
        {
            var (right, down) = (Math.Abs(Gray((x + 1) % TreeLooks.BarkWidth, y) - Gray(x, y)), Math.Abs(Gray(x, (y + 1) % TreeLooks.BarkHeight) - Gray(x, y)));
            if (x + 1 < TreeLooks.BarkWidth) inside = Math.Max(inside, right);
            else seam = Math.Max(seam, right);
            if (y + 1 < TreeLooks.BarkHeight) inside = Math.Max(inside, down);
            else seam = Math.Max(seam, down);
        }

        Assert.InRange(seam, 0, inside);
    }

    /// <summary>
    /// Every shape's four leaves stand in the four boxes of <see cref="TreeLooks.LeafBoxes"/>,
    /// which cover the atlas, each with its stalk at the middle of its box's bottom and its box's
    /// top corners clear, every texel either clear to zero or an opaque gray.
    /// </summary>
    [Fact]
    public void EachLeafStandsInItsBoxStalkDown()
    {
        Assert.Equal(4, TreeLooks.LeafBoxes.Count);
        Assert.Equal(1f, TreeLooks.LeafBoxes.Sum(box => box.Width * box.Height));
        Assert.Equal(4, TreeLooks.LeafBoxes.Select(box => (box.U, box.V)).Distinct().Count());
        foreach (var shape in Enum.GetValues<TreeLeafShape>())
        foreach (var width in (float[])[0.2f, 0.6f, 1f])
        {
            var atlas = TreeLooks.Leaves(shape, width);
            Assert.Equal(4 * TreeLooks.LeafSize * TreeLooks.LeafSize, atlas.Length);
            for (var at = 0; at < atlas.Length; at += 4)
            {
                var clear = atlas[at + 3] == 0;
                Assert.Equal(clear ? ((byte)0, (byte)0, (byte)0) : (atlas[at], atlas[at], (byte)255), (atlas[at + 1], atlas[at + 2], atlas[at + 3]));
                if (clear) Assert.Equal(0, atlas[at]);
            }

            foreach (var (u, v, boxWidth, boxHeight) in TreeLooks.LeafBoxes)
            {
                var (left, top) = ((int)(u * TreeLooks.LeafSize), (int)(v * TreeLooks.LeafSize));
                var (right, bottom) = (left + (int)(boxWidth * TreeLooks.LeafSize) - 1, top + (int)(boxHeight * TreeLooks.LeafSize) - 1);
                Assert.Equal(255, Alpha(atlas, (left + right) / 2, bottom));
                Assert.Equal(0, Alpha(atlas, left, top));
                Assert.Equal(0, Alpha(atlas, right, top));
            }
        }
    }

    /// <summary>
    /// Each shape's atlas differs from every other's, and each of its four leaves covers more of
    /// its box than the one before, as a wider width covers more of every box.
    /// </summary>
    [Fact]
    public void TheShapesDifferAndWidenWithTheirWidth()
    {
        var shapes = Enum.GetValues<TreeLeafShape>();
        var atlases = shapes.Select(shape => TreeLooks.Leaves(shape, 0.6f)).ToArray();
        for (var a = 0; a < atlases.Length; a++)
        for (var b = a + 1; b < atlases.Length; b++)
            Assert.False(atlases[a].AsSpan().SequenceEqual(atlases[b]), $"{shapes[a]} and {shapes[b]} make the same atlas.");

        foreach (var shape in shapes)
        {
            var narrow = Covered(TreeLooks.Leaves(shape, 0.3f));
            var wide = Covered(TreeLooks.Leaves(shape, 0.6f));
            for (var leaf = 0; leaf < 4; leaf++) Assert.True(narrow[leaf] < wide[leaf], $"{shape}'s leaf {leaf} covers {narrow[leaf]} texels at 0.3 and {wide[leaf]} at 0.6.");
            for (var leaf = 1; leaf < 4; leaf++) Assert.True(wide[leaf - 1] < wide[leaf], $"{shape}'s leaf {leaf} covers {wide[leaf]} texels after {wide[leaf - 1]}.");
        }
    }

    /// <summary>A width that is not a positive number is refused as the width argument.</summary>
    [Theory]
    [InlineData(0f)]
    [InlineData(-0.5f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void AWidthThatIsNotPositiveIsRefused(float width)
    {
        var refusal = Assert.Throws<ArgumentOutOfRangeException>(() => TreeLooks.Leaves(TreeLeafShape.Oval, width));
        Assert.Equal("width", refusal.ParamName);
    }

    private static byte Alpha(byte[] atlas, int x, int y) => atlas[4 * (x + TreeLooks.LeafSize * y) + 3];

    // The opaque texels in each of the atlas's four cells, in the order of its boxes.
    private static int[] Covered(byte[] atlas)
    {
        var covered = new int[4];
        for (var y = 0; y < TreeLooks.LeafSize; y++)
        for (var x = 0; x < TreeLooks.LeafSize; x++)
            if (Alpha(atlas, x, y) == 255) covered[x / Cell + 2 * (y / Cell)]++;
        return covered;
    }

    private static string Hash(params byte[][] pictures) => Convert.ToHexString(SHA256.HashData(pictures.SelectMany(picture => picture).ToArray()));
}
