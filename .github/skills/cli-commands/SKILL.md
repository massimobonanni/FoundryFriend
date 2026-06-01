---
name: cli-commands
description: "Use this skill when adding, modifying, or reviewing CLI commands in a .NET project built with System.CommandLine. Triggers include: creating a new CLI command, adding options or arguments, wiring command handlers, registering subcommands, building command groups, or any architecture decision about CLI command structure. Also use when the user mentions 'System.CommandLine', 'CommandBase', 'SetAction', 'ParseResult', 'RootCommand', 'subcommand', or asks to add a verb to the CLI. Do NOT use for general C# coding, web APIs, UI work, or non-CLI projects."
---

# System.CommandLine CLI Developer Skill

You are working on a .NET CLI application built with **System.CommandLine v2.0.8** targeting **.NET 10**.
Follow these rules and patterns strictly when creating or modifying CLI commands.

---

## Architecture Overview

```
src/FoundryFriend.CLI/
├── Program.cs                  # Entry point: DI, root command, invocation
├── Commands/
│   ├── CommandBase.cs          # Abstract base — ALL commands inherit this
│   ├── RootCommand.cs          # Registers top-level subcommands
│   ├── CustomVersionAction.cs  # Custom --version behavior
│   ├── <Group>/                # One folder per command group
│   │   ├── <Group>Command.cs   # Parent command (registers children)
│   │   └── <Group><Verb>Command.cs  # Leaf command (has handler)
├── Extensions/                 # ParseResult, ServiceProvider, LoadingIndicator extensions
└── Utilities/                  # ConsoleUtility, LoadingIndicator
```

Shared contracts and models live in `FoundryFriend.Core/` (Interfaces, Entities, Configuration, Security).

---

## RULE 1 — Every Command Extends `CommandBase`

`CommandBase` is abstract, wraps `System.CommandLine.Command`, and holds the `ISessionManager` dependency.

```csharp
internal class MyCommand : CommandBase
{
    public MyCommand(ISessionManager sessionManager)
        : base("command-name", "Help text shown in --help", sessionManager)
    {
        // define options, arguments, subcommands
        this.SetAction(CommandHandler);
    }

    private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
    {
        // implementation
    }
}
```

**Never** inherit directly from `System.CommandLine.Command`. Always use `CommandBase`.

---

## RULE 2 — Options and Arguments

### Defining Options

```csharp
private readonly Option<string> _myOption;

// In constructor:
_myOption = new Option<string>("--my-option")
{
    Description = "Clear description of what this option does",
    Required = true,   // or false
};
_myOption.Aliases.Add("-m");       // Always add a short alias
this.Options.Add(_myOption);
```

### Defining Arguments (positional)

```csharp
private readonly Argument<string> _fileArgument;

// In constructor:
_fileArgument = new Argument<string>("file")
{
    Description = "Path to the input file"
};
this.Arguments.Add(_fileArgument);
```

### Reading Values in Handlers

```csharp
// Required option/argument — use GetValue:
var value = parseResult.GetValue(_myOption);

// Optional option — use the TryGetValue extension (ParseResultExtensions.cs):
if (parseResult.TryGetValue(_myOption, out var optionalValue))
{
    // option was provided
}
```

---

## RULE 3 — Command Handler Pattern

Handlers are **async methods** wired via `SetAction`:

```csharp
this.SetAction(CommandHandler);

private async Task CommandHandler(ParseResult parseResult, CancellationToken cancellationToken)
{
    // 1. Read option/argument values
    // 2. Load session settings (if needed)
    // 3. Validate configuration early — fail fast with clear error
    // 4. Execute business logic
    // 5. Output results with ConsoleUtility
}
```

---

## RULE 4 — Session Manager Usage

Most commands need endpoint/auth configuration. Always follow this pattern:

```csharp
await _sessionManager.LoadSettingsAsync();

var endpoint = _sessionManager.GetEndpoint();
if (string.IsNullOrWhiteSpace(endpoint))
{
    ConsoleUtility.WriteLine(
        "Error: endpoint not configured. Use 'set' command to configure it.",
        ConsoleColor.Red);
    return;
}

var authMode = _sessionManager.GetAuthenticationMode();
```

For **Identity-only operations**, check auth mode early:

```csharp
if (authMode == AuthenticationMode.Key)
{
    ConsoleUtility.WriteLine(
        "Error: This operation requires Identity authentication.",
        ConsoleColor.Red);
    return;
}
```

---

## RULE 5 — Console Output via `ConsoleUtility`

**Never** use `Console.WriteLine` for application output. Use `ConsoleUtility`:

| Method | Usage |
|--------|-------|
| `ConsoleUtility.WriteLine(msg, color)` | Full line with color |
| `ConsoleUtility.Write(msg, color)` | Partial line (prompts, streaming) |
| `ConsoleUtility.WriteLineWithTimestamp(msg, color)` | `[HH:mm:ss.fff] - msg` |

### Color Conventions

| Color | Meaning |
|-------|---------|
| `ConsoleColor.Green` | Success, banners, connection status |
| `ConsoleColor.Red` | Errors |
| `ConsoleColor.Yellow` | AI/assistant responses |
| `ConsoleColor.White` | User prompts, neutral information |
| `ConsoleColor.Cyan` | Progress, informational messages |

---

## RULE 6 — Command Group (Parent with Subcommands)

A **group command** registers children but does **not** call `SetAction`:

```csharp
internal class MyGroupCommand : CommandBase
{
    public MyGroupCommand(ISessionManager sessionManager)
        : base("mygroup", "Manages my-group resources", sessionManager)
    {
        this.Subcommands.Add(new MyGroupListCommand(sessionManager));
        this.Subcommands.Add(new MyGroupCreateCommand(sessionManager));
        this.Subcommands.Add(new MyGroupDeleteCommand(sessionManager));
    }
}
```

A **hybrid command** can have both a handler AND subcommands (see `SetCommand` — it handles `set --endpoint ...` directly while also having `set show` and `set clear` subcommands).

---

## RULE 7 — Registration

- **Top-level commands** → register in `RootCommand.cs`:
  ```csharp
  this.Subcommands.Add(new MyGroupCommand(serviceProvider.GetSessionManager()));
  ```

- **Subcommands** → register inside the parent command's constructor:
  ```csharp
  this.Subcommands.Add(new MyGroupCreateCommand(sessionManager));
  ```

---

## RULE 8 — User Confirmation for Destructive Operations

```csharp
Console.WriteLine("Are you sure you want to delete X? This action cannot be undone. (yes/no)");
var confirmation = Console.ReadLine();
if (confirmation?.ToLower() != "yes" && confirmation?.ToLower() != "y")
{
    ConsoleUtility.WriteLine("Operation cancelled.", ConsoleColor.Red);
    return;
}
```

---

## RULE 9 — Loading Indicator for Long Operations

```csharp
var result = await SomeLongRunningCallAsync()
    .WithLoadingIndicator(
        message: "Fetching data",
        style: LoadingIndicator.Style.Spinner,
        completionMessage: "Done",
        showTimeTaken: true);
```

Available styles: `Spinner`, `Dots`, `Arrow`, `Braille`.

---

## RULE 10 — Dependency Injection

Services are registered in `Program.cs` as **singletons**:

```csharp
serviceCollection.TryAddSingleton<IMyService, MyServiceImpl>();
```

Add a convenience extension in `ServiceProviderExtensions.cs`:

```csharp
public static IMyService GetMyService(this ServiceProvider provider)
    => provider.GetRequiredService<IMyService>();
```

Interfaces go in `FoundryFriend.Core/Interfaces/`, implementations in Core or CLI depending on scope.

---

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| CLI command name | lowercase kebab-case | `agent create`, `set show` |
| Command class | PascalCase + `Command` suffix | `AgentCreateCommand` |
| Option field | `_camelCaseOption` (private readonly) | `_projectNameOption` |
| Option long name | `--kebab-case` | `--project-name` |
| Option short alias | `-x` (1-2 chars) | `-p`, `-id`, `-md` |
| Argument field | `_camelCaseArgument` | `_fileArgument` |
| Namespace | `FoundryFriend.CLI.Commands.<Group>` | `FoundryFriend.CLI.Commands.Agent` |
| Folder | `Commands/<Group>/` | `Commands/Agent/` |

---

## Visibility

- All command classes are `internal`.
- All extension classes are `internal static`.
- Public visibility is only used in `FoundryFriend.Core` interfaces and entities that need cross-project access.

---

## Checklist for New Commands

When creating a new command, verify:

1. ✅ Inherits from `CommandBase` (not `System.CommandLine.Command`)
2. ✅ Constructor passes `name`, `description`, `sessionManager` to base
3. ✅ All options have `Description`, `Required`, and a short alias
4. ✅ Handler wired via `this.SetAction(CommandHandler)`
5. ✅ Handler signature: `async Task CommandHandler(ParseResult, CancellationToken)`
6. ✅ Session settings loaded if needed (`_sessionManager.LoadSettingsAsync()`)
7. ✅ Early validation with colored error messages
8. ✅ Output uses `ConsoleUtility` with correct color conventions
9. ✅ Command registered in parent (RootCommand or group command)
10. ✅ Class is `internal`
11. ✅ File placed in `Commands/<Group>/` folder
12. ✅ Namespace matches folder: `FoundryFriend.CLI.Commands.<Group>`
