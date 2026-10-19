// שאלות למשתמש בקונסול: שאלה חופשית, שאלת כן/לא ושאלה על נתיב קובץ.
// משמשת את הפקודה create-rsp.
namespace Fid;

static class ConsolePrompt
{
    // מציג שאלה ומחזיר את התשובה בלי רווחים מסביב.
    // אם הקלט נגמר (Ctrl+Z או קלט מקובץ שהסתיים) התוכנית יוצאת, כדי שלולאות השאלות לא ייתקעו.
    public static string Ask(string question)
    {
        Console.Write(question + " ");
        var answer = Console.ReadLine();
        if (answer is null)
        {
            Console.Error.WriteLine("Input ended unexpectedly.");
            Environment.Exit(1);
        }
        return answer.Trim();
    }

    // שואל שאלת כן/לא עד שמתקבלת תשובה מובנת. Enter בלי תשובה נחשב "לא".
    public static bool AskYesNo(string question)
    {
        while (true)
        {
            var answer = Ask(question + " (y/n, Enter for no):").ToLowerInvariant();
            if (answer is "y" or "yes")
                return true;
            if (answer is "" or "n" or "no")
                return false;
            Console.WriteLine("Please answer y or n.");
        }
    }

    // שואל על נתיב עד שמתקבלת תשובה בלי תווים אסורים. מסיר מרכאות מסביב (כמו בנתיב שהועתק מ-Windows).
    // תשובה ריקה מותרת, והמתקשר מחליט מה ברירת המחדל.
    public static string AskPath(string question, string invalidMessage)
    {
        while (true)
        {
            var path = Ask(question).Trim('"');
            if (path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                return path;
            Console.WriteLine(invalidMessage);
        }
    }
}
