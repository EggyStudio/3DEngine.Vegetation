using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The library references no engine, no window and no graphics API, read from what the build made
/// of it, its assembly's references and the packages the test's dependency file records for it.
/// </summary>
/// <remarks>
/// A module copies this file whole into its tests and sets <see cref="Library"/> to its own
/// assembly's name, 3DEngine and the module's, as <c>3DEngine.Vegetation</c> here, so a stranger
/// takes the library into an engine of their own with nothing of another engine's coming along
/// (NORM.md, N 2.1). The project file is not read, since a reference a property or an imported file
/// adds stands in the build alone.
/// </remarks>
public class ReferenceTests
{
    // The library's assembly name, which a module that copies this file sets to its own.
    private const string Library = "3DEngine.Vegetation";

    // The engines' own assemblies, and the windows, the graphics APIs and the bindings over them a
    // .NET program draws through, each matched as a whole part of a name between dots and dashes,
    // so SDL3-CS, Silk.NET.Vulkan and Twizzle.ImGui-Bundle.NET are refused and Metadata is not.
    private static readonly string[] Refused =
    [
        "3DEngine", "BevyCSharp",
        "SDL", "SDL2", "SDL3", "Glfw", "GLFW.NET", "Raylib", "Raylib-cs", "ImGui", "ImGui.NET",
        "Silk.NET", "Vortice", "Vulkan", "VulkanSharp", "OpenTK", "OpenGL", "OpenAL", "SharpDX",
        "DirectX", "Direct3D", "WebGPU", "Wgpu", "Veldrid", "SkiaSharp", "MonoGame", "FNA",
        "Stride", "Avalonia", "System.Windows.Forms", "PresentationFramework", "Microsoft.Maui",
    ];

    [Fact]
    public void The_Library_References_No_Engine_Window_Or_Graphics_Api()
    {
        var references = Assembly.Load(Library).GetReferencedAssemblies().Select(name => name.Name!).ToList();
        var packages = Packages(Library);

        var refused = references.Concat(packages).Where(IsRefused).Distinct().ToList();

        Assert.True(refused.Count == 0, $"{Library} references {string.Join(", ", refused)}, an engine, a window or a graphics API (NORM.md, N 2.1).");
        Assert.Contains("System.Runtime", references);
    }

    [Theory]
    [InlineData("3DEngine", true)]
    [InlineData("BevyCSharp", true)]
    [InlineData("SDL3-CS", true)]
    [InlineData("Vortice.Vulkan", true)]
    [InlineData("Twizzle.ImGui-Bundle.NET", true)]
    [InlineData("Silk.NET.Vulkan", true)]
    [InlineData("Raylib-cs", true)]
    [InlineData("System.Runtime", false)]
    [InlineData("System.Text.RegularExpressions", false)]
    [InlineData("System.Reflection.Metadata", false)]
    [InlineData("xunit.core", false)]
    public void A_Reference_Is_Refused_Where_A_Part_Of_Its_Name_Is_A_Refused_Name(string name, bool refused)
    {
        Assert.Equal(refused, IsRefused(name));
    }

    private static bool IsRefused(string name) =>
        Refused.Any(refused => Regex.IsMatch(name, $@"(^|[.-]){Regex.Escape(refused)}($|[.-])", RegexOptions.IgnoreCase));

    // The packages the build resolved for the library and those they bring in turn, from the test
    // assembly's dependency file, whose targets name each library and package with its version.
    private static SortedSet<string> Packages(string library)
    {
        var file = Path.Combine(AppContext.BaseDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".deps.json");
        using var deps = JsonDocument.Parse(File.ReadAllText(file));
        var target = deps.RootElement.GetProperty("targets").EnumerateObject().First().Value;
        var entries = target.EnumerateObject().ToDictionary(entry => entry.Name.Split('/')[0], entry => entry.Value);
        Assert.True(entries.ContainsKey(library), $"{file} records no {library}.");

        var found = new SortedSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>([library]);
        while (pending.TryPop(out var name))
            if (entries.TryGetValue(name, out var entry) && entry.TryGetProperty("dependencies", out var dependencies))
                foreach (var dependency in dependencies.EnumerateObject())
                    if (found.Add(dependency.Name))
                        pending.Push(dependency.Name);
        return found;
    }
}
