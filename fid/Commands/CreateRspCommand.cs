// הפקודה create-rsp: שואלת את המשתמש על כל אפשרות, בודקת כל תשובה,
// ושומרת קובץ תגובה (rsp) עם פקודת bundle מוכנה. מריצים אותו עם: fid "@bundle.rsp"
using System.CommandLine;
using System.Text;

namespace Fid.Commands;

static class CreateRspCommand
{
    public static Command Create()
    {
        var command = new Command("create-rsp", "Create a response file with a ready bundle command");
        command.SetAction(_ => Run());
        return command;
    }

    // אוספת את כל התשובות לפי הסדר, בונה מהן את פקודת bundle ושומרת אותה בקובץ.
    // מחזירה 0 בהצלחה ו-1 אם לא ניתן לשמור את הקובץ.
    private static int Run()
    {
        var language = AskLanguages();
        var output = ConsolePrompt.AskPath("Bundle file name or full path (Enter for bundle.txt):",
            "The path contains invalid characters.");
        var note = ConsolePrompt.AskYesNo("Write the source of each file as a comment?");
        var sort = AskSort();
        var removeEmptyLines = ConsolePrompt.AskYesNo("Remove empty lines?");
        // מרכאות נמחקות מהשם, כי השם נעטף במרכאות בפקודה ומרכאה בתוכו הייתה שוברת אותה
        var author = ConsolePrompt.Ask("Author name (Enter to skip):").Replace("\"", "");

        var rspFile = ConsolePrompt.AskPath("Response file name (Enter for bundle.rsp):",
            "The file name contains invalid characters.");
        if (rspFile == "")
            rspFile = "bundle.rsp";

        var commandLine = BuildCommandLine(language, output, note, sort, removeEmptyLines, author);

        try
        {
            File.WriteAllText(rspFile, commandLine);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            Console.Error.WriteLine($"Error: could not save the response file '{rspFile}'. {ex.Message}");
            return 1;
        }

        Console.WriteLine($"Response file created. Run it with: fid \"@{rspFile}\"");
        return 0;
    }

    // שואלת אילו שפות לארוז עד שמתקבלת לפחות שפה אחת, וכל השפות מוכרות.
    // הבדיקה זהה לזו של האפשרות --language, כי שתיהן משתמשות ב-Languages.TryGetExtensions.
    private static string AskLanguages()
    {
        while (true)
        {
            var language = ConsolePrompt.Ask(
                $"Languages, separated by spaces ({string.Join(", ", Languages.Names)}), or '{Languages.All}':");
            var tokens = language.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                Console.WriteLine("At least one language is required.");
            else if (!Languages.TryGetExtensions(tokens, out _, out var error))
                Console.WriteLine(error);
            else
                return language;
        }
    }

    // שואלת לפי מה למיין עד שמתקבל ערך מותר. תשובה ריקה אומרת ברירת מחדל (name).
    private static string AskSort()
    {
        while (true)
        {
            var sort = ConsolePrompt.Ask("Sort files by 'name' or 'type' (Enter for name):").ToLowerInvariant();
            if (sort == "" || BundleSettings.SortValues.Contains(sort))
                return sort;
            Console.WriteLine("Please type 'name' or 'type'.");
        }
    }

    // בונה את שורת הפקודה. אפשרות שנשארה בברירת המחדל לא נכתבת,
    // ונתיב ושם יוצר נעטפים במרכאות כדי שרווחים בתוכם לא יפצלו אותם.
    private static string BuildCommandLine(string language, string output, bool note, string sort,
        bool removeEmptyLines, string author)
    {
        var command = new StringBuilder($"bundle --language {language}");
        if (output != "")
            command.Append($" --output \"{output}\"");
        if (note)
            command.Append(" --note");
        if (sort != "")
            command.Append($" --sort {sort}");
        if (removeEmptyLines)
            command.Append(" --remove-empty-lines");
        if (author != "")
            command.Append($" --author \"{author}\"");
        return command.ToString();
    }
}
