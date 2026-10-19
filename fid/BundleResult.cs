// התוצאה של פעולת אריזה: כמה קבצים ושורות נארזו, אזהרות על קבצים שדולגו,
// והודעת שגיאה אם האריזה נכשלה.
namespace Fid;

record BundleResult(int Files, int Lines, IReadOnlyList<string> Warnings, string? Error = null)
{
    public bool Succeeded => Error is null;
}
