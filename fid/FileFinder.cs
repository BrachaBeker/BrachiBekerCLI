// חיפוש קבצים בתיקייה ובכל תתי-התיקיות שלה,
// תוך דילוג על תיקיות של פלט בנייה, ספריות חיצוניות וכלי פיתוח.
namespace Fid;

static class FileFinder
{
    private static readonly HashSet<string> excludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "debug", "release", "publish", "node_modules", ".git", ".vs", ".vscode", ".idea"
    };

    // מחזיר את כל הקבצים בתיקייה, ונכנס ברקורסיה לכל תת-תיקייה שאינה ברשימת המוחרגות.
    // הקבצים מוחזרים אחד-אחד (yield), ותיקיות ללא הרשאה מדולגות בשקט.
    public static IEnumerable<string> FindFiles(string folder)
    {
        var options = new EnumerationOptions { IgnoreInaccessible = true };

        foreach (var file in Directory.EnumerateFiles(folder, "*", options))
            yield return file;

        foreach (var subFolder in Directory.EnumerateDirectories(folder, "*", options))
        {
            if (excludedFolders.Contains(Path.GetFileName(subFolder)))
                continue;
            foreach (var file in FindFiles(subFolder))
                yield return file;
        }
    }
}
