# Vegetation cheatsheet

Every public type of the library in the order the README first names it, with its public members a
line each, the declaration and the first sentence of its summary. The README says what each type is
for and how an engine binds the module; this page is for finding a member's name and shape. Places
are in meters with Y up, colors in linear sRGB, and a seed gives the same result on every system.

<!-- written by Cheatsheet.Write from the built library -->
```csharp
record TreeSpecies                                       // What a kind of tree is, by Weber and Penn's controls of 1995, from which Grow grows a tree of that kind for each seed
TreeSpecies();                                           // A species with every control at its value unless set, a conical tree of 10 meters, a trunk with two levels of branches and no leaves, which a preset or a game's own species sets from
TreeShape Shape { get; init; }                           // The outline of the crown, Conical unless set
float Scale { get; init; }                               // How tall the tree is in meters, before the trunk's Length, 10 unless set
float ScaleVariation { get; init; }                      // How far in meters Scale varies either way from one tree to the next, up to the scale itself, 0 unless set
float BaseSize { get; init; }                            // The share of the scale below the crown, a bare trunk that carries no branches, 0.3 unless set
int BaseSplits { get; init; }                            // How many more trunks the trunk splits into at the crown's foot, 0 unless set, spread evenly round it and leaning out by the trunk's SplitAngle, as an oak's or a willow's trunk parts low down
float Ratio { get; init; }                               // The radius of the trunk at the ground against its height, 0.015 unless set
float Flare { get; init; }                               // How much wider the trunk is at the ground, over its lowest eighth, 0 unless set, so at 1 its foot is twice as wide
float AttractionUp { get; init; }                        // How strongly the branches turn upward as they grow, below zero turning them downward, 0 unless set
float PruneRatio { get; init; }                          // How far branches are held to the pruning envelope, from 0, not at all, to 1, a branch stopping where it leaves it, 0 unless set
string? BarkSet { get; init; }                           // The name of the bark set of the texture collection the tree is drawn with, or nothing for the bark Bark makes, nothing unless set
string? LeafSet { get; init; }                           // The name of the leaf set of the texture collection the tree's leaves are cut from, or nothing for the leaves Leaves makes, nothing unless set
Vector3 BarkColor { get; init; }                         // The bark's color in linear sRGB where the bark is the one Bark makes, a gray brown unless set
int Leaves { get; init; }                                // How many leaves each stem of the last level carries, as the paper's leaves, 0 unless set, so a species without them is bare
TreeLeafShape LeafShape { get; init; }                   // The outline of the leaves Leaves makes where no leaf set is named, Oval unless set
float LeafScale { get; init; }                           // How long a leaf is in meters, or how wide a frond is, 0.17 unless set
float LeafScaleX { get; init; }                          // How wide a generated leaf is against its length, 1 unless set
TreeLevel LeafLevel { get; init; }                       // The angles the leaves leave their twig at, the down angle from the twig and the turn round it from one leaf to the next, as the paper gives its leaves the angles of the level past the last stems
float LeafBend { get; init; }                            // How far each leaf turns to face up and outward from the tree, from 0 to 1, 0.5 unless set, as the paper's leaves turn toward the light
Vector3 LeafColor { get; init; }                         // The leaves' color in summer in linear sRGB, which tints the generated leaves, a mid green unless set
Vector3 FallColor { get; init; }                         // The leaves' color in the fall in linear sRGB, toward which an engine's leaf material turns them, a yellow brown unless set
float PruneWidth { get; init; }                          // The pruning envelope's widest radius as a share of the scale, 0.5 unless set
float PruneWidthPeak { get; init; }                      // How far up the crown the pruning envelope is widest, from above 0, its foot, to 1, its top, 0.5 unless set
float PrunePowerLow { get; init; }                       // The curve of the pruning envelope below its peak, 0.5 unless set, under one rounding it out and over one in, a value near zero keeping it as wide down to the crown's foot
float PrunePowerHigh { get; init; }                      // The curve of the pruning envelope above its peak, 0.5 unless set, as PrunePowerLow is below it
IReadOnlyList<TreeLevel> Levels { get; init; }           // The trunk and the levels of branches beneath it, from one, a trunk alone, to four, a trunk with three levels of branches
TreeSpecies QuakingAspen { get; }                        // The quaking aspen of Weber and Penn's appendix, a tall slender tree of 13 meters, three either way, with a narrow crown over a bare trunk
TreeSpecies BlackTupelo { get; }                         // The black tupelo of Weber and Penn's appendix, 23 meters, five either way, a straight trunk to the top with level branches all the way up
TreeSpecies WeepingWillow { get; }                       // The weeping willow of Weber and Penn's appendix, 12 meters, four either way, its trunk parting low into three and its branches arching out and hanging in curtains to near the ground, held to the paper's pruning envelope
TreeSpecies CaliforniaBlackOak { get; }                  // The California black oak of Weber and Penn's appendix, 10 meters, ten either way, its trunk parting low and forking on into a broad rounded crown of crooked limbs
TreeSpecies Bush { get; }                                // A round shrub of a meter and a half, its stems parting at the ground and branching into a dense ball
TreeSpecies Fern { get; }                                // A fern, a short stock with fronds of a meter arching out from it all round, each drawn as a frond along its stem
void Check();                                            // Throws where a control of the species is outside what it may be, which the message names
bool Equals(TreeSpecies? other);                         // Whether another species has the same controls, its levels compared one by one
int GetHashCode();                                       // A hash of the species' outline, size and count of levels, the same for equal species

record TreeLevel                                         // One level of a species' stems, its trunk or a level of its branches, with Weber and Penn's controls of 1995 for the stems of that level
TreeLevel();                                             // A level with every control at its value unless set, as a species sets its own
float DownAngle { get; init; }                           // The angle in degrees between a stem of this level and the stem it leaves, as it leaves it, 45 unless set
float DownAngleVariation { get; init; }                  // How far in degrees DownAngle varies either way at random, 0 unless set
float Rotate { get; init; }                              // The angle in degrees round the stem they leave from one stem of this level to the next, 140 unless set, so they leave it on every side in a spiral
float RotateVariation { get; init; }                     // How far in degrees Rotate varies either way at random, 0 unless set
float Curve { get; init; }                               // How many degrees a stem of this level bends over its whole length, or over its first half where CurveBack is set, downward where above zero and upward below it, 0 unless set
float CurveBack { get; init; }                           // How many degrees a stem of this level bends over its second half, where it is not zero, so a stem bent one way and then the other is an S, 0 unless set
float CurveVariation { get; init; }                      // How many degrees a stem of this level bends at random over its length, either way, 0 unless set
float Splits { get; init; }                              // How many times a stem of this level forks along its length on average, 0 unless set
float SplitAngle { get; init; }                          // The angle in degrees between the two stems of a fork, and for the trunk's base splits the angle each leans out by, 0 unless set
float SplitAngleVariation { get; init; }                 // How far in degrees SplitAngle varies either way at random, 0 unless set
int Branches { get; init; }                              // How many stems of this level each stem of the level before carries at most, 0 unless set
float Length { get; init; }                              // How long a stem of this level grows at most, against the stem it leaves, 1 unless set
float LengthVariation { get; init; }                     // How far Length varies either way at random, 0 unless set
float Taper { get; init; }                               // How much a stem of this level narrows along its length, from 0, as thick at its tip as at its foot, to 1, thinning as its branches leave it, 1 unless set

enum TreeShape                                           // The outline of a tree's crown, the eight of Weber and Penn's paper of 1995 and its pruning envelope, which set how long a tree's first branches are by how high up the trunk they leave it
Conical = 0,                                             // Widest at its foot and narrowing to a fifth of that at the top, as a spruce's
Spherical = 1,                                           // Widest halfway up, rounding to a fifth of that at its foot and its top
Hemispherical = 2,                                       // Widest at its foot, rounding over to a fifth of that at the top
Cylindrical = 3,                                         // As wide all the way up
TaperedCylindrical = 4,                                  // Wide at its foot, narrowing to half that at the top
Flame = 5,                                               // Widest three tenths of the way up, narrowing to nothing at its foot and its top
InverseConical = 6,                                      // A fifth of its width at its foot, widening to the top
TendFlame = 7,                                           // A Flame that keeps half its width at both ends, as a poplar's or an aspen's
Envelope = 8,                                            // The pruning envelope the species' PruneWidth, peak and powers draw, the paper's custom shape, as a weeping willow's

enum TreeLeafShape                                       // The outline of a leaf Leaves draws where a species names no leaf set, the paper's leaf shapes, its length along its midrib from the stalk to the tip
Oval = 0,                                                // Widest a little below its middle, rounding to a point, as an aspen's or a birch's
Triangle = 1,                                            // Widest at its foot and narrowing straight to its tip
ThreeLobeOak = 2,                                        // Three rounded lobes down each side and one at the tip, as an oak's
ThreeLobeMaple = 3,                                      // Three pointed lobes spreading from the top of the stalk over a small base, a vein to each, as a red maple's
FiveLobeMaple = 4,                                       // Five pointed lobes spreading from the top of the stalk over a small base, a vein to each, as a sugar maple's
ThreeLeaflets = 5,                                       // Three small leaflets on one stalk, the middle one the longest

class TreeLooks                                          // The bark's pictures and the leaves' atlas made in code, which an engine draws a tree with where its species names no set of the collection
int BarkWidth = 128;                                     // The bark's pictures' width in texels, round the stem
int BarkHeight = 512;                                    // The bark's pictures' height in texels, along the stem
int LeafSize = 256;                                      // The leaves' atlas's width and height in texels, two leaves by two
IReadOnlyList<(float U, float V, float Width, float Height)> LeafBoxes { get; }  // Where each of the atlas's four leaves is, a square cell each, as the share of the atlas's width and height from its left and top and the share it spans, in the form LeafBoxes gives a set's, each leaf's stalk at the bottom
(byte[] Color, byte[] Normal, byte[] Depth) Bark();      // Makes the bark's color, its normal map and its depth, each tiling both ways
byte[] Leaves(TreeLeafShape shape, float width);         // Makes the leaves' atlas, four leaves of a shape, each a little wider than the last

record TreeSkeleton                                      // A grown tree's stems as points, each with the point it grew from, its place and its radius, which Grow grows from a species and a seed and an engine builds into a mesh
TreeSkeleton(IReadOnlyList<TreeNode> nodes);             // A skeleton of the points given
IReadOnlyList<TreeNode> Nodes { get; }                   // The points, the trunk's foot first and each after the point it grew from
(Vector3 Low, Vector3 High) Bounds { get; }              // The smallest box holding every point, not counting the stems' thickness
TreeSkeleton Grow(TreeSpecies species, uint seed);       // Grows a tree of a species from a seed
TreeSkeleton Grow(TreeSpecies species, uint seed, IReadOnlyList<(Vector3 Low, Vector3 High)> blockers);  // Grows a tree of a species from a seed, round boxes it does not grow into and that shade it
bool Equals(TreeSkeleton? other);                        // Whether another skeleton has the same points in the same order, compared one by one
int GetHashCode();                                       // A hash of the count of points and the first and last of them, the same for equal skeletons

record struct TreeNode                                   // A point of a tree's skeleton, where a stem passes, how thick it is there and which point it grew from
TreeNode(int Parent, Vector3 Place, float Radius);
int Parent { get; init; }                                // The index of the point this one grew from, which comes before it, or -1 for the trunk's foot
Vector3 Place { get; init; }                             // Where it is, in meters, the trunk's foot at the origin
float Radius { get; init; }                              // The stem's radius there, in meters

struct Pcg                                               // The random numbers a tree grows from, O'Neill's PCG of 2014 with 64 bits of state and 32 out, written here so the same seed gives the same numbers on every runtime
Pcg(ulong seed);                                         // A generator started from a seed, as the paper's reference starts one
uint Next();                                             // The next 32 bits
float Unit();                                            // A number from zero up to one, one never reached, from the top 24 bits so every value is a float's exactly
float Signed();                                          // A number from minus one up to one
int Below(int count);                                    // A number from zero up to count, never reaching it

class TreePlanting                                       // Where trees stand and how each stands, a forest's places spread over a disc from a seed and a tree's variant, turn and size from its own seed, so a level that keeps its trees by species and seed stands the same trees in every engine that takes the module
int Variants = 8;                                        // How many variants a species grows, which the trees planted from it share, each turned and sized by its own seed
uint VariantSeed(int variant);                           // The seed a species' variant grows from, as Grow takes it
TreeStance Stance(uint seed);                            // How a tree planted with a seed stands, which variant of its species it is, its turn and its size
IReadOnlyList<(Vector3 Place, uint Seed)> Forest(uint seed, Vector3 middle, float radius, int count);  // Spreads a forest's trees over a disc on level ground, none nearer another than an even spread allows

record struct TreeStance                                 // How a tree stands where it is planted, from its seed by Stance
TreeStance(int Variant, float Turn, float Size);
int Variant { get; init; }                               // Which of its species' variants it is, from zero up to Variants, grown from VariantSeed of it
float Turn { get; init; }                                // How far it turns about the up axis, in radians from zero up to a whole turn, counterclockwise seen from above, as Quaternion.CreateFromAxisAngle(Vector3.UnitY, Turn) turns it
float Size { get; init; }                                // How large it stands against its variant, from nine tenths up to eleven tenths

class TreeTextures                                       // The vegetation's texture collection, the bark, leaf and grass sets the module carries by name, which a species names for its bark and its leaves and a game may add its own to
IReadOnlyList<TreeTextureSet> All { get; }               // Every set, the module's and those added, in the order of their names
IReadOnlyList<string> Files { get; }                     // The path in the collection of every file of every set, the module's and those of the folders added, in order
TreeTextureSet? Find(string name);                       // The set of a name, or nothing where the collection has none
Stream? Open(string path);                               // Opens a file of the collection by its path, as a set gives it, or answers nothing where the collection holds no file of that path
IReadOnlyList<TreeTextureSet> Add(string directory);     // Adds the sets of a folder on this machine, each in a folder of its name beside a textures.json naming them

record TreeTextureSet                                    // A set of the vegetation's texture collection, its files by their paths in the collection, found by name in TreeTextures
TreeTextureSet(string Name, TreeTextureKind Kind, string Albedo, string Normal, string Surface);
string Name { get; init; }                               // Its name, which a species gives as its BarkSet or LeafSet
TreeTextureKind Kind { get; init; }                      // Whether it is a bark, an atlas of leaves or a card of grass
string Albedo { get; init; }                             // The path of the color in RGB and the opacity in A, BC7 in sRGB
string Normal { get; init; }                             // The path of the tangent-space normal's X and Y, BC5, a two-channel normal map
string Surface { get; init; }                            // The path of occlusion in R, roughness in G, metallic in B and translucency in A, BC7
(int Width, int Height)? Size { get; init; }             // The albedo's width and height in texels, the normal and the surface being half of each, or nothing where the set's manifest does not say
IReadOnlyList<(float U, float V, float Width, float Height)> LeafBoxes { get; init; }  // Where each leaf of a leaf set's atlas is, as the share of the atlas's width and height from its left and top and the share it spans, its stalk at the bottom, or the whole atlas as one leaf where the manifest gives none
Vector3 MeanColor { get; init; }                         // The mean color of the albedo's opaque texels in linear sRGB, the summer color a fall tint is worked out against, or white where the manifest does not say
(float Occlusion, float Roughness, float Metallic, float Translucency) MeanSurface { get; init; }  // The mean of the surface's occlusion, roughness, metallic and translucency over the albedo's opaque texels, which a tree's far card, a picture of a whole branch, is drawn with, or a surface fully rough and lit through nowhere where the manifest does not say
(float Across, float Down)? Meters { get; init; }        // How many meters of a stem one repeat of a bark's picture covers, round the stem across its width and along it down its height, or nothing where the manifest does not say
string? Depth { get; init; }                             // The path of a bark's depth for parallax occlusion, one channel in BC4, white the deepest, a depth below the surface rather than a height, or nothing for a set without one

enum TreeTextureKind                                     // What a set of TreeTextures is for
Bark = 0,                                                // A bark that tiles round a stem and along it
Leaves = 1,                                              // An atlas of leaves with their opacity, cut out on the cards a tree's leaves are drawn on
Grass = 2,                                               // A card of grass blades with their opacity, the blades' roots along its bottom edge, cut out on the cards grass is drawn on

class TreeLeaves                                         // The leaves of a grown tree, a card a leaf along its twigs, cut from the species' leaf set or TreeLooks' atlas, or a frond along each stem of the last level where the species' leaves are fronds
TreeLeafMesh Build(TreeSkeleton skeleton, TreeSpecies species, uint seed);  // Makes the leaves of a grown tree, every leaf of it
TreeLeafMesh Build(TreeSkeleton skeleton, TreeSpecies species, uint seed, TreeDetail detail);  // Makes the leaves of a grown tree at a level of detail

class TreeLeafMesh                                       // A tree's leaves as vertex arrays, from Build, which an engine copies into a mesh of its own
IReadOnlyList<Vector3> Positions { get; }                // Each vertex's place in meters
IReadOnlyList<Vector3> Normals { get; }                  // Each vertex's normal at unit length, the way its card or strip faces
IReadOnlyList<Vector2> Uvs { get; }                      // Each vertex's texture coordinates in the leaves' atlas, from 0 to 1, U from the atlas's left and V from its top, a card's stalk at its box's bottom
IReadOnlyList<float>? Sky { get; }                       // Each vertex's share of the sky that reaches its card through the crown, from 0 to 1, from Shade, or nothing for fronds
IReadOnlyList<Vector3>? Out { get; }                     // Each vertex's way out of the crown at its card, at unit length, from Shade, or nothing for fronds
IReadOnlyList<Vector4> Limb { get; }                     // Each vertex's limb's pivot in meters in X, Y and Z and the limb's phase from 0 up to 1 in W, from Pivot of the limb its twig grows from
IReadOnlyList<Vector4> Own { get; }                      // Each vertex's own pivot in meters in X, Y and Z, the place its leaf flutters about, a card's stalk or a frond's foot, and the leaf's phase from 0 up to 1 in W
IReadOnlyList<uint> Indices { get; }                     // The triangles, three vertices each by their index
IReadOnlyList<int> Twigs { get; }                        // The node of the tip of the twig each card stands on, a card a leaf, which a far card drawn in place of a branch's leaves gathers them by; empty for fronds

enum TreeDetail                                          // How much of a tree a mesh draws, the whole of it near the camera or less of it further off
Full = 0,                                                // Every stem with all its sides and every leaf
Reduced = 1,                                             // Half the sides, the thinnest stems left out, and half the leaves, each larger

class TreeLimbs                                          // Which limb each stem of a tree sways with in the wind and where the limb pivots, worked out from the skeleton alone, which an engine writes into each vertex of the stems and the leaves for its wind to move them by
int Limbs = 16;                                          // How many of a tree's thickest stems are its limbs, the trunk among them
IReadOnlyList<int> Starts { get; }                       // The node each stem starts at, the trunk's, node 0, first and each after its parent's
IReadOnlySet<int> Kept { get; }                          // The nodes the limbs start at, the Limbs thickest stems
TreeLimbs Plan(TreeSkeleton skeleton);                   // Works out the limbs of a grown tree
int StemOf(int node);                                    // The node the stem a node stands on starts at
int OwnerOf(int stem);                                   // The limb a stem grows from, by where the limb starts, itself where it is a limb
int LimbOf(int stem);                                    // The limb a stem swings with, none for the trunk, the limb it grows from, or itself for a stem that grows from the trunk below every limb
Vector4 Pivot(int limb, Vector3 at);                     // A limb's pivot and phase, where it leaves its parent in X, Y and Z and its phase from 0 up to 1 in W, or a point's own place and no phase for no limb, so the point does not swing
float Phase(int number);                                 // A phase from 0 up to 1 for a limb or a leaf, by its number, spread by Knuth's hash

class TreeStems                                          // A grown tree's stems as tubes need them, each with its bark wrapped round and along it, which an engine builds its stems' meshes from with whatever tube it has
IReadOnlyList<TreeStem> Build(TreeSkeleton skeleton, TreeSpecies species);  // Gives a grown tree's stems with its species' bark wrapped round them
IReadOnlyList<TreeStem> Build(TreeSkeleton skeleton, (float Across, float Down) repeat);  // Gives a grown tree's stems with a bark of a given repeat wrapped round them

record TreeStem                                          // A stem of a tree as a tube needs it, its points from the joint where it leaves its parent, the radius at each, how many times its bark goes round it, and how far along the bark's picture each point stands, from Build
TreeStem(IReadOnlyList<Vector3> Points, IReadOnlyList<float> Radii, int Around, IReadOnlyList<float> Along);
IReadOnlyList<Vector3> Points { get; init; }             // The stem's points in meters, the joint on its parent first, Y up
IReadOnlyList<float> Radii { get; init; }                // The stem's radius at each point in meters
int Around { get; init; }                                // How many times the bark's picture goes round the stem, a whole count above zero
IReadOnlyList<float> Along { get; init; }                // How far along the bark's picture each point stands, in repeats from the stem's first point, a tube's texture coordinate along it, the picture's across being its coordinate round

class TreeCrown                                          // A tree's crown as a volume of leaves, from which each leaf takes how much of the sky reaches it through the leaves round it and which way is out of the crown at it, so an engine shades a leaf deep in the crown darker than one on its rim and bends its light toward the outside
(float[] Sky, Vector3[] Out) Shade(IReadOnlyList<Vector3> places, IReadOnlyList<float> areas);  // Finds how much of the sky each leaf sees through the crown and which way is out of the crown at it

class GrassScatter                                       // Where grass stands over a stretch of ground, the stretch cut into square patches and each patch's clumps scattered over it from a seed, which an engine draws as a mesh a patch, two cards crossed a clump
IReadOnlyList<GrassPatch> Patches(Vector2 size, GrassSettings settings);  // Cuts a stretch of ground into the patches its grass is drawn in
IReadOnlyList<GrassClump> Clumps(GrassSettings settings, Vector2 size, uint seed);  // Scatters a patch's clumps over it, a cell each of a grid as fine as the density asks

record GrassSettings                                     // What a stretch of grass is planted with and how thick it stands, as GrassScatter scatters it
GrassSettings();                                         // Settings with every value at its value unless given, a field of wheat grass
string Set { get; init; }                                // The set of the texture collection the cards are cut from, one of the kind Grass, wheat-grass unless given
float Density { get; init; }                             // How many clumps stand on a square meter, each two cards crossed
float Height { get; init; }                              // How tall a clump stands on average, in meters, its card as wide as the set's picture is for that height
float Variation { get; init; }                           // How far a clump's height strays from the average either way, as a share of it
uint Seed { get; init; }                                 // The seed the clumps are scattered, turned, sized and swayed by
float PatchSize { get; init; }                           // The side of the square patches the grass is drawn in, in meters, each a mesh of its own that the camera culls as one
bool CastsShadows { get; init; }                         // Whether the blades cast shadows, which many small cut-out cards cost more than they show and whose thin blades flicker in the sun's shadow map as they sway

record struct GrassPatch                                 // A square of a stretch of grass that an engine draws as one mesh, from Patches
GrassPatch(int Column, int Row, Vector2 Middle, Vector2 Size, uint Seed);
int Column { get; init; }                                // Which patch of its row it is, counted along X from the stretch's corner
int Row { get; init; }                                   // Which row it is in, counted along Z from the stretch's corner
Vector2 Middle { get; init; }                            // Where its middle stands from the stretch's middle, along X in X and along Z in Y, in meters
Vector2 Size { get; init; }                              // Its width along X in X and its depth along Z in Y, in meters
uint Seed { get; init; }                                 // The seed its clumps are scattered by, as Clumps takes it

record struct GrassClump                                 // A clump of grass, two cards crossed about its root, from Clumps
GrassClump(Vector3 Root, float Turn, float Height, float Swing, float Flutter);
Vector3 Root { get; init; }                              // The middle of its foot on the ground, at y 0, from its patch's middle, in meters
float Turn { get; init; }                                // How far its first card turns about the up axis from X, in radians from zero up to half a turn, the card lying along X cos Turn and Z sin Turn and the second card a quarter turn more
float Height { get; init; }                              // How tall it stands, in meters
float Swing { get; init; }                               // Its phase in the wind's swing along the wind, from zero up to one
float Flutter { get; init; }                             // Its phase in the flutter across the wind, from zero up to one
```
