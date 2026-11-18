using System.Reflection;
using Tools;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// The checks every module keeps, from the Tools submodule, a test each, that the library takes no
/// engine, window or graphics API, that neither a public member, its documentation nor any tracked
/// text names an engine, that the README names every public type, that the README's binding block
/// is the host's code, that the cheatsheet is what the built library writes, and that the README
/// opens with its link row.
/// </summary>
public sealed class ModuleTests
{
    // The library's assembly, the checkout's root and the source of the host that runs the
    // README's binding block, the three a module sets.
    private const string Library = "3DEngine.Vegetation";
    private const string Host = "Vegetation.Tests/BindingTests.cs";

    private static string Root
    {
        get
        {
            var at = new DirectoryInfo(AppContext.BaseDirectory);
            while (at is not null && !File.Exists(Path.Combine(at.FullName, "Vegetation.slnx")))
                at = at.Parent;
            return at?.FullName
                ?? throw new DirectoryNotFoundException("No Vegetation.slnx above the tests.");
        }
    }

    private static readonly string Deps = Path.Combine(AppContext.BaseDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".deps.json");
    private static readonly string Readme = File.ReadAllText(Path.Combine(Root, "README.md"));

    /// <summary>
    /// The library references no engine, window or graphics API, nor any package that does.
    /// </summary>
    [Fact]
    public void The_Library_Takes_No_Engine_Window_Or_Graphics_Api()
    {
        var fault = ModuleChecks.ReferenceFault(Assembly.Load(Library), Deps);
        Assert.True(fault is null, fault);
    }

    /// <summary>No public type or member, nor its documentation, names an engine.</summary>
    [Fact]
    public void No_Public_Name_Or_Its_Documentation_Names_An_Engine()
    {
        var fault = ModuleChecks.EngineNameFault(Assembly.Load(Library));
        Assert.True(fault is null, fault);
    }

    /// <summary>
    /// No tracked text of the checkout names an engine, its modules, review and norm apart, the
    /// assembly's and package's name of the form 3DEngine.Vegetation aside.
    /// </summary>
    [Fact]
    public void No_Tracked_Text_Names_An_Engine()
    {
        var fault = ModuleChecks.EngineNameInTextFault(Root);
        Assert.True(fault is null, fault);
    }

    /// <summary>The README names every public type of the library.</summary>
    [Fact]
    public void The_Readme_Names_Every_Public_Type()
    {
        var fault = ModuleChecks.UnnamedFault(Assembly.Load(Library), Readme);
        Assert.True(fault is null, fault);
    }

    /// <summary>
    /// The README's binding block stands word for word between the host's markers.
    /// </summary>
    [Fact]
    public void The_Readmes_Binding_Block_Stands_Word_For_Word_In_The_Host()
    {
        var fault = ModuleChecks.BindingBlock(Readme, File.ReadAllText(Path.Combine(Root, Host)));
        Assert.True(fault is null, fault);
    }

    /// <summary>
    /// The README opens with its link row to the devlog and the cheatsheet, above its title.
    /// </summary>
    [Fact]
    public void The_Readme_Opens_With_Its_Link_Row()
    {
        var fault = ModuleChecks.LinkRowFault(Assembly.Load(Library), Readme);
        Assert.True(fault is null, fault);
    }

    /// <summary>
    /// CHEATSHEET.md holds what the built library writes, its text round the list kept.
    /// </summary>
    [Fact]
    public void The_Cheatsheet_Is_What_The_Built_Library_Writes()
    {
        var fault = ModuleChecks.CheatsheetFault(Assembly.Load(Library), Readme, Path.Combine(Root, "CHEATSHEET.md"));
        Assert.True(fault is null, fault);
    }
}
