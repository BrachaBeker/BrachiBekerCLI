// הלוגיקה של האריזה: מוצאת את קובצי הקוד, ממיינת אותם, מחברת את התוכן שלהם ושומרת לקובץ אחד.
// המחלקה לא מדפיסה לקונסול ולא תלויה בשורת הפקודה. היא מחזירה BundleResult, והפקודה מחליטה מה להציג.
using System.Text;

namespace Fid;

static class Bundler
{
    // מריץ אריזה מלאה של התיקייה root לפי ההגדרות.
    // נכשל אם לא נמצאו קבצים, אם אף קובץ לא נקרא, או אם לא ניתן לשמור את קובץ הפלט.
    public static BundleResult Run(string root, BundleSettings settings)
    {
        var files = SelectFiles(root, settings);
        if (files.Count == 0)
            return new BundleResult(0, 0, [], "No code files of the selected languages were found in this folder.");

        var warnings = new List<string>();
        var (content, bundledFiles, bundledLines) = BuildContent(root, files, settings, warnings);
        if (bundledFiles == 0)
            return new BundleResult(0, 0, warnings, "Error: none of the code files could be read.");

        try
        {
            File.WriteAllText(settings.Output.FullName, content);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            return new BundleResult(bundledFiles, bundledLines, warnings,
                $"Error: could not save the bundle to '{settings.Output.FullName}'. {ex.Message}");
        }

        return new BundleResult(bundledFiles, bundledLines, warnings);
    }

    // מוצא את הקבצים מהשפות שנבחרו, בלי קובץ הפלט עצמו (שלא ייארז לתוך עצמו), וממיין אותם.
    // במקרה של שוויון ממיינים לפי הנתיב המלא, כדי שאותה תיקייה תיתן תמיד את אותו סדר.
    private static List<string> SelectFiles(string root, BundleSettings settings)
    {
        var files = FileFinder.FindFiles(root)
            .Where(f => settings.Extensions.Contains(Path.GetExtension(f)))
            .Where(f => !string.Equals(f, settings.Output.FullName, StringComparison.OrdinalIgnoreCase));

        var sorted = settings.Sort == "type"
            ? files.OrderBy(f => Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
                   .ThenBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                   .ThenBy(f => f, StringComparer.OrdinalIgnoreCase)
            : files.OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                   .ThenBy(f => f, StringComparer.OrdinalIgnoreCase);

        return sorted.ToList();
    }

    // מחבר את תוכן הקבצים לטקסט אחד: שם היוצר בראש, הערת מקור מעל כל קובץ (אם נבחר),
    // ושורה ריקה בין קבצים. קובץ שלא ניתן לקרוא מדולג, והסיבה נוספת לרשימת האזהרות.
    private static (string Content, int Files, int Lines) BuildContent(
        string root, List<string> files, BundleSettings settings, List<string> warnings)
    {
        var bundle = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(settings.Author))
            bundle.AppendLine(Languages.Comment(settings.Output.Extension, $"Author: {settings.Author}")).AppendLine();

        var bundledFiles = 0;
        var bundledLines = 0;
        foreach (var file in files)
        {
            IEnumerable<string> lines;
            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add($"Warning: skipped '{Path.GetRelativePath(root, file)}'. {ex.Message}");
                continue;
            }

            if (settings.Note)
                bundle.AppendLine(Languages.Comment(Path.GetExtension(file), $"Source: {Path.GetRelativePath(root, file)}"));

            if (settings.RemoveEmptyLines)
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line));

            foreach (var line in lines)
            {
                bundle.AppendLine(line);
                bundledLines++;
            }
            bundle.AppendLine();
            bundledFiles++;
        }

        return (bundle.ToString(), bundledFiles, bundledLines);
    }
}
