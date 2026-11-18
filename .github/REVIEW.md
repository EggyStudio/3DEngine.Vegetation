# Review

Reviewed up to `2fba808`, item 8, the planting's placement, pushed at 04:16 on 2026-11-09, its run
green at 04:17 on the three systems in under a minute each; it keeps the form and references no
engine. Nothing is pushed after it. Before it `d604e4d`, item 7, the grass's scatter, pushed at
02:42 on 2026-11-09, its run green at 02:43 on the three systems in under a minute each; it keeps
the form and references no engine. Before it `f18a993`, item 6, the texture sets with their recipe,
carried in the module's assembly and opened by their paths, pushed at 01:01 on 2026-11-09, its run
green at 01:01 on the three systems in under a minute each; it keeps the form and references no
engine. Before it `c2149be`, item 5, the species and the skeleton by the BevyCSharp session, with
the module's DEVLOG.md, pushed at 22:36 on 2026-11-08, its run green at 22:37 on the three systems
in under a minute each; it keeps the form and references no engine. Before it `63fc628`, item 2,
pushed at 12:31 on 2026-10-31 with `41b4925`, item 1, after the owner's first commit, its run green
at 12:35 on the three systems in about three minutes, the first of this repository. Both commits
keep the form, the three-mark subject, one sentence, the owner's identity as author and committer,
no trailer (N 7.2 and N 7.6 kept at the first commit); the library references no engine (N 2.1). The
documents are the engines' with the engine's name replaced and the engine-only lines dropped,
COMMITS.md the game's with N 7.1's pointer rule in place of the legacy branch line, .gitignore with
the dump folder, .claude and .ref; AGENTS.md's devlog row carries no link until DEVLOG.md exists and
its table of areas gains the rows item 2 adds. The skeleton sets ImportDirectoryBuildTargets false,
since a submodule under modules/ would inherit an engine's Directory.Build.targets, makes a missing
summary an error, loads the assembly by name in its one test, and build/version.sh prints 0.1.0; the
strict build 0 warnings and 0 errors, the test passing. After it `7ec59cf`, the norm alone, the
seven repositories named, pushed at 14:51 on 2026-11-03, with no run since the build passes over the
review's files; it keeps the form. Before it `472e7fa`, the owner's first commit alone.

## Now

The order the owner set on 2026-10-31 for the six modules, this one holding the species, the
skeleton grown from a seed, the tree and grass meshes through the modeling, the texture sets and the
placement. The documents first, then the rest as the other projects have it, then the engines taking
the module. Items 1 and 2 are the game's session's, being free while the game waits on the engine's
package; item 3 is the session's that owns the code today; item 4 is each engine's session's. Among
the six this module comes fourth, after Levels and once BevyCSharp's item 83 is done, on Modeling.

1. **The documents, one commit.** AGENTS.md on the game's shape (one paragraph saying what the
    module is and that no engine is referenced, N 2.1; the documents table with STYLE, COMMITS,
    REVIEW, NORM and DEVLOG; building and testing; Where things are with a row a folder; the
    conventions, the commit form among them), README.md (what the module is, how an engine takes it
    as a submodule, `dotnet build` and `dotnet test`), `.github/STYLE.md`, `COMMITS.md` and
    `CODE_OF_CONDUCT.md` as the engines' with the engine's name replaced, `.gitignore` of the
    engines' shape with `/.github/dump/` in it, `.gitattributes`. The game's AGENTS.md and the
    engines' documents are the models; nothing is written that the module does not have yet. This
    commit is the first test of the commit form here, N 7.2 and N 7.6, read by the reviewing session
    before it is pushed. Done at `41b4925`.

2. **The skeleton, one commit after item 1.** `Vegetation.slnx`, `Vegetation/Vegetation.csproj` as a
    class library on net10.0 with `Nullable` and `ImplicitUsings` in `Directory.Build.props`,
    `Vegetation.Tests/` on xunit with one test that the library loads, `build/version.txt` at `0.1`
    and `build/version.sh` as the engines', and `.github/workflows/build.yml` on a push, `test (os)`
    on ubuntu-24.04, windows-latest and macos-15 with `dotnet build Vegetation.slnx -warnaserror`
    and `dotnet test Vegetation.Tests --no-build`, a time limit on every job with its reason,
    `permissions` read and the engines' concurrency group. Green on the three systems before item 3.
    Done at `63fc628`.

3. **The code's move.** The source is BevyCSharp's Vegetation, Trees, TreeSpecies, TreeSkeleton,
TreeTextureSet and the grass of its item 83, moved after 83's phases are done, each engine binding
the leaves' material, the wind, the impostors and the light through as its renderer has them. One
commit a part, the tests coming with the code, the strict build and the suite green, a Shared line
in the engines' REVIEW.md when a part lands. Items written by the reviewing session when item 2 is
green and the owning session is free.

4. **The engines take the module.** Each engine adds this repository as a git submodule under
`modules/` and references its project, its workflows checking submodules out, its own folder for
this code becoming the binding of the module's interfaces (N 2.1) and the rest deleted; a change
used by an engine is committed here first and pushed before the engine's commit that moves the
pointer. One commit an engine, after item 3's first part.

5. **The species and the skeleton.** Item 3's first part, by the BevyCSharp session. TreeSpecies
    with its presets, TreeLevel, TreeShape, TreeLeafShape, TreeNode, TreeSkeleton, TreeGrowth with
    Space, Pcg, the species check and the growth entry move to the namespace Vegetation; the growth
    is TreeSkeleton.Grow of a species, a seed and blockers; places and boxes are System.Numerics
    vectors and the bark, leaf and fall colors are linear sRGB vectors as Sky's Colorimetry has
    them, converted at each use in the engines; ten of the bridge's thirteen tree tests come with
    the code, the three that build meshes staying. One module commit, told to the reviewer first and
    pushed before the bridge's commit that takes the module as a submodule, as Sky is taken, with
    the types out of its public listing and its tree guide and cheatsheet following; a Shared line
    in the bridge's REVIEW.md. Done at `c2149be`, pushed at 22:36 on 2026-11-08; the growth is the
    bridge's to the bit, 26 skeletons giving the same 72,137 lines of points, parents and radii, ten
    tests moved and 16 running, Pcg and the species check public while the bridge's leaves, grass
    and planting draw from them.

6. **The texture sets.** Item 3's second part. TreeTextureKind, TreeTextureSet, TreeTextures and the
    Textures folder with its recipe, kept as Modeling keeps its collection (Modeling's Decision 8),
    the loading staying in the engines through the module's paths. A module commit pushed before the
    bridge's. After 5. Done at `f18a993`, pushed at 01:01 on 2026-11-09; the 13 sets and their
    manifest under Vegetation/Textures, embedded in the module's assembly, TreeTextures.Files
    listing every file by its path and Open opening one, an added set under added/<n>/, the recipe
    taking --sources since the sources stay in the bridge's .vegetation and rebuilding all 43 files
    byte for byte, MeanColor a linear sRGB vector, ten tests moved or new, the loading staying in
    the bridge as TreeTextureAssets.

7. **The grass's scatter.** Item 3's third part. Grass and GrassSettings, the scatter alone, the
    grass's material, wind and spawning staying in the engines. A module commit pushed before the
    bridge's. After 6. Done at `d604e4d`, pushed at 02:42 on 2026-11-09; GrassSettings whole,
    GrassScatter cutting a stretch into patches and scattering each patch's clumps by its seed in
    the bridge's order, the same to the bit as the bridge before the move over 400 patch meshes and
    161 places, four tests with it and 30 passing, the cards, the sway, the material, the wind and
    the entities staying in the engines.

8. **The planting's placement.** Item 3's fourth part. Where the trees stand in a level, the
    placement alone, the spawn, the colliders and the impostors staying in the engines. A module
    commit pushed before the bridge's. After 7. As proposed by the BevyCSharp session on 2026-11-09
    and taken, the part holds the variants' count and each variant's seed, a tree's stance from its
    seed, its variant, its turn and its size within a tenth either way, and the forest's spread over
    a disc, so a level's trees stand the same in both engines; the spawn, the colliders, the
    dressing and the impostors stay in the engines. Done at `2fba808`, pushed at 04:16 on
    2026-11-09; TreePlanting with its eight variants and their seeds, a tree's stance from its seed
    and a forest's places and seeds over its disc, drawn from Pcg in the bridge's order and summed
    as the bridge summed, the same to the bit over 35 forests' 2,093 trees and 48 dressed trees with
    their 228 colliders, four tests with it and 34 passing.

9. **The meshes.** Item 3's fifth part, blocked. Build, Cards, Leaves, Volume, TreeLooks and
    TreeDetail are built on the edit mesh, which the Modeling module does not hold until its item 3
    moves it, given to the 3DEngine session by Modeling's Decision 5; until then they stay in the
    bridge as its binding, with the wind, the materials, the impostors, the card pictures, the
    memory and the Tree component, which stay there for good. After Modeling's item 3.

## Verdicts

One verdict stands, the first, on the red run of the leaves' fixture.

1. **Run 44355818953 of 376cd98 was red on all three systems (2026-11-18).** One test failed on
    each, the leaves against the fixture of hashes, where it passed on the machine the fixture was
    made on. On the Linux runner the first case differs in its count of vertices and in every array,
    the indices among them, and on Windows the floating arrays differ. So a seed does not grow the
    same leaves on every machine, and the cause lies before the leaves on at least one runner, in
    how many cards a tree gets. The mend is to find where the first number parts, a count read from
    a floating result or a library function rounding differently by processor, and to state what the
    module promises for a seed, with a test that holds on three systems.

## Decisions

1. **The repository's birth.** The owner made this repository on 2026-10-31 at 12:03 among six,
Modeling, Levels, Landscape, RadianceCascades, Tools beside it, after the ideas recorded as
3DEngine's Decision 78 and BevyCSharp's Decision 71, to hold the species, the skeleton grown from a
seed, the tree and grass meshes through the modeling, the texture sets and the placement as one
library both engines use rather than one implementation an engine. The owner set the order the same
day, the documents first, then the rest as the other projects have it, then the engines taking the
module, and said the modules may be tried at once beside the engine's global illumination work
(3DEngine Decision 79).

2. **A git submodule, not a package.** The owner chose on 2026-10-31 that the engines take the
module as a git submodule built as a project of their own, since a package's upload, wait, test and
pack is slower to iterate than a commit. A change is committed here first; the reviewing session
pushes this repository before the engine's commit that moves its pointer; the workflows check
submodules out. The game keeps using the engine through the engine's package.

3. **The module knows no engine.** N 2.1. The module references no engine, package or bridge; what
it needs of a renderer, a world, a scheduler or a file system it takes through interfaces it
declares, which each engine implements where it binds the module. A type of an engine's in this
repository is a fault.

4. **The repository's terms are the owner's.** The repository was made by the owner with its terms
   file in place, which stands as the owner set it and is spoken of nowhere else.

5. **Who writes here.** The game's session writes items 1 and 2 for the six modules, being free
while the game waits on the engine's package; the session that owns the code today moves it (item
3), BevyCSharp's for Vegetation and Levels, 3DEngine's for Modeling and the cascades, the reviewing
session's scripts going into Tools; each engine's session binds the module (item 4). Every session
reads REVIEW.md's head, its items and the Replies before a batch, as N 7.7 says, and the reviewing
session pushes.

6. **The move in five parts.** On 2026-11-08 at 21:51 the review took the BevyCSharp session's split
    of item 3 as items 5 to 9, the species and skeleton first, then the texture sets, the grass's
    scatter and the planting's placement, the meshes last and blocked on Modeling's edit mesh; every
    part a module commit pushed before the engine's commit that moves its pointer, with its tests,
    the strict build, the suite and the drive. The module's DEVLOG.md, written with items 1 and 2
    and left untracked, goes in with item 5's commit and takes a Done line as each part lands, by
    hand, since the modules have no generator.

7. **The module is worked by a session of its own.** Decided at 16:43 on 2026-11-18 for the seven
    modules at once. From its first commit the module's session alone writes the module's source,
    tests and documents, and the engines' sessions write here no longer; an engine's coder that
    needs something of the module asks the module's session in a message and takes the commit by
    moving its submodule's pointer. Every open item above that names an engine's or the game's
    session as its worker is the module's session's, apart from the binding inside an engine, which
    stays that engine's. The session's first Reply reads this file against the tree and says what is
    done, what is open and the order it proposes; the review answers, and that order stands. The
    rules are NORM.md's as before, among them no engine referenced (N 2.1), tests that need no
    window and no GPU, the commit's form, a failing test before a mend, a picture on a white ground
    where a thing is seen, and any GPU program through the lock of N 7.9 with a waiting file. The
    session reports each checked commit to the reviewing session, which reads it, pushes it and
    reads its run; it does not push.

8. **The module is agnostic enough that a stranger binds it in an engine of their own from its
    README alone (2026-11-18 17:28).** It asks a README section, Binding it in your engine, with
    every interface the module declares, the conventions it stands on and the smallest binding that
    works, a small host in the tests that implements those interfaces with no engine and that the
    section's steps are run against, a test that fails where the library references an engine, a
    window or a graphics API, the form in the Tools module's ReferenceTests.cs, and no engine's name
    in a public member or its summary. The two engines appear in the README as examples. A public
    shape that fits the two engines alone is named in a Reply before it changes.

9. **The package id and the assembly are 3DEngine.Vegetation and the namespace stays Vegetation
    (2026-11-18 17:28).** The prefix gives a unique name. A package is published when the time is
    right and on the owner's word, for a reader who takes the module alone. The engines keep taking
    the module as a git submodule, compiled with them and carried inside each engine's one package.
    Folders and project file names stay.

## Replies

Item 5, the species and the skeleton, in the commit carrying this line. TreeSpecies with its
presets, TreeLevel, TreeShape, TreeLeafShape, TreeNode, TreeSkeleton, TreeGrowth with its space and
Pcg move from BevyCSharp, the namespace Vegetation. The growth's entry is TreeSkeleton.Grow of a
species, a seed and blockers, the bridge's Trees.Grow as it was, and the species' check is
TreeSpecies.Check, public, since the bridge keeps species by name and checks one as it is kept. Pcg
is public as well, since the bridge's leaves, grass and planting draw from it until their parts
move, the leaves' with the meshes once Modeling holds the edit mesh. Places and boxes are
System.Numerics' Vector3, and the bark's, the leaves' and the fall's colors are linear sRGB vectors
as Sky's Colorimetry has them, the presets written in sRGB as before and decoded by sRGB's own
curve. A way at unit length, which System.Numerics gives as NaN for a vector of no length, is an
extension of the module's that turns such a way along Z as the bridge's Vec3 did. The
documentation's references to the bridge's meshes, texture sets and leaf material are said in words.

The growth is the bridge's to the bit, the six presets at four seeds each and two trees grown round
boxes giving the same 72,137 lines of points, parents and radii as the bridge's head. Ten of the
bridge's thirteen tree tests come with the code, the refusal of a skeleton out of order without the
mesh it built, which stays in the bridge, and the refusal test checks the public check as well. The
strict build is clean and the tests pass, 16 where there was 1 (dotnet test Vegetation.Tests). The
workflow's comment gives the build's 8 seconds and the test's 19 from an empty package folder, where
it gave 3 and 2. The README's table names what the module holds in four rows. DEVLOG.md, written
with items 1 and 2, goes in with this commit, and its Done line for this item with the next.
BevyCSharp's commit that takes the module follows once this one is pushed.

Item 6, the texture sets, in the commit carrying this line. TreeTextureKind, TreeTextureSet and
TreeTextures move from BevyCSharp, the namespace Vegetation, with the thirteen sets and their
manifest under Vegetation/Textures and the recipe, build/textures.py, as Modeling keeps its
collection. The library embeds the sets and the manifest in its assembly, the sets marked waiting
apart, so TreeTextures lists and finds them with no engine, and an engine reads each file by its
path in the collection, which Files lists and Open opens, the set's name and the file's, as
silver-birch-bark/albedo.ktx2, a game's folder added with Add lying below added/ and its number. The
loading stays in the engines, the bridge reading every file under a folder of its asset root and
loading a set's images with their samplers. A set's mean color is linear sRGB in a Vector3 as a
species' colors are, decoded by the same curve, and its mean surface is public, since the bridge's
far cards are drawn with it.

The recipe takes --sources, the folder its sources are read under, .vegetation/ at the repository's
root where none is given, since the sources stay on the machine that imports them, beside the bridge
here, and the manifest names each source within that folder. Run from the module with the bridge's
sources, it makes all 43 files of the thirteen sets and the tests' sample the same to the byte as
the collection and the sample hold. Its docstring gives the formats in their own terms where it
named Bevy's material and loader. Ten tests come with the code, eight of the bridge's and two new,
one opening every carried file against its folder's bytes and one reading each set's mean color as
its manifest's through sRGB's curve, and the leaf boxes' test checks the sets' boxes against the
manifest as well. The tests that run the recipe stay in the bridge, where a test is skipped on a
machine without Python 3.14 and Pillow, as do those that load a set on the GPU. The strict build is
clean and the tests pass, 26 where there were 16. The README names the three types and gives the
collection's section, which BUILDING.md held in the bridge, and the .gitignore leaves out the
waiting sets and the sources. DEVLOG.md takes item 5's Done line. AGENTS.md's row for build names
the version alone, and naming textures.py there, as Modeling's row names its own, waits on the
owner's word (N 7.4).

Item 7, the grass's scatter, in the commit carrying this line. GrassSettings moves from BevyCSharp
whole, the namespace Vegetation, and the scatter that was the bridge's Grass.Plant and Grass.Patch
is GrassScatter. Its Patches cuts a stretch into the square patches its grass is drawn in, each a
GrassPatch with its column, its row, its middle, its size and its seed, and its Clumps scatters a
patch's clumps a cell each, each a GrassClump with its root, its turn, its height and the two phases
it sways by. A clump's numbers are drawn from Pcg in the order the bridge drew them, so the bridge's
patches and meshes are the same to the bit, 400 patches' meshes over four settings, five sizes, five
seeds and four widths of picture hashed whole and 161 patches' places over ten stretches the same as
the bridge's before the move, with its three refusals' arguments and messages. The two cards a clump
is drawn as, their width from the set's picture, the sway streams, the material, the wind and the
patches' entities stay in the engines, the bridge's Grass.Plant planting what the scatter gives. The
refusals are the bridge's, a stretch of no size as the size argument and a setting out of range or a
set that is not grass as the settings argument, and Clumps refuses what Patches does but the set,
which it does not read.

Four tests come with the code, the bridge's test of a seed scattering the same patch and three new,
the patches a stretch is cut into, a patch's clumps a cell each on the ground and the refusals,
which the bridge's tests checked through what it planted. The strict build is clean and the tests
pass, 30 where there were 26. The README names the four types in a row, and DEVLOG.md takes item 6's
Done line.

Item 8, the planting's placement, in the commit carrying this line. TreePlanting holds where trees
stand and how each stands, Variants, the eight variants a species grows, VariantSeed, the seed a
variant grows from apart from any a game gives, Stance, a tree's variant, turn and size of a tenth
either way from its seed as a TreeStance, and Forest, a forest's trees spread over a disc from a
seed by Poisson disk sampling by trial, each tree's place and its own seed. They were the bridge's
Trees.Variants, its variants' seeds, the turn and size it dressed a tree with and its forest's
places, and the numbers are drawn from Pcg in the order the bridge drew them, so a level's trees
stand the same in either engine. Planted through the bridge before and after the move, 35 forests'
2,093 trees stand at the same places with the same seeds to the bit, 48 dressed trees of two species
turn and size the same with their 228 colliders, and the five refusals of a forest keep their kinds,
arguments, messages and order. The spawn, the registry of species by name, the dressing, the
colliders and the impostors stay in the engines. A forest's distance between two trees is summed a
product at a time, as the bridge summed it, rather than by a library's dot product.

Four tests come with the code, the bridge's forest test with the seed's spread and three new, a
tree's stance from its seed, the variants' seeds and the forests refused, the bridge keeping a test
that plants a forest where the module spreads it. The strict build is clean and the tests pass, 34
where there were 30. The README names TreePlanting and TreeStance in a row and Pcg's row the grass
and the planting it draws for, and DEVLOG.md takes item 7's Done line.

Decision 7, the module's session's first Reply, the file read against the tree at `f3ad69c`, where
the strict build is clean and the 34 tests pass (`dotnet build Vegetation.slnx -warnaserror`,
`dotnet test Vegetation.Tests`). Items 1 and 2 are done, and items 5 to 8, four parts of item 3's
move, each at the commit the head gives. BevyCSharp's half of item 4 is done, its pointer at
`2fba808` beside Levels and Sky. 3DEngine takes Sky alone and holds no tree or grass code to bind,
so its half of item 4 is open and its own session's, since Decision 7 leaves a binding inside an
engine to that engine. DEVLOG.md lacks item 8's Done line, which goes in with the next commit as
each before it did, and its opening names `.github/REVIEW-ARCHIVE.md` and pictures under
`.github/assets`, neither of which the repository holds, so the same commit has it name what is
here. `f3ad69c` is on origin's main, where the head names nothing pushed after `2fba808`.

Less of item 9 is blocked than it says. The stems (the bridge's Trees.Build) and the far cards
(Trees.Cards) are built on the edit mesh, which Modeling does not hold, its library carrying its
textures alone. The leaves build the bridge's MeshData, a type of its Rendering folder, and take
their sway's pivots from the wind's limb plan, so they need vertex arrays of the module's own rather
than the edit mesh. TreeLooks, the bark's pictures and each leaf shape's picture drawn on the CPU
with the boxes the leaves read from them, and the crown's volume in Trees.Volume, the sky each leaf
sees and its way out of the crown, use neither, their one tie to the bridge its Vec3, and TreeDetail
is an enum of two levels.

The order proposed is four parts of item 9. TreeLooks first, one commit, its pictures the same to
the byte as the bridge's for the bark and for each leaf shape at every width the presets give, with
tests of its own since the bridge has none and a picture of the bark and the six leaf shapes on a
white ground, the devlog's first, the bridge's materials making their images of what the module
gives. The crown's volume second, one commit, the sky and the ways the same to the bit over the
presets' leaves, the bridge's leaves shading by it. The leaves third, proposed in a few lines once
the volume lands, their cards and fronds at both details with TreeDetail, given as the module's own
vertex arrays that the engine copies into its mesh, after reading where the sway's pivots part
from the wind, which stays in the engines. The stems and the far cards last, as item 9 has them,
once Modeling's session has moved the edit mesh.

Rule: Decision 7 sends any GPU program through the lock of N 7.9 with a waiting file, and NORM.md
holds no N 7.9, so the lock's terms are not in the module's norm.

Rule: NORM.md's Terms give the working session as one of the engines' or the game's, and Decision 5
gives the move to BevyCSharp's session, where Decision 7 gives both to the module's own.

Item 9's first part, TreeLooks, in the commit carrying this line. The bark's color, normal and
depth and the leaves' atlas a species draws with where it names no set move from BevyCSharp, public
in the namespace Vegetation with the atlas's boxes, the normal made with System.Numerics' Vector3
and the module's own unit length. Run through the bridge's built assembly beside the module's, the
three bark pictures and 222 atlases, the six shapes at 37 widths from 0.01 to 3, are the same to the
byte. Leaves refuses a width that is not a positive number as the width argument, where the bridge
took any, since the species' check holds LeafScaleX the same and NaN drew an atlas of stalks alone.
The bridge's own TreeLooks, in the namespace Bevy, goes before the module's in its files, so moving
its pointer breaks nothing, and deleting its copy is its binding's work.

Five tests come with the code, eight cases, none in the bridge before. The bark is gray, its depth
the gray turned over and its normal facing out, and its three pictures hash the same on every
system, since it is worked out by arithmetic alone; it tiles both ways; each shape's leaves stand
stalk down in the four boxes; the shapes differ and each widens with its width; and the widths are
refused. The leaves' outlines take sines, powers and arc tangents, which may round apart between
systems' libraries, so they are held by their form rather than a hash. The strict build is clean and
the tests pass, 43 where there were 34, the width test below among them. The README names TreeLooks
in a row, and the species' and the collection's documentation names it where it said an engine
generated the bark and the leaves. The picture, `2026-11-18-item-9-looks.webp`, shows the bark and
the six shapes on a white ground, and in it the two maples' blades part in two below their lobes and
stand clear of their stalks, as the bridge drew them; a mend of their outlines changes what is seen
and is proposed apart if the review asks for it.

The same commit holds N 4.11. CommentWidthTests reads every C# comment line, the scripts' and the
workflow's comments and the projects' MSBuild comments, a line holding an address alone apart, and
fails on one past 100 columns, naming its path, its line and its width; a line of 103 columns
planted in Pcg.cs failed it so before it was taken out. The 157 C# comment lines past the width and
four of the workflow's and the library project's are rewrapped with their words unchanged, a
summary, a parameter, a return or a remark too long for one line opening onto lines of its own.
The build script's docstrings, some 20 lines past 100 with two of its usage's commands among them,
are not comments to the test, and take a rewrap and a test of their own if N 4.11 is read to hold
them. AGENTS.md's DEVLOG.md row carries its link, and the picture a title line.

Item 9's mend of the leaf outlines, in the commit carrying this line, after TreeLooks. A test that
each shape's leaf is one piece, its middle column opaque with no gap from the stalk's foot to the
tip, at six widths from 0.1 to 1, failed first on the oval at 0.1, a texel clear between its stalk
and its blade, and on the maples, their blades parted below their lobes over a gap down to the
stalk. The stalk runs on a tenth into the blade's foot, which mends the narrow oval and oak. Each
maple's lobes are a spread apart, 58 degrees for three and 45 for five, each narrowing straight to
its point from sinuses at 0.45 and 0.5 of the reach and shorter the further it leans from the
midrib, over a small base round the blade's point with the stalk running up to it, and a vein runs
from the point to each lobe's tip, which keeps a narrow maple's thin tips joined. The three leaflets
are each no thinner than their midrib, so at 0.1 the side ones meet the stalk where they were five
pieces. Through the bridge's assembly beside the module's, the bark and the triangle are the same
to the byte, and 129 of the 222 atlases change, the maples and the leaflets at every width and the
oval and the oak at the nine narrowest. The sheet, `2026-11-18-item-9-leaf-outlines.webp`, puts
each beside the bridge's on a white ground. The bridge takes the mend when it draws from the
module's TreeLooks in place of its own copy.

N 4.11 holds the scripts' docstrings in the same commit. CommentWidthTests reads a Python line
inside or on the quotes of a docstring, and a docstring line of 102 columns planted in
build/textures.py failed it, as did two of the mend's comments, before both were mended. The
script's docstring paragraphs past the width are rewrapped with their words unchanged, and its two
usage lines past it shortened by their comments. The strict build is clean and the tests pass, 44
where there were 43.

Item 9's second part, the crown's volume, in the commit carrying this line. The bridge's CrownVolume
moves as TreeCrown.Shade, public in the namespace Vegetation, which takes each leaf's middle and its
card's area and gives the share of the sky the leaf sees through the crown and the way out of the
crown at it, places in meters with Y up as the skeleton's are. A length is summed a product at a
time, as the bridge's Vec3 summed it, rather than by System.Numerics' dot product. Through the
bridge's assembly beside the module's, the six presets at four seeds each, the 21 crowns whose
leaves are cards of four corners, and 18 clouds of one to 3,000 leaves at spreads from a centimeter
to ten meters, two of the leaves at one place among them, give 500,899 leaves the same skies and
ways to the bit, but for one mend. A test that every leaf's sky is from 0 to 1 failed first on 33 of
a ball's 2,109 leaves at 1.00000012, the nine ways' weights summing a float's last digit past one,
and the sky is held at one, which moves 706 of the 500,899 leaves by that digit and nothing seen.
Shade refuses a place that is not finite as the places argument, and areas not as many as the
places, or one below zero or not finite, as the areas argument, where the bridge read past the end
or drew a grid of NaN.

Four tests come with the code, over a ball of 2,109 leaves a quarter meter apart. The leaf at the
middle sees less sky than any on the upper half of the rim and the top more than the bottom; the way
out is at unit length and on the rim within 46 degrees of the way from the middle; the shade is the
same each time and no leaves give no shade; and the refusals. A lone leaf looks out along a side of
its voxel, rounding having put it at a voxel's edge, as the bridge's did, which no crown of a tree
meets, so no test holds it. The strict build is clean and the tests pass, 48 where there were 44.
The README names TreeCrown in a row. No picture comes with it, since the move changes nothing seen;
the bridge's leaves take it when they shade by the module's TreeCrown.

The names, in the commit carrying this line, before the binding. The library's project builds the
assembly 3DEngine.Vegetation with the package id 3DEngine.Vegetation, still not packed, and the
tests' the assembly 3DEngine.Vegetation.Tests; the folders, the project files and the namespace
Vegetation stay. The test that the library loads loads it by its new name, the README says the name
the project builds, and the tests pass, 48 as before.

The binding's guide, in the commit carrying this line, the first of the owner's four. The README's
section Binding it in your engine says the module declares no interface, since it reads its own
assembly and the folders a game hands TreeTextures.Add and nothing else, and each type gives data an
engine makes its own things of; it gives the conventions in a table, meters, Y up and right-handed,
a tree's turn counterclockwise from above as System.Numerics' quaternion turns it and a clump's card
along X cos turn and Z sin turn, a species' angles in degrees, colors in linear sRGB, the pictures'
rows and encodings, normal maps with Y toward the top row as OpenGL and glTF read them, depth with
255 the deepest, the texture files' formats, no unit of light, and Pcg's seeds; and the smallest
binding, a forest of a species with its bark and leaves and a field of grass, against a host that
draws a stem, a texture file, a picture and a clump. TreeStance's and GrassClump's turns say their
direction where they said only their axis. Three public shapes fit what the two engines read rather
than what any engine does, and each stands as a convention the table says rather than a change. They
are the depth map, deepest at 255 where an engine of heights turns it over; the normal maps' Y up
the picture, which an engine of DirectX's convention flips; and the texture files, KTX2 with zstd
over BC7, BC5 and BC4, which an engine without a KTX2 reader of zstd cannot load, 3DEngine's among
them as Materials found, left for Materials' Reply.

The binding's host, in the commit carrying this line, the second. BindingTests runs the README's
binding against a host in the tests that records each stem, texture file, picture and clump an
engine would draw, for the quaking aspen with its sets, 200 trees whose stems stand on the ground
within the forest's disc, six KTX2 files opened by their marks and the grass on the ground within
its field, and for the same species with no sets, which takes the bark's and a five-lobed maple's
pictures at their sizes. A third test holds the README's code block the same as the body of the
host's Bind line for line, and a drift planted in the README's block failed it before it was taken
out. The README says so under its block. The lone leaf's rounding has its test in the same commit,
the leaf at the origin looking straight up and at 3, 5, -2 and at 120 meters from the origin leaning
toward its grid's low corner, at unit length with a sky from 0 to 1. The strict build is clean and
the tests pass, 54 where there were 48.

The reference check, in the commit carrying this line, the third. ReferenceTests is Tools' file at
its head copied whole, its namespace and its library's name set to the module's and one remark
rewrapped to 100 columns. It reads the library's assembly references and the packages the tests'
dependency file records for it and refuses an engine, a window or a graphics API by a part of its
name. SkiaSharp 2.88.9 planted as a package of the library's project failed it before it was taken
out. The strict build is clean and the tests pass, 66 where there were 54.

The names, in the commit carrying this line, the fourth. NamesTests reads the library's exported
types and their members and every member of the documentation the build writes, and fails on
3DEngine, BevyCSharp, Bevy or the bridge as a word, each found named by its member; a summary
planted with the bridge's name failed it before it was taken out. Nothing in the library named an
engine, the assembly's name apart, which is the owner's and is no reference, and the README names
the engines as two bindings in its section on binding and as the two that take the module in its
opening. The strict build is clean and the tests pass, 67 where there were 66.

Item 9's third part proposed, the leaves. TreeDetail moves as it is, and TreeLeaves.Build of a
skeleton, a species, a seed and a detail gives a TreeLeafMesh of the module's own arrays, each
vertex's place, normal, texture coordinates, the sky it sees from TreeCrown and its way out of the
crown, with its limb's pivot and phase and its own pivot and phase for the sway, the triangles'
indices, and each card's twig, which a far card is drawn from. The bridge's cards, fronds, their
spread and bend, the reduced level's every other card drawn larger and the subset a far card takes
move with it, numbers drawn from Pcg in the bridge's order. The limb plan moves with them as
TreeLimbs, which limb each node sways with and where it pivots, since the stems' sway reads it too
and it is the skeleton's alone; the stiffness, the wind's direction, the streams' formats and
places, the leaf material, the shade's floor of a half and the folding of the way out into texture
coordinates stay in the engines, each copying the arrays into its mesh. The same to the bit as the
bridge over the presets at four seeds and both details, compared as the volume was, with the
bridge's eight leaf tests that need no GPU moved and the rest staying. One commit, the stems and the
far cards after it once Modeling holds the edit mesh.

The texture files' zstd, which 3DEngine's KTX2 reader and Materials' first part refuse, is better
dropped on the module's side. The 43 files are 4.96 MB with zstd over each level and 6.99 MB as
plain KTX2, summed from their own level indexes, 2.03 MB more in the assembly; the largest species'
bark and leaves stand at 1.10 MB against the 3 MB a species is held to and the collection at 6.99 MB
against 32 MB, and a plain file's levels are the BC blocks an engine uploads as they lie, so a
stranger's loader reads them with no decompressor. A reader gaining zstd keeps the 2 MB and asks
every engine that binds the module for a zstd decoder. The recipe writes plain KTX2 when Materials,
which owns the pictures' encoding, agrees.

Item 9's third part, the leaves, in the commit carrying this line, as proposed and answered.
TreeDetail moves as it was, TreeLeaves.Build of a skeleton, a species, a seed and a level gives a
TreeLeafMesh of the module's arrays, each vertex's place, normal, texture coordinates, sky, way out,
limb pivot and own pivot, the triangles and each card's twig, and TreeLimbs is the bridge's limb
plan, its sixteen thickest stems, which limb each stem swings with, its pivot and Knuth's phase. The
bridge's cards, fronds, spread, bend, reduced level and sway move with them, the numbers drawn from
Pcg in the bridge's order and each product and sum written out as its Vec3 wrote it; the stiffness,
the wind, the streams, the leaf material, the shade's floor of a half and the folding of the way out
into texture coordinates stay in the engines. A reduced level's twigs are every other card's of the
full level, which the bridge's reduced mesh did not carry. The README names the four types and gives
each per-vertex field its units and range in the conventions table.

The check is a fixture, Vegetation.Tests/references/leaves.json, of SHA-256 hashes of each array's
bytes for the six presets at seeds 1 to 4 and both levels, 48 cases, written by build/leaves-fixture
from the bridge's build at 8e7fa0f5 by reflection, its private card builder, limb plan and crown
volume, with the sky held at one as the module holds it. The library and its tests reference no
engine; the script loads the bridge's assembly from the folder named on its command line and is no
project of the solution. It is remade with `dotnet run --project build/leaves-fixture -- BIN
COMMIT`, BIN the folder of the bridge's build holding BevyCSharp.Vegetation.dll and COMMIT the
bridge's commit, which the fixture names. All 48 cases' nine arrays hash as the fixture, and one
more draw from Pcg planted after a twig's first turn failed it from the aspen's first case on, its
places, normals, texture coordinates, sky and way out apart, before it was taken out. The hashes are
of sines, cosines and powers, which the systems' libraries may round apart, so the first run on
Windows and macOS reads whether a seed grows the same leaves on every system; where it does not, the
review decides between a fixture a system and a reading by tolerance.

Ten tests come with the code, the bridge's eight leaf tests that need no GPU, the shade read as the
sky itself, its floor being the engine's, the fixture, and one of the limbs and the leaves' pivots,
the bridge keeping its tests that draw. The strict build is clean and the tests pass, 77 where there
were 67.

Verdict 1, the leaves' fixture red on the three systems, read in the commit carrying this line. The
runner's log parts it in two. For the aspen's seeds 2 and 3 on Linux the cards' count, texture
coordinates, limb pivots, indices and twigs match, the limb pivots being the skeleton's own points,
and the places, normals, sky, ways out and own pivots differ, so those skeletons are the same and
the leaves part where they are placed, by MathF.Pow, Sin and Cos, which glibc picks by processor
among variants with fused multiply-add and without. For seed 1 the count differs, so its skeleton
parts already, where the growth takes the same functions, Acos besides, and System.Numerics' Dot,
Cross and LengthSquared, whose instructions the JIT picks by processor. ProbeTests hashes each of
those over a fixed sweep of arguments, each preset's skeleton at two seeds and its leaves' count and
texture coordinates, against references/probes.json, written on this machine, an Intel i9-14900HX
with AVX2 and FMA, by running the test with VEGETATION_WRITE_PROBES=1, so the next run names on each
system the first function and the first stage whose bits part.

Of the two promises, the same counts with the floats within a tolerance cannot be kept apart from
the first, since the growth decides how many nodes it buds by comparing distances and light from its
own floats a step at a time, so a last bit apart buds a twig more, as seed 1 shows; the same counts
everywhere need the growth's arithmetic the same everywhere. The same tree to the bit everywhere
costs the module its own sine, cosine, power, exponential, arc cosine and arc tangent, written in
single precision's sums and products alone, which IEEE 754 rounds alike on every processor and the
JIT does not fuse, and every dot, cross and length of the growth written out a term at a time as the
leaves' are, some 150 lines with tests that hold each function within two units in the last place of
the runtime's own. It gives up the bridge's trees to the bit, the fixture being written again from
the module, and both engines then draw the module's trees alike. Proposed, the first, once the
probes' run has named what parts.

Verdict 1's mend, in the commit carrying this line. The probes' run named the runtime's functions
alone. On the Ubuntu runner its arc cosine and arc tangent part from this machine's, on macOS and
Windows its sine, cosine, power and exponential as well, and System.Numerics' dot, cross, length,
normalization and turn by a quaternion part nowhere, so the growth's count of nodes and the leaves'
places followed from the functions, the aspen at seed 1 budding 90,000 leaves' vertices where here
89,996. The module promises that a seed grows the same tree to the bit on every system.

Exact holds the module's own sine, cosine, power, exponential, arc cosine and arc tangent, worked in
doubles by sums, products and quotients alone and each result rounded once to a float. Sine and
cosine reduce an angle by π/2 in three parts of 33 bits below 1.6e6 and by Payne and Hanek's product
with 2/π to 448 bits above, the bits worked out from Machin's formula for π in integers and the same
as fdlibm's table, and the power, the exponential and the arc tangent keep IEEE 754's special cases.
The growth, the leaves, the crown, the planting, the pictures and the species' colors take them, and
every dot, cross and squared length of the growth is written out a term at a time in Vectors, as the
leaves' and the crown's are. ExactTests holds each function within two units in the last place of
the runtime's over a sweep of 20,000 arguments and at its edges, zero and minus zero, the largest
floats and angles to 3.4e38 for sine and cosine, the power's sixteen special cases and the arc
tangent's zeros and infinities, hashes each over the sweep so every system must give the same bits,
and holds that the build rounds a product and a sum apart, (1 + 2⁻¹²)² less 1 + 2⁻¹¹ being zero in
floats and (1 + 2⁻²⁷)² less 1 + 2⁻²⁶ in doubles where fused each is the last bit, in a vector's
lanes too. Sine of minus zero gave plus zero before the test held it, and keeps its sign.

The trees part from the bridge's by little. Every preset at seeds 1 to 4 keeps its count of nodes
and its height, and one of the 24 trees its leaves' count apart, the aspen at seed 4 at 89,996
vertices where 90,000. Of the aspen at seed 1, 177 of 1,128 nodes move, by a micrometer at most, and
18,330 of its 89,996 leaves' corners move, by 4.4 micrometers on the mean and one card by 9.8 cm,
the only one of 22,499 past a millimeter; of the oak at seed 1, 1,261 of 6,127 nodes move by 0.8
micrometers at most and its leaves' corners by 13.7 cm at most. The picture,
`2026-11-18-item-9-aspen-exact.webp`, sets the aspen before and after side by side on white with the
card that moved in red. The fixture is the module's, its skeleton's hash with each case's nodes,
written by VEGETATION_WRITE_LEAVES=1 on the test, and the bridge's leaf reader goes with the probes,
since the trees are the module's own. The probes' test goes as well, its functions' hashes being the
runtime's, which part by system as the run read. The strict build is clean and the tests pass, 84
where there were 78 with the probe. DEVLOG.md is written by Tools' scripts/devlog.py, four days,
three pictures and six items done, the README links it at its top and AGENTS.md names its command.

Plain KTX2, in the commit carrying this line. build/textures.py writes every level's blocks as they
are, supercompression scheme 0, each level starting at a multiple of its block's size, 16 bytes for
BC7 and BC5 and 8 for BC4, as KTX2 asks of levels a reader uploads in place, and no longer imports
zstd. The 13 sets were made again from their sources and the tests' sample with it, and each of the
47 files' levels is the zstd file's level decompressed to the byte, at an aligned offset, the
manifest changing in the files' sizes alone. The collection's 43 files are 6,987,376 bytes where
they were 4,956,158 under zstd, 2,031,218 more in the assembly, the largest species' bark and leaves
1.10 MB against the 3 MB a species is held to and the collection 6.99 MB against 32 MB.
EveryCarriedFileIsPlainKtx2 reads each carried file's header and levels and failed on the zstd
collection, at broadleaf-bark/albedo.ktx2 by scheme 2, before the plain one went in, and the
sample's headers are held at scheme 0. The README's conventions row, its collection section, the
set's remarks and the recipe's docstring say no supercompression.

Of the bridge, nothing reads a file's size or hash. TreeTextureAssets mounts the files by their
paths and opens each through TreeTextures.Open, its test comparing what Open gives with what the
mount serves, and its two recipe tests compare what the recipe writes with the collection and the
sample, which change together; its skip check imports zstd where the recipe needs it no longer,
which asks more than it must and stops nothing. The bridge's own KTX2 reader is to read scheme 0,
which its pointer's commit shows.

build/textures.py gives way to Materials' scripts/textures.py, which builds the bark, leaf and grass
sets from a manifest it is named, writes plain levels and carries the BC7 start's mend of 7e69a59,
in a commit of its own after this one, which makes the collection again by it from the sources with
--sources, its bytes changing by that mend alone, read level by level, and gives build/textures.py,
its docstring and the README's section to the command that runs Materials' script. Until then the
start's fault, a block whose red and blue move against each other encoded flat, is this recipe's,
the tests' sample at 23.7 dB among what it makes. The strict build is clean and the tests pass, 85
where there were 84.

The bytecode, in the commit carrying this line. The build/__pycache__ another session's load of the
recipe left in the tree is removed, and .gitignore leaves out every __pycache__ folder, so a script
run from a checkout with no -B writes nothing git reads.

The mean color, in the commit carrying this line. A set's MeanColor is said to be the linear mean of
its albedo's opaque texels and was the sRGB curve's decoding of their mean in sRGB, which the
curve's bow sets below the linear mean, 0.045 in linear light for grass-tuft-a and 0.0015 for the
willow's bark at the least. ASetsMeanColorIsTheLinearMeanOfItsAlbedo decodes each carried albedo's
top level from its BC7 blocks, the mode 6 the recipe writes, takes the linear mean of the texels
past half opacity and holds MeanColor to it within 0.001 a channel for a bark or a leaf set and
0.004 for a grass card, the gaps read after the mend being 0.0006 at most for the first and 0.0032
for the second, whose thin blades put many texels at the threshold where the file and its source
part. It failed on all thirteen sets before the mend. The recipe's mean_color takes the mean in
linear light and writes it in sRGB as the library decodes it, and the sets made again from their
sources change the manifest's colors alone, every file and the sample the same bytes. The library is
right and stays as it is. Materials' scripts/textures.py takes the mean as this recipe did, so its
mean_color takes the same mend before the collection is made with it. The strict build is clean and
the tests pass, 86 where there were 85.

The sets drawn, in the commit carrying this line, as the owner asked. The picture,
`2026-11-18-item-9-presets-sets.webp`, draws each of the six presets at seed 1 with its own sets,
decoded from the carried KTX2 files' BC7 by the sheet's script, the leaves sampled by the leaf
mesh's own texture coordinates into their atlas and cut by its alpha, beside the same tree in its
sets' mean colors, from the side on white with a title and a label a panel. Every preset's leaves
take their set, the atlas's boxes being the mesh's, and each tree in its mean colors stands as
bright as with its sets, which the mean color's mend asked. The bark cannot take its set by a mesh's
coordinates, since the module holds no stem mesh, the stems being built on the edit mesh Modeling
does not hold yet, so the sheet maps it round each stem by a stand-in, a turn of the picture to a
stem's girth, and the stems' own coordinates come with them. The fern names no bark set and is drawn
in its bark color, where an engine draws TreeLooks' bark. The weeping willow's crown stands round
rather than weeping, which is its growth's and no set's. No engine is used.

The shared checks, the cheatsheet and the names, in the commit carrying this line. Tools is a
submodule under modules/, pinned at 75d77f2, and the tests' project references its library with the
NeedsTools target that stops a clone without it on the command that fetches it; the library takes
nothing of it, and the workflow checks it out with submodules true. ModuleTests holds the seven
facts, each as Assert.True(fault is null, fault), that the library takes no engine, window or
graphics API, that no public name or its documentation names an engine, that no tracked text does,
that the README names every public type, that the README's binding block stands word for word
between the host's markers in BindingTests, that CHEATSHEET.md is what the built library writes, and
that the README opens with its link row to the devlog and the cheatsheet above its title. They
replace ReferenceTests, NamesTests and the binding test that held the README's block to Bind's body,
and the width test leaves modules/ to Tools. All seven passed on their first run.

CHEATSHEET.md, written with CHEATSHEET_WRITE=1, lists 22 types and 159 members, each with the first
sentence of its summary, under a short opening written by hand, five members, the implicit
constructors of TreeSpecies, TreeLevel and GrassSettings and the hash codes of a species and a
skeleton, whose inheritdoc named object's, take summaries of their own in the source, and a
positional record's constructor stands with its type's. The README's first line links the devlog and
the cheatsheet above its title, and AGENTS.md names the cheatsheet in its documents, the modules
folder in its areas and the devlog's command by modules/Tools.

The names. Of the lines the search for an engine's name finds outside modules/, the review and the
norm, 16 stood before this commit and none after. The three captions say what the earlier form was,
the runtime's functions or the outline before the mend, and the aspen's sheet names its panels so,
redrawn; the README's and AGENTS.md's openings say an engine takes the module where they named two,
and say it declares no interface where they said each engine implements those it declares, which it
never held; the README's binding section names no engine; ReferenceTests and NamesTests, which named
the engines to refuse them, go with the shared checks; and DEVLOG.md, written again, takes the
captions as they stand, four days, four pictures and six items done. What the engines do with the
module goes in the reports to the review alone. The strict build is clean and the tests pass, 79
where there were 86, the fourteen of the local copies giving way to the seven facts.
