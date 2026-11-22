// Writes what the README's captures are drawn from: the skeleton, the leaf mesh and the sets of
// four presets grown at seed 1, and the clumps and blades of two patches of grass, each as plain
// text beside the sets' albedo files, for scripts/captures.py to draw. It takes the folder to write
// into.
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Vegetation;

var folder = args[0];
Directory.CreateDirectory(folder);

(string Name, TreeSpecies Species)[] presets =
[
    ("QuakingAspen", TreeSpecies.QuakingAspen),
    ("BlackTupelo", TreeSpecies.BlackTupelo),
    ("WeepingWillow", TreeSpecies.WeepingWillow),
    ("CaliforniaBlackOak", TreeSpecies.CaliforniaBlackOak),
];

var index = new List<object>();
foreach (var (name, species) in presets)
{
    var skeleton = TreeSkeleton.Grow(species, 1);
    var leaves = TreeLeaves.Build(skeleton, species, 1);
    File.WriteAllLines(Path.Combine(folder, $"{name}-nodes.tsv"),
        skeleton.Nodes.Select(n => string.Join('\t', n.Parent, F(n.Place.X), F(n.Place.Y), F(n.Place.Z), F(n.Radius))));
    File.WriteAllLines(Path.Combine(folder, $"{name}-vertices.tsv"),
        leaves.Positions.Select((p, i) => string.Join('\t', F(p.X), F(p.Y), F(p.Z), F(leaves.Uvs[i].X), F(leaves.Uvs[i].Y))));
    File.WriteAllLines(Path.Combine(folder, $"{name}-indices.tsv"),
        leaves.Indices.Select(i => i.ToString(CultureInfo.InvariantCulture)));
    index.Add(new
    {
        name,
        bark = Describe(species.BarkSet),
        leaves = Describe(species.LeafSet),
        barkColor = new[] { species.BarkColor.X, species.BarkColor.Y, species.BarkColor.Z },
    });
}
File.WriteAllText(Path.Combine(folder, "presets.json"), JsonSerializer.Serialize(index));

// The two patches of grass, the lawn's near the camera and a closer, smaller one, each as its
// clumps for the far cards and its blades' arrays for the near level, and the sheet's one patch
// grown across the seasons, the health and the thickness.
var grass = new GrassSettings { Seed = 3 };
DumpGrass("grass", grass, new Vector2(2.4f, 0.6f), 5);
DumpGrass("close", grass, new Vector2(0.35f, 0.25f), 9);
var sheet = new List<object>();
foreach (var (label, settings) in SheetVariants(grass))
{
    var name = $"sheet-{sheet.Count}";
    DumpGrass(name, settings, new Vector2(0.5f, 0.35f), 9);
    sheet.Add(new { name, label });
}
File.WriteAllText(Path.Combine(folder, "sheet.json"), JsonSerializer.Serialize(sheet));
var grassSet = TreeTextures.Find(grass.Set)!;
var bladeSet = TreeTextures.Find(GrassBlades.Set)!;
Copy(grassSet.Albedo);
Copy(bladeSet.Albedo);
Copy(bladeSet.Normal);
Copy(bladeSet.Surface);
File.WriteAllText(Path.Combine(folder, "grass.json"), JsonSerializer.Serialize(new
{
    albedo = grassSet.Albedo,
    size = new[] { grassSet.Size?.Width ?? 0, grassSet.Size?.Height ?? 0 },
    blade = new { albedo = bladeSet.Albedo, normal = bladeSet.Normal, surface = bladeSet.Surface },
}));

void DumpGrass(string name, GrassSettings settings, Vector2 size, uint seed)
{
    var clumps = GrassScatter.Clumps(settings, size, seed);
    File.WriteAllLines(Path.Combine(folder, $"{name}-clumps.tsv"),
        clumps.Select(c => string.Join('\t', F(c.Root.X), F(c.Root.Z), F(c.Turn), F(c.Height))));
    var mesh = GrassBlades.Write(settings, size, seed, 5);
    File.WriteAllLines(Path.Combine(folder, $"{name}-mesh.tsv"),
        mesh.Positions.Select((p, i) => string.Join('\t', F(p.X), F(p.Y), F(p.Z),
            F(mesh.Normals[i].X), F(mesh.Normals[i].Y), F(mesh.Normals[i].Z),
            F(mesh.Colors[i].X), F(mesh.Colors[i].Y), F(mesh.Colors[i].Z),
            F(mesh.Uvs[i].X), F(mesh.Uvs[i].Y))));
    File.WriteAllLines(Path.Combine(folder, $"{name}-mesh-indices.tsv"),
        mesh.Indices.Select(i => i.ToString(CultureInfo.InvariantCulture)));
}

// The sheet's rows: the four seasons, then the health from parched to lush, then the thickness
// from sparse to a closed mat, each changing only its own number from the lawn in autumn,
// healthy and thick.
static IEnumerable<(string, GrassSettings)> SheetVariants(GrassSettings lawn)
{
    foreach (var (label, season) in new[] { ("spring", GrassCondition.Spring), ("summer", GrassCondition.Summer),
                 ("autumn", GrassCondition.Autumn), ("winter", GrassCondition.Winter) })
        yield return ($"season {label}", lawn with { Season = season });
    foreach (var health in new[] { 0f, 0.5f, 1f })
        yield return ($"health {health:0.0}", lawn with { Health = health });
    foreach (var thickness in new[] { 0f, 0.5f, 1f })
        yield return ($"thickness {thickness:0.0}", lawn with { Thickness = thickness });
}

// A set as the drawing reads it, its albedo copied beside, or only its name where the collection
// has none of that name and the species is drawn in its own color.
object Describe(string? setName)
{
    var set = setName is null ? null : TreeTextures.Find(setName);
    if (set is null) return new { name = setName, found = false };
    Copy(set.Albedo);
    return new
    {
        name = setName,
        found = true,
        albedo = set.Albedo,
        mean = new[] { set.MeanColor.X, set.MeanColor.Y, set.MeanColor.Z },
    };
}

void Copy(string path)
{
    var target = Path.Combine(folder, "sets", path);
    if (File.Exists(target)) return;
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    using var from = TreeTextures.Open(path)!;
    using var to = File.Create(target);
    from.CopyTo(to);
}

static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);
