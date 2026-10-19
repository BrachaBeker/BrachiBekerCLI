# fid – Code Bundler CLI

A command line tool that packs the code files of a project into a single file.
Built with .NET 10 and `System.CommandLine`.

## Build

```
dotnet publish fid -c Release
```

The executable is created in `fid\bin\Release\net10.0\publish\fid.exe`.
Add that folder to the `PATH` environment variable to run `fid` from any folder.

## Project structure

| File | Responsibility |
| --- | --- |
| `Program.cs` | Entry point: creates the root command, adds the sub commands and runs |
| `Commands/BundleCommand.cs` | The `bundle` command: options, input validation, and passing the choices to `Bundler` |
| `Commands/CreateRspCommand.cs` | The `create-rsp` command: asks the questions and writes the response file |
| `Bundler.cs` | Bundling logic: selects, sorts and joins the files and saves the bundle (no console output) |
| `BundleSettings.cs` | The options chosen by the user, in one object |
| `BundleResult.cs` | The result of a bundle: counts, warnings and error |
| `Languages.cs` | Supported languages, their extensions and comment syntax |
| `FileFinder.cs` | Recursive file search that skips folders such as `bin` and `node_modules` |
| `ConsolePrompt.cs` | Console questions: free text, yes/no and file path |

## Commands

### `fid bundle`

Bundles the code files of the current folder (including sub folders) into one file.
Build and tool folders such as `bin`, `obj`, `node_modules` and `.git` are skipped.

| Option | Alias | Description |
| --- | --- | --- |
| `--language` | `-l` | **Required.** One or more languages, or `all` |
| `--output` | `-o` | Bundle file name or full path (default: `bundle.txt`) |
| `--note` | `-n` | Write the relative path of each source file as a comment above its code |
| `--sort` | `-s` | `name` (default) or `type` |
| `--remove-empty-lines` | `-r` | Remove empty lines from the code |
| `--author` | `-a` | Author name, written as a comment at the top of the bundle |

Supported languages: `csharp`, `java`, `python`, `javascript`, `typescript`, `html`, `css`, `sql`,
`cpp`, `c`, `php`, `go`, `rust`, `ruby`, `kotlin`, `swift`.

Examples:

```
fid bundle --language csharp
fid bundle -l csharp python -o "C:\temp\my bundle.txt" -n -s type
fid bundle -l all -r -a "Brachi"
```

### `fid create-rsp`

Asks for the value of every option, validates each answer, and saves a response file
with a ready `bundle` command.

```
fid create-rsp
fid "@bundle.rsp"
```

## Validation and errors

- An unknown language, an invalid sort value, or an output path whose folder does not exist
  are rejected before the bundle starts, with a clear message.
- A file that cannot be read (locked or without permission) is skipped with a warning,
  and the rest of the files are still bundled.
- The exit code is `0` on success and `1` on failure.
