namespace RolemasterCharacterCreation.Services;

/// <summary>
/// Resolves the on-disk location of the <c>docs/</c> tree the app reads at runtime.
/// Published apps carry docs under the content root (the build copies them to <c>./docs</c>);
/// local dev keeps the docs tree one level up from the project (<c>../docs</c>). Callers combine
/// the returned root with a docs-relative path, e.g.
/// <c>Path.Combine(DocsLocator.Root(env.ContentRootPath), "game-data/skills.md")</c>.
/// </summary>
public static class DocsLocator
{
    public static string Root(string contentRootPath)
    {
        var underRoot = Path.Combine(contentRootPath, "docs");
        return Directory.Exists(underRoot)
            ? underRoot
            : Path.GetFullPath(Path.Combine(contentRootPath, "..", "docs"));
    }
}
