using System.Text.Json;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The texture collection, each carried set's files against its manifest and within its bounds, the
/// files' formats and a bark's depth, the lookup by name, a game's folder of sets added, and every
/// file opened by its path as its folder holds it.
/// </summary>
public sealed class TreeTexturesTests
{
    private static readonly string Collection = Path.Combine(RepositoryRoot(), "Vegetation", "Textures");

    // A set of 64 square made from no source by the Materials module's scripts/textures.py
    // --sample, kept with the tests so a machine without the import's Python still adds one.
    private static readonly string Sample = Path.Combine(RepositoryRoot(), "Vegetation.Tests", "references", "textures");

    private static readonly TreeSpecies[] Presets = [TreeSpecies.QuakingAspen, TreeSpecies.BlackTupelo, TreeSpecies.WeepingWillow, TreeSpecies.CaliforniaBlackOak, TreeSpecies.Bush, TreeSpecies.Fern];

    [Fact]
    public void AFolderOfSetsIsAddedAndFoundByName()
    {
        var added = Assert.Single(TreeTextures.Add(Sample));
        Assert.Equal("sample", added.Name);
        Assert.Equal(TreeTextureKind.Bark, added.Kind);
        Assert.StartsWith("added/", added.Albedo, StringComparison.Ordinal);
        Assert.EndsWith("/sample/albedo.ktx2", added.Albedo, StringComparison.Ordinal);
        Assert.Same(added, TreeTextures.Find("sample"));
        Assert.Contains(added, TreeTextures.All);
        Assert.Null(TreeTextures.Find("no-such-bark"));

        // Its files are read where they are, by the paths it gives and by no other.
        Assert.Contains(added.Depth!, TreeTextures.Files);
        Assert.Equal(File.ReadAllBytes(Path.Combine(Sample, "sample", "albedo.ktx2")), Read(added.Albedo));
        Assert.Null(TreeTextures.Open("sample/albedo.ktx2"));
    }

    [Fact]
    public void AFolderMissingAFileOfASetIsRefused()
    {
        var folder = Directory.CreateTempSubdirectory("vegetation-textures-");
        try
        {
            var set = Directory.CreateDirectory(Path.Combine(folder.FullName, "game-bark")).FullName;
            foreach (var file in new[] { "albedo.ktx2", "surface.ktx2" })
            {
                File.Copy(Path.Combine(Sample, "sample", file), Path.Combine(set, file));
            }

            File.WriteAllText(Path.Combine(folder.FullName, "textures.json"), """{ "entries": [ { "name": "game-bark", "kind": "bark" } ] }""");
            Assert.Throws<InvalidDataException>(() => TreeTextures.Add(folder.FullName));
            Assert.Throws<InvalidDataException>(() => TreeTextures.Add(set));
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [Fact]
    public void EveryCarriedSetKeepsToItsBounds()
    {
        var entries = Entries();
        long total = 0;
        var sizes = new Dictionary<string, long>();
        foreach (var set in Carried())
        {
            var entry = entries[set.Name];
            long size = 0;
            foreach (var file in new[] { "albedo.ktx2", "normal.ktx2", "surface.ktx2" })
            {
                var bytes = File.ReadAllBytes(Path.Combine(Collection, set.Name, file));
                Assert.Equal(entry.GetProperty("files").GetProperty(file).GetInt64(), bytes.Length);
                var (_, width, height, _, _) = Header(bytes);
                Assert.True(width <= 1024 && height <= 1024, $"{set.Name}/{file} is {width} by {height}");
                size += bytes.Length;
            }

            sizes[set.Name] = size;
            total += size;
        }

        // A species' bark and leaves together, and the whole collection.
        Assert.True(total <= 32L << 20, $"the collection is {total} bytes");
        foreach (var species in Presets)
        {
            var size = sizes.GetValueOrDefault(species.BarkSet ?? "") + sizes.GetValueOrDefault(species.LeafSet ?? "");
            Assert.True(size <= 3L << 20, $"a species' sets are {size} bytes");
        }
    }

    [Fact]
    public void EveryPresetFindsItsBarkAndItsLeaves()
    {
        foreach (var species in Presets.Where(species => species != TreeSpecies.Fern))
        {
            Assert.Equal(TreeTextureKind.Bark, TreeTextures.Find(species.BarkSet!)?.Kind);
            Assert.Equal(TreeTextureKind.Leaves, TreeTextures.Find(species.LeafSet!)?.Kind);
        }

        Assert.Null(TreeSpecies.Fern.BarkSet);
        Assert.Equal(TreeTextureKind.Leaves, TreeTextures.Find(TreeSpecies.Fern.LeafSet!)?.Kind);
    }

    [Fact]
    public void TheGrassCardsAreCarriedAsGrass()
    {
        foreach (var name in new[] { "wheat-grass", "grass-tuft-a", "grass-tuft-b" })
            Assert.Equal(TreeTextureKind.Grass, TreeTextures.Find(name)?.Kind);
    }

    [Fact]
    public void ALeafSetsManifestGivesEachLeafsBoxInItsAtlas()
    {
        foreach (var entry in Entries().Values.Where(entry => entry.GetProperty("kind").GetString() == "leaves"))
        {
            var boxes = entry.GetProperty("leaves").EnumerateArray().Select(box => box.EnumerateArray().Select(value => value.GetSingle()).ToArray()).ToArray();
            Assert.NotEmpty(boxes);
            Assert.All(boxes, box => Assert.True(box[0] >= 0f && box[1] >= 0f && box[2] > 0f && box[3] > 0f && box[0] + box[2] <= 1.0001f && box[1] + box[3] <= 1.0001f, $"{entry.GetProperty("name")} has a leaf at {string.Join(", ", box)}"));
            Assert.Equal(boxes.Select(box => (box[0], box[1], box[2], box[3])), TreeTextures.Find(entry.GetProperty("name").GetString()!)!.LeafBoxes);
        }
    }

    [Fact]
    public void ASetsMeanColorIsItsManifestsInLinearSrgb()
    {
        foreach (var set in Carried())
        {
            var color = Entries()[set.Name].GetProperty("color");
            float[] linear = [.. Enumerable.Range(0, 3).Select(i => color[i].GetSingle()).Select(value => value <= 0.04045f ? value / 12.92f : Exact.Pow((value + 0.055f) / 1.055f, 2.4f))];
            Assert.Equal((linear[0], linear[1], linear[2]), (set.MeanColor.X, set.MeanColor.Y, set.MeanColor.Z));
        }
    }

    [Fact]
    public void ASetsFilesAreKtx2InTheirFormatsWithEveryLevel()
    {
        // The sample's albedo, 64 square with its levels to a texel, in BC7 as sRGB, with no
        // supercompression, its normal half the size in BC5 and its surface in BC7 as data.
        Assert.Equal((146u, 64u, 64u, 7u, 0u), Header(File.ReadAllBytes(Path.Combine(Sample, "sample", "albedo.ktx2"))));
        Assert.Equal((141u, 32u, 32u, 6u, 0u), Header(File.ReadAllBytes(Path.Combine(Sample, "sample", "normal.ktx2"))));
        Assert.Equal((145u, 32u, 32u, 6u, 0u), Header(File.ReadAllBytes(Path.Combine(Sample, "sample", "surface.ktx2"))));
    }

    /// <summary>
    /// Every file the library carries says no supercompression in its header, and each of its
    /// levels is as long as its blocks uncompressed and starts at a multiple of its block's size,
    /// 16 bytes for BC7 and BC5 and 8 for BC4, so a reader with no decompressor uploads it in
    /// place.
    /// </summary>
    [Fact]
    public void EveryCarriedFileIsPlainKtx2()
    {
        var carried = TreeTextures.Files.Where(path => !path.StartsWith("added/", StringComparison.Ordinal)).ToList();
        Assert.Equal(43, carried.Count);
        foreach (var path in carried)
        {
            using var stream = TreeTextures.Open(path)!;
            using var bytes = new MemoryStream();
            stream.CopyTo(bytes);
            var file = bytes.ToArray();
            var (format, levels, scheme) = (BitConverter.ToUInt32(file, 12), BitConverter.ToUInt32(file, 40), BitConverter.ToUInt32(file, 44));
            Assert.True(scheme == 0, $"{path} is supercompressed by scheme {scheme}.");
            var align = format == 139 ? 8ul : 16ul;
            for (var level = 0; level < levels; level++)
            {
                var (offset, length, uncompressed) = (BitConverter.ToUInt64(file, 80 + 24 * level), BitConverter.ToUInt64(file, 88 + 24 * level), BitConverter.ToUInt64(file, 96 + 24 * level));
                Assert.True(length == uncompressed && offset % align == 0 && offset + length <= (ulong)file.Length, $"{path}'s level {level} lies at {offset} for {length} bytes of {uncompressed}.");
            }
        }
    }

    [Fact]
    public void EveryBarkCarriesItsDepthAndNoLeafOrGrassSetDoes()
    {
        foreach (var set in Carried())
        {
            // Leaves and grass are cut-out cards, which carry no depth.
            if (set.Kind != TreeTextureKind.Bark)
            {
                Assert.Null(set.Depth);
                continue;
            }

            Assert.Equal($"{set.Name}/depth.ktx2", set.Depth);
            var albedo = Header(File.ReadAllBytes(Path.Combine(Collection, set.Name, "albedo.ktx2")));
            var depth = Header(File.ReadAllBytes(Path.Combine(Collection, set.Name, "depth.ktx2")));
            Assert.Equal((139u, albedo.Width / 2, albedo.Height / 2), (depth.Format, depth.Width, depth.Height));
        }

        // The sample's depth, derived from its normal map, is half its 64 square, in BC4 with no
        // supercompression.
        var sample = Header(File.ReadAllBytes(Path.Combine(Sample, "sample", "depth.ktx2")));
        Assert.Equal((139u, 32u, 32u, 0u), (sample.Format, sample.Width, sample.Height, sample.Supercompression));
    }

    [Fact]
    public void EveryCarriedFileOpensAsItsFolderHoldsIt()
    {
        var carried = TreeTextures.Files.Where(path => !path.StartsWith("added/", StringComparison.Ordinal)).ToList();
        var folder = Directory.EnumerateFiles(Collection, "*.ktx2", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(Collection, file).Replace('\\', '/'))
            .Where(path => !path.StartsWith("waiting/", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);
        Assert.Equal(folder, carried);
        foreach (var path in carried)
            Assert.Equal(File.ReadAllBytes(Path.Combine(Collection, path)), Read(path));
    }

    /// <summary>The sets the module carries, those a test added apart.</summary>
    /// <summary>
    /// Each carried set's mean color is the linear mean of its albedo's opaque texels, decoded from
    /// the file's top level, within 0.001 a channel for a bark or a leaf set and 0.004 for a grass
    /// card, so a far card drawn with it is as bright as the leaves it pictures. The bounds are the
    /// gaps read with the mean taken in linear light, 0.0006 at most for the barks and leaves and
    /// 0.0032 for the grass, whose thin blades put many texels at the alpha's threshold, where the
    /// BC7 file and its source part on which are opaque.
    /// </summary>
    [Fact]
    public void ASetsMeanColorIsTheLinearMeanOfItsAlbedo()
    {
        var apart = new List<string>();
        foreach (var set in Carried())
        {
            using var stream = TreeTextures.Open(set.Albedo)!;
            using var bytes = new MemoryStream();
            stream.CopyTo(bytes);
            var (sum, opaque) = (new double[3], 0);
            foreach (var texel in TopLevel(bytes.ToArray()))
            {
                if (texel.A <= 127) continue;
                opaque++;
                sum[0] += Linear(texel.R / 255.0);
                sum[1] += Linear(texel.G / 255.0);
                sum[2] += Linear(texel.B / 255.0);
            }

            var mean = (sum[0] / opaque, sum[1] / opaque, sum[2] / opaque);
            var gap = Math.Max(Math.Abs(mean.Item1 - set.MeanColor.X), Math.Max(Math.Abs(mean.Item2 - set.MeanColor.Y), Math.Abs(mean.Item3 - set.MeanColor.Z)));
            if (gap > (set.Kind == TreeTextureKind.Grass ? 0.004 : 0.001)) apart.Add($"{set.Name}'s mean color is {set.MeanColor} where its albedo's linear mean is {mean}, {gap:F4} apart");
        }

        Assert.True(apart.Count == 0, string.Join(Environment.NewLine, apart));
    }

    // BC7's partitions of a block into two subsets, a row of its sixteen texels' subsets read left
    // to right and top to bottom, and the anchor texel of each one's second subset, from the
    // Khronos Data Format Specification's tables.
    private static readonly string[] Partitions =
    [
        "0011001100110011", "0001000100010001", "0111011101110111", "0001001100110111",
        "0000000100010011", "0011011101111111", "0001001101111111", "0000000100110111",
        "0000000000010011", "0011011111111111", "0000000101111111", "0000000000010111",
        "0001011111111111", "0000000011111111", "0000111111111111", "0000000000001111",
        "0000100011101111", "0111000100000000", "0000000010001110", "0111001100010000",
        "0011000100000000", "0000100011001110", "0000000010001100", "0111001100110001",
        "0011000100010000", "0000100010001100", "0110011001100110", "0011011001101100",
        "0001011111101000", "0000111111110000", "0111000110001110", "0011100110011100",
        "0101010101010101", "0000111100001111", "0101101001011010", "0011001111001100",
        "0011110000111100", "0101010110101010", "0110100101101001", "0101101010100101",
        "0111001111001110", "0001001111001000", "0011001001001100", "0011101111011100",
        "0110100110010110", "0011110011000011", "0110011010011001", "0000011001100000",
        "0100111001000000", "0010011100100000", "0000001001110010", "0000010011100100",
        "0110110010010011", "0011011011001001", "0110001110011100", "0011100111000110",
        "0110110011001001", "0110001100111001", "0111111010000001", "0001100011100111",
        "0000111100110011", "0011001111110000", "0010001011101110", "0100010001110111",
    ];

    private static readonly int[] SecondAnchors =
    [
        15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
        15, 2, 8, 2, 2, 8, 8, 15, 2, 8, 2, 2, 8, 8, 2, 2,
        15, 15, 6, 8, 2, 8, 15, 15, 2, 8, 2, 2, 2, 15, 15, 6,
        6, 2, 6, 8, 15, 15, 2, 2, 15, 15, 15, 15, 15, 2, 2, 15,
    ];

    // The texels of a KTX2 file's top level of BC7 blocks in mode 6, mode 1 or mode 7, the modes
    // the collection's encoder writes, a row of blocks at a time, each block's sixteen in turn.
    private static IEnumerable<(byte R, byte G, byte B, byte A)> TopLevel(byte[] file)
    {
        int[] weights4 = [0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64];
        int[] weights3 = [0, 9, 18, 27, 37, 46, 55, 64];
        int[] weights2 = [0, 21, 43, 64];
        var (offset, length) = ((int)BitConverter.ToUInt64(file, 80), (int)BitConverter.ToUInt64(file, 88));
        for (var at = offset; at < offset + length; at += 16)
        {
            var block = new System.Numerics.BigInteger(file.AsSpan(at, 16), isUnsigned: true);
            int Bits(int from, int count) => (int)((block >> from) & ((1 << count) - 1));
            if (Bits(0, 7) == 64)
            {
                var (low, high) = (new int[4], new int[4]);
                for (var channel = 0; channel < 4; channel++)
                    (low[channel], high[channel]) = ((Bits(7 + 14 * channel, 7) << 1) | Bits(63, 1), (Bits(14 + 14 * channel, 7) << 1) | Bits(64, 1));
                var place = 65;
                for (var texel = 0; texel < 16; texel++)
                {
                    var size = texel == 0 ? 3 : 4;
                    var w = weights4[Bits(place, size)];
                    place += size;
                    byte Mix(int channel) => (byte)(((64 - w) * low[channel] + w * high[channel] + 32) >> 6);
                    yield return (Mix(0), Mix(1), Mix(2), Mix(3));
                }

                continue;
            }

            if (Bits(0, 8) == 128)
            {
                // Mode 7, two subsets of a partition, each end five bits of red, green, blue and
                // alpha with a bit of its own, and two-bit indices, one bit at each anchor.
                var part = Bits(8, 6);
                var ends7 = new int[2, 2, 4];
                for (var channel = 0; channel < 4; channel++)
                for (var subset = 0; subset < 2; subset++)
                for (var end = 0; end < 2; end++)
                {
                    var six = (Bits(14 + 5 * (4 * channel + 2 * subset + end), 5) << 1) | Bits(94 + 2 * subset + end, 1);
                    ends7[subset, end, channel] = (six << 2) | (six >> 4);
                }

                var place7 = 98;
                for (var texel = 0; texel < 16; texel++)
                {
                    var subset = Partitions[part][texel] - '0';
                    var size = texel == 0 || texel == SecondAnchors[part] ? 1 : 2;
                    var w = weights2[Bits(place7, size)];
                    place7 += size;
                    byte Mix(int channel) => (byte)(((64 - w) * ends7[subset, 0, channel] + w * ends7[subset, 1, channel] + 32) >> 6);
                    yield return (Mix(0), Mix(1), Mix(2), Mix(3));
                }

                continue;
            }

            Assert.True(Bits(0, 2) == 2, $"A block at byte {at} is in none of BC7's modes 6, 1 and 7.");
            var partition = Bits(2, 6);
            var ends = new int[2, 2, 3];
            for (var channel = 0; channel < 3; channel++)
            for (var subset = 0; subset < 2; subset++)
            for (var end = 0; end < 2; end++)
                ends[subset, end, channel] = Bits(8 + 6 * (4 * channel + 2 * subset + end), 6);
            var position = 8 + 72 + 2;
            for (var texel = 0; texel < 16; texel++)
            {
                var subset = Partitions[partition][texel] - '0';
                var size = texel == 0 || texel == SecondAnchors[partition] ? 2 : 3;
                var w = weights3[Bits(position, size)];
                position += size;
                byte Mix(int channel)
                {
                    int Expand(int end)
                    {
                        var seven = (ends[subset, end, channel] << 1) | Bits(80 + subset, 1);
                        return (seven << 1) | (seven >> 6);
                    }

                    return (byte)(((64 - w) * Expand(0) + w * Expand(1) + 32) >> 6);
                }

                yield return (Mix(0), Mix(1), Mix(2), 255);
            }
        }
    }

    private static double Linear(double value) => value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);

    private static IEnumerable<TreeTextureSet> Carried() =>
        TreeTextures.All.Where(set => !set.Albedo.StartsWith("added/", StringComparison.Ordinal));

    private static Dictionary<string, JsonElement> Entries()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Collection, "textures.json")));
        return manifest.RootElement.GetProperty("entries").EnumerateArray().ToDictionary(entry => entry.GetProperty("name").GetString()!, entry => entry.Clone());
    }

    private static byte[] Read(string path)
    {
        using var stream = TreeTextures.Open(path) ?? throw new FileNotFoundException($"The collection opens nothing at {path}.");
        using var bytes = new MemoryStream();
        stream.CopyTo(bytes);
        return bytes.ToArray();
    }

    /// <summary>
    /// A KTX2 file's format, width, height, levels and supercompression, from its header.
    /// </summary>
    private static (uint Format, uint Width, uint Height, uint Levels, uint Supercompression) Header(byte[] file)
    {
        byte[] identifier = [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(identifier, file[..12]);
        uint At(int offset) => BitConverter.ToUInt32(file, offset);
        return (At(12), At(20), At(24), At(40), At(44));
    }

    /// <summary>The folder holding Vegetation.slnx, from the tests' own folder up.</summary>
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
