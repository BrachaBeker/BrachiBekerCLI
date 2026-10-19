// נקודת הכניסה של הכלי fid.
// יוצרת את פקודת השורש, מחברת אליה את שתי תתי-הפקודות ומריצה את מה שהמשתמש הקליד.
using System.CommandLine;
using Fid.Commands;

var rootCommand = new RootCommand("CLI tool for bundling code files");
rootCommand.Subcommands.Add(BundleCommand.Create());
rootCommand.Subcommands.Add(CreateRspCommand.Create());

// מנתח את הארגומנטים, מריץ את הפקודה המתאימה ומחזיר את קוד היציאה שלה (0 הצלחה, 1 כישלון)
return rootCommand.Parse(args).Invoke();
