using System.Numerics;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The README's binding run against a host that stands in for an engine with none, a forest's
/// stems, its bark's and leaves' textures or pictures and a field's grass, the README's block
/// standing word for word between this file's markers, which ModuleTests reads.
/// </summary>
public sealed class BindingTests
{
    /// <summary>
    /// The binding plants 200 trees of the quaking aspen, whose stems stand on the ground and
    /// inside the forest's disc as tubes with their bark wrapped round them, opens its bark's and
    /// leaves' KTX2 files from the collection and scatters the field's grass, every clump on the
    /// ground inside the field.
    /// </summary>
    [Fact]
    public void ASpeciesWithSetsIsBoundWithNoEngine()
    {
        var host = new Host();
        Bind(host, TreeSpecies.QuakingAspen);

        Assert.True(host.Tubes.Count > 200 * 50, $"{host.Tubes.Count} stems for 200 trees.");
        Assert.All(host.Tubes, tube =>
        {
            Assert.True(tube.Around >= 1 && tube.Radii.All(radius => float.IsFinite(radius) && radius > 0f));
            Assert.True(tube.Along.Zip(tube.Along.Skip(1)).All(pair => pair.Second >= pair.First), "A stem's bark runs back along it.");
            Assert.All(tube.Points, point =>
            {
                Assert.InRange(new Vector2(point.X, point.Z).Length(), 0f, 50f + TreeSpecies.QuakingAspen.Scale);
                Assert.True(point.Y >= -0.01f, $"A stem reaches {point.Y} below the ground.");
            });
        });

        Assert.Equal(6, host.Textures.Count);
        byte[] ktx2 = [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.All(host.Textures, texture => Assert.Equal(ktx2, texture.Head));
        Assert.Empty(host.Pictures);

        Assert.NotEmpty(host.Meshes);
        Assert.All(host.Meshes, mesh => Assert.True(mesh.Vertices > 0 && mesh.Indices > 0));
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

    // The README's binding, its code block, of a species planted as a forest with a field of grass.
    private static void Bind(Host host, TreeSpecies species)
    {
        // The README's block from here to the end marker, word for word.
        // Each of the species' variants grown once, which every tree of the forest is one of.
        var variants = new TreeSkeleton[TreePlanting.Variants];
        for (var variant = 0; variant < variants.Length; variant++)
            variants[variant] = TreeSkeleton.Grow(species, TreePlanting.VariantSeed(variant));
        var stems = variants.Select(variant => TreeStems.Build(variant, species)).ToArray();

        // Two hundred trees over a disc of 50 meters about the origin, each standing by its seed.
        foreach (var (place, seed) in TreePlanting.Forest(1, Vector3.Zero, 50f, 200))
        {
            var stance = TreePlanting.Stance(seed);
            var turn = Quaternion.CreateFromAxisAngle(Vector3.UnitY, stance.Turn);
            Vector3 At(Vector3 point) => place + Vector3.Transform(point * stance.Size, turn);
            foreach (var stem in stems[stance.Variant])
            {
                var radii = stem.Radii.Select(radius => radius * stance.Size).ToArray();
                host.Tube([.. stem.Points.Select(At)], radii, stem.Around, stem.Along);
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

        // Grass over a field of 2 by 2 meters about the origin, a patch at a time, the patches
        // small since the lawn stands thick. A near patch is drawn as blades, all five segments of
        // each, and a far one as two crossed cards.
        var grass = new GrassSettings { PatchSize = 1f };
        foreach (var patch in GrassScatter.Patches(new Vector2(2f, 2f), grass))
        {
            var middle = new Vector3(patch.Middle.X, 0f, patch.Middle.Y);
            var blades = GrassBlades.Write(grass, patch.Size, patch.Seed, 5, patch.Middle);
            host.Blades(middle, blades.Positions, blades.Normals, blades.Uvs, blades.Colors, blades.Indices);
            foreach (var clump in GrassScatter.Clumps(grass, patch.Size, patch.Seed))
                host.Clump(middle + clump.Root, clump.Turn, clump.Height);
        }
        // The end marker.
    }

    // What an engine does with what the module gives, recorded rather than drawn.
    private sealed class Host
    {
        public List<(Vector3[] Points, float[] Radii, int Around, IReadOnlyList<float> Along)> Tubes { get; } = [];
        public List<(string Path, byte[] Head)> Textures { get; } = [];
        public List<(string Name, int Length)> Pictures { get; } = [];
        public List<(Vector3 Root, float Turn, float Height)> Clumps { get; } = [];
        public List<(Vector3 Middle, int Vertices, int Indices)> Meshes { get; } = [];

        // A stem, which an engine builds as a tube round its points by their radii, its bark
        // wrapped round it the count given and along it by the coordinate at each point.
        public void Tube(Vector3[] points, float[] radii, int around, IReadOnlyList<float> along) => Tubes.Add((points, radii, around, along));

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

        // A patch's blades, which an engine draws from the arrays as they are, both sides of each
        // triangle, about the patch's middle.
        public void Blades(
            Vector3 middle,
            IReadOnlyList<Vector3> positions,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector2> uvs,
            IReadOnlyList<Vector3> colors,
            IReadOnlyList<uint> indices)
        {
            Assert.Equal(positions.Count, normals.Count);
            Assert.Equal(positions.Count, uvs.Count);
            Assert.Equal(positions.Count, colors.Count);
            Meshes.Add((middle, positions.Count, indices.Count));
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
