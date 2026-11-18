// Writes Vegetation.Tests/references/leaves.json, the hashes of the leaves an engine built before
// they moved into the module, a hash an array for each preset, seed and level of detail, which
// TreeLeavesTests holds the module's leaves to.
//
// Usage:
//   dotnet run --project build/leaves-fixture -- BIN COMMIT [OUT]
//
// BIN is the folder of an engine's build holding the assembly whose Bevy.Trees built the leaves,
// BevyCSharp.Vegetation.dll, and COMMIT the engine's commit it was built at, written into the
// fixture. The engine's private card builder, limb plan and crown volume are called by reflection,
// so nothing here or in the module is compiled against the engine.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Vegetation;

var (bin, commit) = (args[0], args[1]);
var output = args.Length > 2 ? args[2] : Path.Combine("Vegetation.Tests", "references", "leaves.json");
AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
{
    var name = new AssemblyName(e.Name).Name;
    if (name is "Vegetation" or "3DEngine.Vegetation") return typeof(TreeLeaves).Assembly;
    var path = Path.Combine(bin, name + ".dll");
    return File.Exists(path) ? Assembly.LoadFrom(path) : null;
};

const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
var trees = Assembly.LoadFrom(Path.Combine(bin, "BevyCSharp.Vegetation.dll")).GetType("Bevy.Trees", true)!;
var makeCards = trees.GetMethod("MakeCards", Any)!;
var planLimbs = trees.GetMethod("PlanLimbs", Any)!;
var crownVolume = trees.GetMethod("CrownVolume", Any)!;
var vec3 = crownVolume.GetParameters()[0].ParameterType.GetGenericArguments()[0];

TreeSpecies[] presets = [TreeSpecies.QuakingAspen, TreeSpecies.BlackTupelo, TreeSpecies.WeepingWillow, TreeSpecies.CaliforniaBlackOak, TreeSpecies.Bush, TreeSpecies.Fern];
string[] names = ["QuakingAspen", "BlackTupelo", "WeepingWillow", "CaliforniaBlackOak", "Bush", "Fern"];
var cases = new List<Dictionary<string, object>>();
for (var p = 0; p < presets.Length; p++)
foreach (var seed in (uint[])[1, 2, 3, 4])
foreach (var detail in (string[])["Full", "Reduced"])
{
    var (species, skeleton) = (presets[p], TreeSkeleton.Grow(presets[p], seed));
    var cards = makeCards.Invoke(null, [skeleton, species, seed])!;
    cards.GetType().GetMethod("Sway")!.Invoke(cards, [planLimbs.Invoke(null, [skeleton])]);
    var full = cards.GetType().GetMethod("Data")!.Invoke(cards, null)!;
    var mesh = detail == "Reduced" && species.Leaves > 0 ? cards.GetType().GetMethod("Halved")!.Invoke(cards, null)! : full;
    var tips = ((IReadOnlyList<int>)cards.GetType().GetProperty("Tips")!.GetValue(cards)!).ToArray();

    float[] Floats(object data, string name) => data.GetType().GetProperty(name)!.GetValue(data) switch
    {
        null => [],
        float[] floats => floats,
        Array vectors => Raw(vectors),
        var other => throw new InvalidOperationException($"{name} is {other.GetType()}."),
    };
    byte[] Stream(object data, int index) =>
        data.GetType().GetProperty("Streams")!.GetValue(data) is Array streams
            ? (byte[])streams.GetValue(index)!.GetType().GetProperty("Data")!.GetValue(streams.GetValue(index))!
            : [];

    // The sky and the way out the engine shaded each card by, from its crown volume over the full
    // level's cards, the sky held at one as the module holds it, each card's at its four corners.
    var (sky, outward) = (Array.Empty<float>(), Array.Empty<float>());
    if (species.Leaves > 0)
    {
        var positions = Floats(full, "Positions");
        var count = positions.Length / 12;
        var (places, areas) = (new float[count * 3], new float[count]);
        for (var card = 0; card < count; card++)
        {
            float P(int corner, int axis) => positions[(card * 4 + corner) * 3 + axis];
            for (var axis = 0; axis < 3; axis++) places[card * 3 + axis] = (P(0, axis) + P(1, axis) + P(2, axis) + P(3, axis)) * 0.25f;
            var (ax, ay, az) = (P(1, 0) - P(0, 0), P(1, 1) - P(0, 1), P(1, 2) - P(0, 2));
            var (bx, by, bz) = (P(3, 0) - P(0, 0), P(3, 1) - P(0, 1), P(3, 2) - P(0, 2));
            var (cx, cy, cz) = (ay * bz - az * by, az * bx - ax * bz, ax * by - ay * bx);
            areas[card] = MathF.Sqrt(cx * cx + cy * cy + cz * cz);
        }

        var result = crownVolume.Invoke(null, [Vectors(places), areas])!;
        var cardSky = (float[])result.GetType().GetField("Item1")!.GetValue(result)!;
        var cardOut = Raw((Array)result.GetType().GetField("Item2")!.GetValue(result)!);
        var keep = detail == "Reduced" ? Enumerable.Range(0, count / 2).Select(card => card * 2).ToArray() : Enumerable.Range(0, count).ToArray();
        sky = [.. keep.SelectMany(card => Enumerable.Repeat(MathF.Min(cardSky[card], 1f), 4))];
        outward = [.. keep.SelectMany(card => Enumerable.Repeat(cardOut[(card * 3)..(card * 3 + 3)], 4).SelectMany(way => way))];
    }

    var twigs = detail == "Reduced" && species.Leaves > 0 ? [.. Enumerable.Range(0, tips.Length / 2).Select(card => tips[card * 2])] : tips;
    cases.Add(new()
    {
        ["preset"] = names[p], ["seed"] = seed, ["detail"] = detail,
        ["vertices"] = Floats(mesh, "Positions").Length / 3,
        ["positions"] = Hash(Floats(mesh, "Positions")),
        ["normals"] = Hash(Floats(mesh, "Normals")),
        ["uvs"] = Hash(Floats(mesh, "Uvs")),
        ["sky"] = Hash(sky),
        ["out"] = Hash(outward),
        ["limb"] = Hash(Stream(mesh, 0)),
        ["own"] = Hash(Stream(mesh, 1)),
        ["indices"] = Hash((uint[])mesh.GetType().GetProperty("Indices")!.GetValue(mesh)!),
        ["twigs"] = Hash(twigs),
    });
    Console.WriteLine($"{names[p]} {seed} {detail}, {cases[^1]["vertices"]} vertices");
}

var fixture = new Dictionary<string, object>
{
    ["engine"] = $"BevyCSharp {commit}",
    ["made"] = "dotnet run --project build/leaves-fixture -- BIN COMMIT",
    ["cases"] = cases,
};
File.WriteAllText(output, JsonSerializer.Serialize(fixture, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"{cases.Count} cases written to {output}");

// The floats of an array of the engine's vectors, three a vector as it lays them out.
static float[] Raw(Array vectors)
{
    var floats = new float[vectors.Length * Marshal.SizeOf(vectors.GetType().GetElementType()!) / 4];
    var handle = GCHandle.Alloc(vectors, GCHandleType.Pinned);
    try { Marshal.Copy(handle.AddrOfPinnedObject(), floats, 0, floats.Length); }
    finally { handle.Free(); }
    return floats;
}

// An array of the engine's vectors from floats, three a vector.
Array Vectors(float[] floats)
{
    var vectors = Array.CreateInstance(vec3, floats.Length / 3);
    var handle = GCHandle.Alloc(vectors, GCHandleType.Pinned);
    try { Marshal.Copy(floats, 0, handle.AddrOfPinnedObject(), floats.Length); }
    finally { handle.Free(); }
    return vectors;
}

static string Hash<T>(T[] values) where T : unmanaged => Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(values.AsSpan())));
