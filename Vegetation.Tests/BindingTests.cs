using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The README's binding run against a host that stands in for an engine with none, a forest's
/// stems, its bark's and leaves' textures or pictures and a field's grass, and the README's code
/// the same as the code run here.
/// </summary>
public sealed class BindingTests
{
    /// <summary>
    /// The binding plants 200 trees of the quaking aspen, whose stems stand on the ground and
    /// inside the forest's disc, opens its bark's and leaves' KTX2 files from the collection and
    /// scatters the field's grass, every clump on the ground inside the field.
    /// </summary>
    [Fact]
    public void ASpeciesWithSetsIsBoundWithNoEngine()
    {
        var host = new Host();
        Bind(host, TreeSpecies.QuakingAspen);

        Assert.True(host.Stems.Count > 200 * 100, $"{host.Stems.Count} stems for 200 trees.");
        Assert.All(host.Stems, stem =>
        {
            Assert.True(float.IsFinite(stem.Radius) && stem.Radius > 0f);
            Assert.InRange(new Vector2(stem.To.X, stem.To.Z).Length(), 0f, 50f + TreeSpecies.QuakingAspen.Scale);
            Assert.True(stem.From.Y >= -0.01f && stem.To.Y >= -0.01f, $"A stem reaches {stem.From.Y} and {stem.To.Y} below the ground.");
        });

        Assert.Equal(6, host.Textures.Count);
        byte[] ktx2 = [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.All(host.Textures, texture => Assert.Equal(ktx2, texture.Head));
        Assert.Empty(host.Pictures);

        Assert.NotEmpty(host.Clumps);
        Assert.All(host.Clumps, clump =>
        {
            Assert.Equal(0f, clump.Root.Y);
            Assert.InRange(clump.Root.X, -20f, 20f);
            Assert.InRange(clump.Root.Z, -20f, 20f);
        });
    }

    /// <summary>
    /// A species that names no set is drawn with the bark and the leaves the module makes, which
    /// the binding hands the host as pictures of their sizes.
    /// </summary>
    [Fact]
    public void ASpeciesWithNoSetsIsBoundWithTheModulesPictures()
    {
        var host = new Host();
        Bind(host, TreeSpecies.QuakingAspen with { BarkSet = null, LeafSet = null, LeafShape = TreeLeafShape.FiveLobeMaple });

        Assert.Empty(host.Textures);
        Assert.Equal(["bark", "leaves"], host.Pictures.Select(picture => picture.Name));
        Assert.Equal(4 * TreeLooks.BarkWidth * TreeLooks.BarkHeight, host.Pictures[0].Length);
        Assert.Equal(4 * TreeLooks.LeafSize * TreeLooks.LeafSize, host.Pictures[1].Length);
    }

    /// <summary>
    /// The code block of the README's section on binding is the body of <see cref="Bind"/> as it
    /// stands in this file, so the guide is the code the tests compile and run.
    /// </summary>
    [Fact]
    public void TheReadmesBindingIsTheCodeRunHere()
    {
        var root = RepositoryRoot();
        var readme = File.ReadAllLines(Path.Combine(root, "README.md"));
        var section = Array.IndexOf(readme, "## Binding it in your engine");
        Assert.True(section >= 0, "The README has no section named Binding it in your engine.");
        var open = Array.FindIndex(readme, section, line => line == "```csharp");
        var close = Array.FindIndex(readme, open + 1, line => line == "```");
        var guide = readme[(open + 1)..close];

        var source = File.ReadAllLines(Path.Combine(root, "Vegetation.Tests", "BindingTests.cs"));
        var start = Array.IndexOf(source, "    private static void Bind(Host host, TreeSpecies species)") + 2;
        Assert.True(start >= 2, "BindingTests.cs declares no Bind.");
        var end = Array.FindIndex(source, start, line => line == "    }");
        var body = source[start..end].Select(line => line.Length >= 8 ? line[8..] : line.Trim()).ToArray();

        Assert.Equal(body, guide);
    }

    // The README's binding, its code block, of a species planted as a forest with a field of grass.
    private static void Bind(Host host, TreeSpecies species)
    {
        // Each of the species' variants grown once, which every tree of the forest is one of.
        var variants = new TreeSkeleton[TreePlanting.Variants];
        for (var variant = 0; variant < variants.Length; variant++)
            variants[variant] = TreeSkeleton.Grow(species, TreePlanting.VariantSeed(variant));

        // Two hundred trees over a disc of 50 meters about the origin, each standing by its seed.
        foreach (var (place, seed) in TreePlanting.Forest(1, Vector3.Zero, 50f, 200))
        {
            var stance = TreePlanting.Stance(seed);
            var turn = Quaternion.CreateFromAxisAngle(Vector3.UnitY, stance.Turn);
            var nodes = variants[stance.Variant].Nodes;
            Vector3 At(Vector3 point) => place + Vector3.Transform(point * stance.Size, turn);
            foreach (var node in nodes)
            {
                if (node.Parent >= 0)
                    host.Stem(At(nodes[node.Parent].Place), At(node.Place), node.Radius * stance.Size);
            }
        }

        // The bark and the leaves from the collection where the species names a set it carries,
        // and otherwise the pictures the module makes, which the species' colors tint.
        if (species.BarkSet is { } barkName && TreeTextures.Find(barkName) is { } bark)
        {
            foreach (var path in (string[])[bark.Albedo, bark.Normal, bark.Surface])
                host.Texture(path, TreeTextures.Open(path)!);
        }
        else
        {
            host.Picture("bark", TreeLooks.Bark().Color, TreeLooks.BarkWidth, TreeLooks.BarkHeight);
        }

        if (species.LeafSet is { } leafName && TreeTextures.Find(leafName) is { } leaves)
        {
            foreach (var path in (string[])[leaves.Albedo, leaves.Normal, leaves.Surface])
                host.Texture(path, TreeTextures.Open(path)!);
        }
        else
        {
            var atlas = TreeLooks.Leaves(species.LeafShape, species.LeafScaleX);
            host.Picture("leaves", atlas, TreeLooks.LeafSize, TreeLooks.LeafSize);
        }

        // Grass over a field of 40 by 40 meters about the origin, a clump at a time.
        var grass = new GrassSettings();
        foreach (var patch in GrassScatter.Patches(new Vector2(40f, 40f), grass))
        {
            var middle = new Vector3(patch.Middle.X, 0f, patch.Middle.Y);
            foreach (var clump in GrassScatter.Clumps(grass, patch.Size, patch.Seed))
                host.Clump(middle + clump.Root, clump.Turn, clump.Height);
        }
    }

    // What an engine does with what the module gives, recorded rather than drawn.
    private sealed class Host
    {
        public List<(Vector3 From, Vector3 To, float Radius)> Stems { get; } = [];
        public List<(string Path, byte[] Head)> Textures { get; } = [];
        public List<(string Name, int Length)> Pictures { get; } = [];
        public List<(Vector3 Root, float Turn, float Height)> Clumps { get; } = [];

        // A stem from one point to the next, which an engine draws as a tapered cylinder.
        public void Stem(Vector3 from, Vector3 to, float radius) => Stems.Add((from, to, radius));

        // A texture file of the collection, which an engine reads with its KTX2 loader.
        public void Texture(string path, Stream file)
        {
            using (file)
            {
                var head = new byte[12];
                file.ReadExactly(head);
                Textures.Add((path, head));
            }
        }

        // A picture of RGBA bytes, which an engine uploads as an image of its size.
        public void Picture(string name, byte[] rgba, int width, int height)
        {
            Assert.Equal(4 * width * height, rgba.Length);
            Pictures.Add((name, rgba.Length));
        }

        // A clump of grass, which an engine draws as two cards crossed about its root.
        public void Clump(Vector3 root, float turn, float height) => Clumps.Add((root, turn, height));
    }

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
