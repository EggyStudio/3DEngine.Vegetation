using System.Numerics;
using Materials;

namespace Vegetation;

/// <summary>
/// The vegetation's texture collection, the bark, leaf and grass sets the module carries by name,
/// which a species names for its bark and its leaves and a game may add its own to.
/// </summary>
/// <remarks>
/// <para>
/// The module carries each set it was built with inside its assembly, three KTX2 files a set and a
/// bark's depth a fourth, that the Materials module's <c>scripts/textures.py</c> imports by a
/// recipe in its manifest, which its <see cref="TextureCollection"/> reads, so a game ships no
/// folder for them. An engine reads every file by its
/// path in the collection, which <see cref="Files"/> lists and <see cref="Open"/> opens, and loads
/// a set's images from those paths with its own loader. A tree whose species names a set the
/// collection does not hold is drawn with the bark and leaves <see cref="TreeLooks"/> makes.
/// </para>
/// <para>
/// A game adds a folder of its own sets with <see cref="Add"/>, as the import writes them, each in
/// a folder of its name beside a <c>textures.json</c> naming them, and a set of the same name as
/// one the module carries stands in its place.
/// </para>
/// </remarks>
public static class TreeTextures
{
    // What the module's resources are named from, the manifest and each set's files.
    private const string Prefix = "Vegetation/Textures/";

    private static readonly Lock Gate = new();
    private static readonly List<string> Added = [];
    private static Dictionary<string, TreeTextureSet>? _sets;

    // Each file's path in the collection and where it is read from, a resource's name for a set the
    // module carries and a file on this machine for a set of a folder added.
    private static readonly Dictionary<string, (bool Carried, string From)> Places = new(StringComparer.Ordinal);
    private static IReadOnlyList<string> _files = [];

    /// <summary>Every set, the module's and those added, in the order of their names.</summary>
    public static IReadOnlyList<TreeTextureSet> All
    {
        get
        {
            lock (Gate) return [.. Sets().Values.OrderBy(set => set.Name, StringComparer.Ordinal)];
        }
    }

    /// <summary>
    /// The path in the collection of every file of every set, the module's and those of the folders
    /// added, in order.
    /// </summary>
    /// <remarks>
    /// The list is made again as a folder is added and is otherwise the same list, so an engine
    /// that keeps the list it read the files by tells by it whether it reads every one.
    /// </remarks>
    public static IReadOnlyList<string> Files
    {
        get
        {
            lock (Gate)
            {
                Sets();
                return _files;
            }
        }
    }

    /// <summary>The set of a name, or nothing where the collection has none.</summary>
    /// <param name="name">The set's name, as <c>silver-birch-bark</c>.</param>
    public static TreeTextureSet? Find(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        lock (Gate) return Sets().GetValueOrDefault(name);
    }

    /// <summary>
    /// Opens a file of the collection by its path, as a set gives it, or answers nothing where the
    /// collection holds no file of that path.
    /// </summary>
    /// <param name="path">
    /// The file's path in the collection, as <c>silver-birch-bark/albedo.ktx2</c>.
    /// </param>
    /// <exception cref="FileNotFoundException">
    /// The file of a folder added has gone from where it was when the folder was added.
    /// </exception>
    public static Stream? Open(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        (bool Carried, string From) place;
        lock (Gate)
        {
            Sets();
            if (!Places.TryGetValue(path, out place)) return null;
        }

        return place.Carried ? typeof(TreeTextures).Assembly.GetManifestResourceStream(place.From) : File.OpenRead(place.From);
    }

    /// <summary>
    /// Adds the sets of a folder on this machine, each in a folder of its name beside a
    /// <c>textures.json</c> naming them.
    /// </summary>
    /// <param name="directory">
    /// The folder, as the Materials module's <c>scripts/textures.py --out</c> writes one.
    /// </param>
    /// <returns>The sets it added, in the order the folder names them.</returns>
    /// <exception cref="InvalidDataException">
    /// The folder's <c>textures.json</c> is missing or names a set none of whose files are there.
    /// </exception>
    /// <remarks>
    /// The files are read where they are, at paths below <c>added/</c> and the folder's number, and
    /// a set of the same name as one the collection has stands in its place, so a game's own bark
    /// replaces the module's of that name.
    /// </remarks>
    public static IReadOnlyList<TreeTextureSet> Add(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var full = Path.GetFullPath(directory);
        var manifest = Path.Combine(full, "textures.json");
        if (!File.Exists(manifest)) throw new InvalidDataException($"{full} holds no textures.json naming its sets, which the texture recipe writes beside them.");

        lock (Gate)
        {
            var known = Sets();
            var under = $"added/{Added.Count}/";
            var sets = Listed(File.ReadAllText(manifest), carried: false, file => File.Exists(Path.Combine(full, file)), manifest);
            Added.Add(full);
            foreach (var set in sets)
            {
                known[set.Name] = set with
                {
                    Albedo = under + set.Albedo,
                    Normal = under + set.Normal,
                    Surface = under + set.Surface,
                    Depth = set.Depth is { } depth ? under + depth : null,
                };
                foreach (var file in FilesOf(set)) Places[under + file] = (false, Path.Combine(full, file));
            }

            _files = [.. Places.Keys.Order(StringComparer.Ordinal)];
            return [.. sets.Select(set => known[set.Name])];
        }
    }

    /// <summary>
    /// The module's own sets, read once from its manifest, and those added since.
    /// </summary>
    private static Dictionary<string, TreeTextureSet> Sets()
    {
        if (_sets is { } known) return known;

        var assembly = typeof(TreeTextures).Assembly;
        var names = assembly.GetManifestResourceNames().ToDictionary(name => name.Replace('\\', '/'), StringComparer.Ordinal);
        if (!names.TryGetValue(Prefix + "textures.json", out var listing))
            throw new InvalidDataException("The module carries no manifest of its texture collection, Vegetation/Textures/textures.json.");
        using var reader = new StreamReader(assembly.GetManifestResourceStream(listing)!);

        // A set the manifest names whose files the module does not carry is one the manifest marks
        // waiting, and is left out.
        var sets = Listed(reader.ReadToEnd(), carried: true, file => names.ContainsKey(Prefix + file), "the module's textures.json");
        foreach (var file in sets.SelectMany(FilesOf)) Places[file] = (true, names[Prefix + file]);
        _files = [.. Places.Keys.Order(StringComparer.Ordinal)];
        return _sets = sets.ToDictionary(set => set.Name, StringComparer.Ordinal);
    }

    private static IEnumerable<string> FilesOf(TreeTextureSet set) =>
        set.Depth is { } depth ? [set.Albedo, set.Normal, set.Surface, depth] : [set.Albedo, set.Normal, set.Surface];

    /// <summary>
    /// The sets a manifest names whose three files are there, each file at the path the Materials
    /// module's <see cref="TextureCollection"/> gives it.
    /// </summary>
    private static List<TreeTextureSet> Listed(string json, bool carried, Func<string, bool> exists, string what)
    {
        IReadOnlyList<TextureEntry> entries;
        try
        {
            entries = TextureCollection.Read(json, "textures.json");
        }
        catch (InvalidDataException fault)
        {
            throw new InvalidDataException($"The manifest {what} is not read, {fault.Message}", fault);
        }

        var sets = new List<TreeTextureSet>();
        foreach (var entry in entries)
        {
            var kind = entry.Kind switch
            {
                TextureKind.Bark => TreeTextureKind.Bark,
                TextureKind.Leaves => TreeTextureKind.Leaves,
                TextureKind.Grass => TreeTextureKind.Grass,
                var other => throw new InvalidDataException($"{what} gives the set {entry.Name} the kind {other}, which is not bark, leaves or grass."),
            };

            string[] files = [entry.Albedo.Path, entry.Normal.Path, entry.Surface.Path];
            var present = files.Count(exists);
            if (present == 0 && carried) continue;
            if (present < files.Length) throw new InvalidDataException($"{what} names the set {entry.Name}, and {files.Length - present} of its three files are not there.");

            sets.Add(new TreeTextureSet(entry.Name, kind, files[0], files[1], files[2])
            {
                Size = entry.Size,
                Meters = entry.Meters,
                Depth = entry.Depth is { } depth && exists(depth.Path) ? depth.Path : null,
                MeanColor = entry.MeanColor ?? Vector3.One,
                MeanSurface = entry.MeanSurface is { } surface ? (surface.X, surface.Y, surface.Z, surface.W) : (1f, 1f, 0f, 0f),
                LeafBoxes = entry.LeafBoxes.Count > 0 ? entry.LeafBoxes : [(0f, 0f, 1f, 1f)],
            });
        }

        return sets;
    }
}
