using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// No public type or member of the library, and no documentation of one, names either engine, so a
/// stranger's engine reads the library as written for it, the engines appearing in the README alone
/// as two bindings of it.
/// </summary>
public sealed class NamesTests
{
    // The engines' names and the bridge one of them binds through, each a whole word.
    private static readonly Regex Engine = new(@"\b(3DEngine|BevyCSharp|Bevy|bridge)\b", RegexOptions.IgnoreCase);

    /// <summary>
    /// The public types' and members' names, and every summary, remark, parameter and return of the
    /// documentation the build writes, name no engine, each found named by its member.
    /// </summary>
    [Fact]
    public void NoPublicNameOrDocumentationNamesAnEngine()
    {
        var library = Assembly.Load("3DEngine.Vegetation");
        const BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var names = library.GetExportedTypes()
            .SelectMany(type => type.GetMembers(Declared).Select(member => $"{type.FullName}.{member.Name}").Prepend(type.FullName!));

        var file = Path.Combine(AppContext.BaseDirectory, "3DEngine.Vegetation.xml");
        Assert.True(File.Exists(file), $"The build wrote no documentation at {file}.");
        var documented = XDocument.Load(file).Descendants("member")
            .Select(member => $"{member.Attribute("name")!.Value} {member.Value}");

        var found = names.Concat(documented).Where(text => Engine.IsMatch(text)).ToList();
        Assert.True(found.Count == 0, string.Join(Environment.NewLine, found));
    }
}
