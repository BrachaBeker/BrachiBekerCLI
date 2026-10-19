// הפקודה bundle: מגדירה את האפשרויות ואת בדיקות הקלט שלהן,
// ומעבירה את מה שהמשתמש בחר למחלקה Bundler שמבצעת את האריזה.
using System.CommandLine;

namespace Fid.Commands;

static class BundleCommand
{
    public static Command Create()
    {
        var languageOption = CreateLanguageOption();
        var outputOption = CreateOutputOption();

        var noteOption = new Option<bool>("--note", "-n")
        {
            Description = "Write the source file path as a comment above its code"
        };

        var sortOption = new Option<string>("--sort", "-s")
        {
            Description = "Order of the files: by file name or by code type",
            DefaultValueFactory = _ => "name"
        };
        sortOption.AcceptOnlyFromAmong(BundleSettings.SortValues);

        var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "-r")
        {
            Description = "Remove empty lines from the code"
        };

        var authorOption = new Option<string>("--author", "-a")
        {
            Description = "Author name to write at the top of the bundle"
        };

        var command = new Command("bundle", "Bundle code files into a single file")
        {
            languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption
        };

        // הפעולה: אוספת את הערכים שנותחו להגדרות, מריצה את האריזה על התיקייה הנוכחית ומדפיסה את התוצאה.
        // הערכים כבר עברו בדיקה בוולידטורים, ולכן כאן לא בודקים אותם שוב.
        command.SetAction(parseResult =>
        {
            Languages.TryGetExtensions(parseResult.GetValue(languageOption)!, out var extensions, out _);
            var settings = new BundleSettings(
                extensions,
                parseResult.GetValue(outputOption)!,
                parseResult.GetValue(noteOption),
                parseResult.GetValue(sortOption)!,
                parseResult.GetValue(removeEmptyLinesOption),
                parseResult.GetValue(authorOption));

            var result = Bundler.Run(Directory.GetCurrentDirectory(), settings);

            foreach (var warning in result.Warnings)
                Console.Error.WriteLine(warning);

            if (!result.Succeeded)
            {
                Console.Error.WriteLine(result.Error);
                return 1;
            }

            Console.WriteLine($"Bundled {result.Files} file(s), {result.Lines} line(s) into {settings.Output.FullName}");
            return 0;
        });

        return command;
    }

    // האפשרות --language: חובה, מקבלת כמה שפות אחרי אותו דגל,
    // ודוחה כבר בשלב הניתוח שם שפה שלא קיים ב-Languages.
    private static Option<string[]> CreateLanguageOption()
    {
        var option = new Option<string[]>("--language", "-l")
        {
            Description = $"Languages to include ({string.Join(", ", Languages.Names)}), or '{Languages.All}'",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };
        option.Validators.Add(result =>
        {
            if (!Languages.TryGetExtensions(result.Tokens.Select(t => t.Value), out _, out var error))
                result.AddError(error);
        });
        return option;
    }

    // האפשרות --output: שם או נתיב של קובץ הפלט, ברירת מחדל bundle.txt.
    // הבדיקה דוחה מראש נתיב שהוא תיקייה, נתיב בתיקייה שלא קיימת, או נתיב לא חוקי.
    private static Option<FileInfo> CreateOutputOption()
    {
        var option = new Option<FileInfo>("--output", "-o")
        {
            Description = "Bundle file name or full path",
            DefaultValueFactory = _ => new FileInfo("bundle.txt")
        };
        option.Validators.Add(result =>
        {
            var path = result.Tokens.FirstOrDefault()?.Value;
            if (path is null)
                return;
            try
            {
                var folder = Path.GetDirectoryName(Path.GetFullPath(path));
                if (Directory.Exists(path))
                    result.AddError($"'{path}' is a folder. Please give a file name.");
                else if (folder is not null && !Directory.Exists(folder))
                    result.AddError($"The folder '{folder}' does not exist.");
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                result.AddError($"'{path}' is not a valid path. {ex.Message}");
            }
        });
        return option;
    }
}
