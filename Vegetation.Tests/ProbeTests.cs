using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The arithmetic a tree is grown and leafed by, read on the machine that runs the tests against
/// the machine that wrote <c>references/probes.json</c>, so a run on another system names the first
/// function or stage whose bits part.
/// </summary>
/// <remarks>
/// Each probe hashes a function of the runtime's over a fixed sweep of arguments, a reduction of
/// System.Numerics over fixed vectors, or a stage of a preset's growth and leaves. A probe that
/// differs is a function whose result the machine's library or processor rounds apart, and the
/// stages after it read whether it reaches a tree.
/// </remarks>
public sealed class ProbeTests
{
    /// <summary>Every probe hashes as the machine that wrote the probes' file read it.</summary>
    [Fact]
    public void TheArithmeticIsTheProbesMachines()
    {
        var path = Path.Combine(RepositoryRoot(), "Vegetation.Tests", "references", "probes.json");
        var made = Probes();
        if (Environment.GetEnvironmentVariable("VEGETATION_WRITE_PROBES") == "1")
            File.WriteAllText(path, JsonSerializer.Serialize(made, new JsonSerializerOptions { WriteIndented = true }) + "\n");

        var kept = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
        var differ = made.Where(probe => kept.GetValueOrDefault(probe.Key) != probe.Value)
            .Select(probe => $"{probe.Key} is {probe.Value} where {kept.GetValueOrDefault(probe.Key)}").ToList();
        Assert.True(differ.Count == 0, $"{RuntimeInformation.OSDescription}, {RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} cores{Environment.NewLine}{string.Join(Environment.NewLine, differ)}");
    }

    private static SortedDictionary<string, string> Probes()
    {
        var probes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var random = new Pcg(0x9E57);
        var unit = Enumerable.Range(0, 20_000).Select(_ => random.Unit()).ToArray();
        var signed = unit.Select(value => value * 2f - 1f).ToArray();
        var angles = unit.Select(value => (value * 2f - 1f) * 40f).ToArray();

        probes["math.sin"] = Hash(angles.Select(MathF.Sin));
        probes["math.cos"] = Hash(angles.Select(MathF.Cos));
        probes["math.sincos"] = Hash(angles.SelectMany(angle => { var (sin, cos) = MathF.SinCos(angle); return new[] { sin, cos }; }));
        probes["math.pow"] = Hash(unit.Select(value => MathF.Pow(value * 3f, 0.75f)));
        probes["math.pow100"] = Hash(unit.Select(value => MathF.Pow(100f, value)));
        probes["math.exp"] = Hash(unit.Select(value => MathF.Exp(-20f * value)));
        probes["math.acos"] = Hash(signed.Select(MathF.Acos));
        probes["math.atan2"] = Hash(signed.Zip(angles, MathF.Atan2));
        probes["math.sqrt"] = Hash(unit.Select(value => MathF.Sqrt(value * 1000f)));

        var vectors = Enumerable.Range(0, 10_000).Select(i => new Vector3(signed[i], signed[(i + 7) % signed.Length], signed[(i + 13) % signed.Length]) * 5f).ToArray();
        probes["numerics.dot"] = Hash(vectors.Zip(vectors.Skip(1), Vector3.Dot));
        probes["numerics.cross"] = Hash(vectors.Zip(vectors.Skip(1), Vector3.Cross).SelectMany(v => new[] { v.X, v.Y, v.Z }));
        probes["numerics.lengthsquared"] = Hash(vectors.Select(v => v.LengthSquared()));
        probes["numerics.length"] = Hash(vectors.Select(v => v.Length()));
        probes["numerics.normalize"] = Hash(vectors.Select(Vector3.Normalize).SelectMany(v => new[] { v.X, v.Y, v.Z }));
        probes["numerics.transform"] = Hash(vectors.Select(v => Vector3.Transform(v, Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.7f))).SelectMany(v => new[] { v.X, v.Y, v.Z }));

        (string Name, TreeSpecies Species)[] presets = [("QuakingAspen", TreeSpecies.QuakingAspen), ("BlackTupelo", TreeSpecies.BlackTupelo), ("WeepingWillow", TreeSpecies.WeepingWillow), ("CaliforniaBlackOak", TreeSpecies.CaliforniaBlackOak), ("Bush", TreeSpecies.Bush), ("Fern", TreeSpecies.Fern)];
        foreach (var (name, species) in presets)
        foreach (var seed in (uint[])[1, 2])
        {
            var skeleton = TreeSkeleton.Grow(species, seed);
            probes[$"tree.{name}.{seed}.nodes"] = skeleton.Nodes.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            probes[$"tree.{name}.{seed}.places"] = Hash(skeleton.Nodes.SelectMany(node => new[] { node.Place.X, node.Place.Y, node.Place.Z }));
            probes[$"tree.{name}.{seed}.radii"] = Hash(skeleton.Nodes.Select(node => node.Radius));
            var leaves = TreeLeaves.Build(skeleton, species, seed);
            probes[$"tree.{name}.{seed}.leaves"] = leaves.Positions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            probes[$"tree.{name}.{seed}.uvs"] = Hash(leaves.Uvs.SelectMany(uv => new[] { uv.X, uv.Y }));
        }

        return probes;
    }

    private static string Hash(IEnumerable<float> values) => Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(values.ToArray().AsSpan())))[..16];

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
