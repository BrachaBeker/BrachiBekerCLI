// השפות שהכלי מכיר: הסיומות של כל שפה ותחביר ההערות של כל סוג קובץ.
// כל מקום בקוד שצריך לדעת משהו על שפות נשען על המחלקה הזו, כך שהוספת שפה נעשית רק כאן.
namespace Fid;

static class Languages
{
    public const string All = "all";

    private static readonly Dictionary<string, string[]> extensionsByLanguage = new(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp"] = [".cs"],
        ["java"] = [".java"],
        ["python"] = [".py"],
        ["javascript"] = [".js", ".jsx"],
        ["typescript"] = [".ts", ".tsx"],
        ["html"] = [".html", ".htm"],
        ["css"] = [".css"],
        ["sql"] = [".sql"],
        ["cpp"] = [".cpp", ".hpp", ".h"],
        ["c"] = [".c", ".h"],
        ["php"] = [".php"],
        ["go"] = [".go"],
        ["rust"] = [".rs"],
        ["ruby"] = [".rb"],
        ["kotlin"] = [".kt", ".kts"],
        ["swift"] = [".swift"],
    };

    public static IEnumerable<string> Names => extensionsByLanguage.Keys;

    // מתרגם רשימת שמות שפות (או "all") לקבוצת סיומות קבצים.
    // אם אחד השמות לא מוכר מחזיר false, ובפרמטר error הודעה עם רשימת השפות הנתמכות.
    public static bool TryGetExtensions(IEnumerable<string> names, out HashSet<string> extensions, out string error)
    {
        extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        error = "";

        foreach (var name in names)
        {
            if (name.Equals(All, StringComparison.OrdinalIgnoreCase))
                extensions.UnionWith(extensionsByLanguage.Values.SelectMany(e => e));
            else if (extensionsByLanguage.TryGetValue(name, out var found))
                extensions.UnionWith(found);
            else
            {
                error = $"Unknown language '{name}'. Supported: {string.Join(", ", Names)}, {All}";
                return false;
            }
        }
        return true;
    }

    // עוטף טקסט בתחביר ההערה המתאים לסוג הקובץ, כדי שההערה לא תשבור את הקוד.
    // ברירת המחדל היא // שמתאימה לרוב השפות וגם לקובץ txt.
    public static string Comment(string extension, string text) => extension.ToLowerInvariant() switch
    {
        ".py" or ".rb" => $"# {text}",
        ".html" or ".htm" => $"<!-- {text} -->",
        ".css" => $"/* {text} */",
        ".sql" => $"-- {text}",
        _ => $"// {text}"
    };
}
