// ההגדרות של פעולת אריזה אחת, כפי שהמשתמש בחר אותן בשורת הפקודה.
namespace Fid;

record BundleSettings(
    HashSet<string> Extensions,
    FileInfo Output,
    bool Note,
    string Sort,
    bool RemoveEmptyLines,
    string? Author)
{
    // הערכים המותרים לאפשרות המיון: לפי שם הקובץ או לפי סוג הקוד (סיומת)
    public static readonly string[] SortValues = ["name", "type"];
}
