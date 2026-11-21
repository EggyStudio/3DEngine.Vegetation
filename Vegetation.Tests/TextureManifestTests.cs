using Materials;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The texture collection's manifest is read by the Materials module's <c>TextureCollection</c>,
/// and the module's sets are the entries it gives, so no second reader of the manifest stands in
/// the library to drift from it.
/// </summary>
public sealed class TextureManifestTests
{
    /// <summary>
    /// Every entry the manifest names is a set of the collection, with the name, the role, the
    /// size, the pictures' paths and the pictures' layouts that <c>TextureCollection.Read</c> gives
    /// it.
    /// </summary>
    [Fact]
    public void EveryEntryOfTheManifestIsASetOfTheSameNameRoleSizeAndLayout()
    {
        var entries = TextureCollection.Read(File.ReadAllText(Path.Combine(Root(), "Vegetation", "Textures", "textures.json")), "textures.json");
        Assert.Equal(13, entries.Count);

        foreach (var entry in entries)
        {
            var set = TreeTextures.Find(entry.Name) ?? throw new InvalidOperationException($"The manifest names {entry.Name} and the collection has no such set.");
            Assert.Equal(entry.Kind switch
            {
                TextureKind.Bark => TreeTextureKind.Bark,
                TextureKind.Leaves => TreeTextureKind.Leaves,
                TextureKind.Grass => TreeTextureKind.Grass,
                _ => throw new InvalidOperationException($"{entry.Name} is a tile."),
            }, set.Kind);
            Assert.Equal(entry.Size, set.Size);
            Assert.Equal(entry.Meters, set.Meters);
            Assert.Equal(entry.Albedo.Path, set.Albedo);
            Assert.Equal(entry.Normal.Path, set.Normal);
            Assert.Equal(entry.Surface.Path, set.Surface);
            Assert.Equal(entry.Depth?.Path, set.Depth);
            Assert.Equal(PictureLayout.Color, entry.Albedo.Layout);
            Assert.Equal(PictureLayout.Normal, entry.Normal.Layout);
            Assert.Equal(PictureLayout.Surface, entry.Surface.Layout);
            Assert.Equal(entry.Depth is null ? null : PictureLayout.Depth, entry.Depth?.Layout);
            Assert.Equal(entry.LeafBoxes.Count == 0 ? [(0f, 0f, 1f, 1f)] : entry.LeafBoxes, set.LeafBoxes);
            Assert.Equal(entry.MeanSurface is { } surface ? (surface.X, surface.Y, surface.Z, surface.W) : (1f, 1f, 0f, 0f), set.MeanSurface);
            Assert.Equal(entry.MeanColor ?? System.Numerics.Vector3.One, set.MeanColor, new ColorsAgree());
        }
    }

    /// <summary>
    /// The library's sources hold no reader of JSON, since the one manifest is read by Materials.
    /// </summary>
    [Fact]
    public void TheLibraryHoldsNoReaderOfItsOwnForTheManifest()
    {
        var readers = Directory.EnumerateFiles(Path.Combine(Root(), "Vegetation"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(folder => folder is "obj" or "bin"))
            .Where(path => File.ReadAllText(path).Contains("System.Text.Json", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(Root(), path))
            .ToList();
        Assert.True(readers.Count == 0, $"These read JSON themselves: {string.Join(", ", readers)}");
    }

    // The two decodings of sRGB, the module's own in double precision and Materials' in single,
    // part by the last bit of a float at most, so a color is held to a ten-thousandth.
    private sealed class ColorsAgree : IEqualityComparer<System.Numerics.Vector3>
    {
        public bool Equals(System.Numerics.Vector3 a, System.Numerics.Vector3 b) => (a - b).Length() < 1e-4f;

        public int GetHashCode(System.Numerics.Vector3 value) => 0;
    }

    private static string Root()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "Vegetation.slnx")))
                return folder.FullName;
        }
        throw new DirectoryNotFoundException($"No folder above {AppContext.BaseDirectory} holds Vegetation.slnx.");
    }
}
