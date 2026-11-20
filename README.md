[Devlog](https://github.com/EggyStudio/3DEngine.Vegetation/blob/main/DEVLOG.md) [Cheatsheet](https://github.com/EggyStudio/3DEngine.Vegetation/blob/main/CHEATSHEET.md)

# Vegetation

Vegetation is a library an engine takes as a git submodule, for plant species, each one's skeleton
grown from a seed, the tree and grass meshes built through Modeling, their texture sets and their
placement. It references no engine and declares no interface for one to implement, so an engine
binds it by calling it and drawing what it returns.

## What it holds

| Type | Gives |
|---|---|
| `TreeSpecies` | What a kind of tree is, by Weber and Penn's controls of 1995, its crown's outline, its size and how much one varies from the next, its trunk and its levels of branches, its leaves, and its bark's and leaves' colors in linear sRGB, with presets for a quaking aspen, a black tupelo, a weeping willow, a California black oak, a bush and a fern, and `Check`, which refuses a control outside what it may be |
| `TreeLevel`, `TreeShape` and `TreeLeafShape` | The controls of a level's stems, how they leave the stem before, bend, fork and taper, the crown's outline among the paper's eight or its pruning envelope, and the outline of a leaf `TreeLooks` draws |
| `TreeSkeleton` and `TreeNode` | A tree's stems as points, each with the point it grew from, its place and its radius, and `TreeSkeleton.Grow`, which grows one from a species and a seed by space colonization round boxes it does not grow into, a seed growing the same points every time |
| `Pcg` | The random numbers a tree grows from, O'Neill's PCG, the same for a seed on every runtime, which the grass is scattered and the trees are placed from, and an engine draws a tree's leaves from as well |
| `TreePlanting` and `TreeStance` | Where trees stand, a forest's trees spread over a disc from a seed, none nearer another than an even spread allows, each with a seed of its own, and how a tree stands by its seed, which of its species' variants it is, its turn and its size, with the variants' count and the seed each variant grows from |
| `TreeTextures`, `TreeTextureSet` and `TreeTextureKind` | The texture collection the module carries in its assembly, bark, leaf and grass sets found by name, each set's files by their paths with the mean color and surface its manifest measured and a leaf atlas's boxes, and a game's own folders of sets added beside them |
| `TreeLooks` | The bark's color, normal and depth and a leaf atlas of four leaves of a shape, made in code where a species names no set, the same each time and white or gray for the species' colors to tint, with the boxes of the atlas's leaves |
| `TreeLeaves`, `TreeLeafMesh` and `TreeDetail` | A grown tree's leaves as vertex arrays at two levels of detail, a card a leaf along its twigs or a frond along each stem, each vertex's place, normal, texture coordinates, sky, way out and sway, with the triangles and each card's twig |
| `TreeLimbs` | Which of a tree's sixteen limbs each stem sways with and where each limb pivots, with a phase a limb or a leaf |
| `TreeStems` and `TreeStem` | A tree's stems as a tube needs them, each one's points from the joint on its parent, its radii, how many times its bark goes round it and how far along the bark's picture each point stands, the bark's texels keeping their shape the length of a tapering stem |
| `TreeCrown` | A crown as a volume of its leaves, from which each leaf takes the share of the sky it sees through the leaves round it and the way out of the crown at it, so a leaf deep in the crown is drawn darker than one on its rim |
| `GrassScatter`, `GrassSettings`, `GrassPatch` and `GrassClump` | Where grass stands over a stretch of ground, the stretch cut into square patches an engine draws as a mesh each, and each patch's clumps scattered a cell each by the patch's own seed, a clump's root, turn, height and phases in the wind, with the settings a stretch is planted with |

## Taking it into an engine

An engine adds the repository as a git submodule under `modules/` and references its project, here
from a project one folder below the engine's root.

```bash
git submodule add https://github.com/EggyStudio/3DEngine.Vegetation.git modules/Vegetation
```

```xml
<ProjectReference Include="..\modules\Vegetation\Vegetation\Vegetation.csproj" />
```

The project builds the assembly `3DEngine.Vegetation`, whose types are in the namespace
`Vegetation`.

A clone of the engine fetches the module with `git clone --recurse-submodules`, or with
`git submodule update --init` in a clone made without it, and a workflow checks it out with
`submodules: true` on `actions/checkout`. A change the engine uses is committed and pushed here
before the engine's commit that moves its pointer, so the engine's history never names a commit
this repository does not have.

## Binding it in your engine

The module declares no interface for an engine to implement. It reads nothing but its own assembly
and the folders a game hands `TreeTextures.Add`, and each of its types gives data an engine makes
its own things of, points and radii it builds stems from, places and seeds it plants at, texture
files and pictures it uploads, and shades it writes into a leaf's vertices. Binding it is calling
it and drawing what it returns, with no window, scheduler or graphics API of the module's.

| Quantity | Convention |
|---|---|
| Length | Meters |
| Axes | Right-handed with Y up, a tree standing up Y from the origin and a field's ground at Y 0 |
| A tree's turn | Radians about Y, counterclockwise seen from above, as `Quaternion.CreateFromAxisAngle(Vector3.UnitY, turn)` turns it |
| A clump's turn | Radians, its first card lying along X cos turn and Z sin turn, the second a quarter turn on |
| A species' angles | Degrees, as Weber and Penn give their controls |
| Color | Linear sRGB in a `Vector3`, a species' colors and a set's mean color |
| Pictures | RGBA, a row at a time from the top, the colors sRGB-encoded and the normals and depths linear |
| Normal maps | Tangent space, X toward the picture's right and Y toward its top row, as OpenGL and glTF read them |
| Depth maps | 255 the deepest, a depth below the surface for parallax occlusion rather than a height |
| Texture files | KTX2 with every mipmap level and no supercompression, each level's blocks as a GPU samples them, BC7 for color and surface, BC5 for normals, BC4 for depth |
| Light | None of the module's. `TreeCrown.Shade` gives a share of the sky from 0 to 1, which an engine scales by its own light |
| A stem's bark | A whole count round, the trunk's nearest its circumference over the set's repeat round and a branch's nearest its parent's times its radius over the parent's at the joint, and a coordinate along in repeats, each repeat as long as the circumference over the count in the picture's proportion, so it shortens with the radius |
| A leaf vertex's place and normal | Meters, Y up, from the tree's root at the origin, and a normal at unit length, the way its card faces, a card counterclockwise seen from its face |
| A leaf vertex's texture coordinates | 0 to 1 in the atlas, U from its left and V from its top, a card's stalk at its box's bottom |
| A leaf vertex's sky | 0 to 1, the share of the sky that reaches its card through the crown, the same at a card's four corners, absent for fronds; an engine darkens the leaf by it to a floor of its own |
| A leaf vertex's way out | A unit vector, the way out of the crown at its card, absent for fronds; an engine bends the leaf's diffuse light toward it |
| A leaf vertex's limb and own pivots | Meters in X, Y and Z and a phase from 0 up to 1 in W, the limb's where it leaves its parent and the leaf's at its stalk, or a frond's foot; an engine's wind moves the vertex about them |
| Random numbers | `Pcg` from a seed, the same on every runtime, so a seed plants and grows the same in every engine |

The smallest binding plants a forest of a species and a field of grass. It is the body of
`Bind` in `Vegetation.Tests/BindingTests.cs`, which the tests run against a host that records what
an engine would draw, and a test holds this block the same as that body. `host` is the engine's
side, which builds a stem as a tube round its points with its bark wrapped by its count round and its coordinates along, loads a texture file with its KTX2 loader, uploads
a picture as an image and draws a clump as two cards crossed about its root. The code takes
`using System.Numerics;` and `using Vegetation;`.

```csharp
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

// Grass over a field of 40 by 40 meters about the origin, a clump at a time.
var grass = new GrassSettings();
foreach (var patch in GrassScatter.Patches(new Vector2(40f, 40f), grass))
{
    var middle = new Vector3(patch.Middle.X, 0f, patch.Middle.Y);
    foreach (var clump in GrassScatter.Clumps(grass, patch.Size, patch.Seed))
        host.Clump(middle + clump.Root, clump.Turn, clump.Height);
}
```

A leaf's cards are the engine's to build until the module carries them, and `TreeCrown.Shade`
shades them from their middles and areas.

## Building and testing

With the .NET 10 SDK:

```bash
dotnet build Vegetation.slnx
dotnet test Vegetation.Tests
```

## The texture collection

`Vegetation/Textures` holds the bark, leaf and grass sets the module carries in its assembly, which
`TreeTextures` finds by name, each three KTX2 files in BC7 and BC5 with their mipmaps and no
supercompression, and a bark a fourth, its depth in BC4. The Materials module's
`scripts/textures.py` makes them from the sources sorted under the folder `--sources` names, which
the repository leaves out, by the recipe the collection's `textures.json` gives each set, the crop,
the size, the maps packed into each file and any change of color, so a set is made again from its
source byte for byte. From the module's root with a checkout of Materials beside it, and Python 3.14
and Pillow:

```bash
python3 -I -B ../Materials/scripts/textures.py Vegetation/Textures/textures.json --sources DIR
python3 -I -B ../Materials/scripts/textures.py Vegetation/Textures/textures.json --sources DIR fern-leaves
python3 -I -B ../Materials/scripts/textures.py --sample Vegetation.Tests/references/textures
```

A set the manifest marks waiting is imported into `waiting/`, which the repository and the build
leave out, and `TreeTextures.Add` reads it there for a look on the machine that imported it. A set
is at most 1024 square, a species' bark and leaves at most 3 MB together and the collection at most
32 MB, which `TreeTexturesTests` holds.

## License

The Mozilla Public License 2.0, in [LICENSE](LICENSE).
