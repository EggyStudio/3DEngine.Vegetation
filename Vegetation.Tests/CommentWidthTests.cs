using Xunit;

namespace Vegetation.Tests;

/// <summary>
/// Every comment line of the module's sources holds to 100 columns (NORM.md, N 4.11), the C#
/// comments, the scripts' and the workflows' comments, the Python scripts' docstrings and the
/// projects' MSBuild comments, a line holding an address alone apart.
/// </summary>
public sealed class CommentWidthTests
{
    private const int Width = 100;

    /// <summary>
    /// No comment line under the repository's sources runs past 100 columns, each found named by
    /// its path, its line and its width.
    /// </summary>
    [Fact]
    public void EveryCommentLineHoldsToAHundredColumns()
    {
        var root = RepositoryRoot();
        var longer = Sources(root)
            .SelectMany(path => CommentLines(path).Select(line => (Path.GetRelativePath(root, path), line.Number, line.Text)))
            .Where(line => line.Text.Length > Width && !IsAddress(line.Text))
            .Select(line => $"{line.Item1}:{line.Number} is {line.Text.Length} columns")
            .ToList();
        Assert.True(longer.Count == 0, string.Join(Environment.NewLine, longer));
    }

    // The files a contributor writes, outside the build's output folders.
    private static IEnumerable<string> Sources(string root)
    {
        string[] patterns = ["*.cs", "*.sh", "*.py", "*.yml", "*.csproj", "*.props", "*.targets"];
        return patterns
            .SelectMany(pattern => Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
            .Where(path => !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar)
                .Any(folder => folder is "bin" or "obj" or ".git" or "dump" or ".vegetation" or "waiting" or "modules"))
            .Order();
    }

    // Each line of a file that is a comment, numbered from one, by the comment marks of its kind. A
    // Python line is a comment from a hash, or inside or on the quotes of a docstring, the scripts
    // holding no other string in three quotes.
    private static IEnumerable<(int Number, string Text)> CommentLines(string path)
    {
        var lines = File.ReadAllLines(path);
        var extension = Path.GetExtension(path);
        var inside = false;
        for (var at = 0; at < lines.Length; at++)
        {
            var text = lines[at].TrimStart();
            var quotes = (text.Length - text.Replace("\"\"\"", "", StringComparison.Ordinal).Length) / 3;
            var comment = extension switch
            {
                ".cs" => text.StartsWith("//", StringComparison.Ordinal),
                ".py" => inside || quotes > 0 || text.StartsWith('#') && !text.StartsWith("#!", StringComparison.Ordinal),
                ".sh" or ".yml" => text.StartsWith('#') && !text.StartsWith("#!", StringComparison.Ordinal),
                _ => inside || text.StartsWith("<!--", StringComparison.Ordinal),
            };
            if (extension is ".csproj" or ".props" or ".targets")
                inside = (inside || text.Contains("<!--", StringComparison.Ordinal)) && !text.Contains("-->", StringComparison.Ordinal);
            if (extension == ".py" && quotes % 2 == 1)
                inside = !inside;
            if (comment) yield return (at + 1, lines[at]);
        }
    }

    // A comment whose text is one address, which no wrapping shortens.
    private static bool IsAddress(string line)
    {
        var words = line.Trim().TrimStart('/', '#').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length == 1 && Uri.IsWellFormedUriString(words[0], UriKind.Absolute);
    }

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
