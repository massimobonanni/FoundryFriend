# FoundryFriend.CLI

The CLI application is the entry point of FoundryFriend. It provides an interactive terminal interface for chatting with Azure AI Foundry model deployments and managing AI agents in Foundry projects.

## Architecture

The project follows a clean command-handler pattern built on [System.CommandLine](https://learn.microsoft.com/dotnet/standard/commandline/) v2.0.8 with dependency injection via `Microsoft.Extensions.DependencyInjection`.

```
FoundryFriend.CLI/
├── Program.cs                  # Entry point — DI registration, root command setup, invocation
├── Commands/
│   ├── CommandBase.cs          # Abstract base class for all commands
│   ├── RootCommand.cs          # Registers top-level subcommands
│   ├── Set/                    # Session configuration (endpoint, auth mode, access key)
│   │   ├── SetCommand.cs       # Hybrid: handles set options + has subcommands
│   │   ├── SetShowCommand.cs   # Displays current settings
│   │   └── SetClearCommand.cs  # Clears all settings
│   ├── Chat/                   # Direct model deployment chat
│   │   └── ChatCommand.cs      # Interactive multi-turn chat via IChatService
│   ├── Agent/                  # Agent management
│   │   ├── AgentCommand.cs     # Parent group command
│   │   ├── AgentConfiguration.cs   # JSON-serializable agent definition (loaded from config files)
│   │   ├── AgentCreateCommand.cs   # Create agent from JSON config
│   │   ├── AgentListCommand.cs     # List agents in a project
│   │   ├── AgentChatCommand.cs     # Interactive chat with an agent
│   │   └── AgentDeleteCommand.cs   # Delete an agent
│   └── Login/                  # Azure authentication
│       └── LoginCommand.cs     # Delegates to 'az login' via IAzCliService
├── Services/                   # Service implementations (injected into commands)
│   ├── ChatService.cs          # IChatService — model deployment chat via Azure AI Inference
│   ├── AgentChatService.cs     # IAgentChatService — agent conversation via Project Responses API
│   ├── AgentService.cs         # IAgentService — agent CRUD via Agent Administration API
│   └── AzCliService.cs         # IAzCliService — runs Azure CLI commands as external processes
├── Extensions/
│   ├── ServiceProviderExtensions.cs   # Typed service resolution helpers
│   ├── ParseResultExtensions.cs       # TryGetValue for optional options
│   └── LoadingIndicatorExtensions.cs  # .WithLoadingIndicator() for async calls
├── Utilities/
│   ├── ConsoleUtility.cs       # Colored console output (Write, WriteLine, WriteLineWithTimestamp)
│   └── LoadingIndicator.cs     # Spinner/dots/arrow/braille progress indicators
└── AgentSamples/               # Sample agent configuration JSON files
```

### Key design principles

- **Thin command handlers** — Commands only parse input, validate configuration, and call a service method. All business logic lives in service classes injected via DI.
- **All commands extend `CommandBase`** — This abstract class wraps `System.CommandLine.Command` and carries the `ISessionManager` dependency.
- **Console output via `ConsoleUtility`** — Never use `Console.WriteLine` directly. Colors follow a convention: green = success, red = errors, yellow = AI responses, cyan = progress, white = user prompts.

## Dependencies

| Package | Purpose |
|---------|---------|
| `System.CommandLine` 2.0.8 | CLI argument parsing and command routing |
| `Azure.AI.Projects` | Azure AI Foundry project client, agent administration, conversations |
| `Azure.AI.Inference` | Chat completions client for model deployments |
| `Azure.Identity` | `DefaultAzureCredential` for identity-based authentication |
| `Figgle` + `Figgle.Fonts` | ASCII art application banner |
| `Microsoft.Extensions.DependencyInjection` | Singleton/transient service registration |

## Building and Running

```bash
# Build
dotnet build

# Run directly
dotnet run -- <command> [options]

# Or use the compiled executable
./bin/Debug/net10.0/foundryfriend <command> [options]
```

### Quick start

```bash
# 1. Log in to Azure (opens browser for interactive authentication)
dotnet run -- login

# 2. Log in to a specific tenant using device code (headless environments)
dotnet run -- login --tenant contoso.com --device-code

# 3. Configure the Foundry endpoint
dotnet run -- set --endpoint https://<your-foundry-endpoint> --auth-mode Identity

# 4. Chat with a model deployment
dotnet run -- chat --model-deployment gpt-4.1

# 5. List agents in a project
dotnet run -- agent list --project-name my-project

# 6. Chat with an agent
dotnet run -- agent chat --agent-id asst_abc123 --project-name my-project
```

## Commands Reference

### `login`

Log in to Azure by delegating to `az login`. Requires the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) to be installed and available on `PATH`.

| Option | Alias | Description |
|--------|-------|-------------|
| `--tenant <id\|domain>` | `-t` | Azure AD tenant ID or domain name to log in to |
| `--device-code` | `-dc` | Use device code flow (headless / remote environments) |

```bash
foundryfriend login
foundryfriend login --tenant contoso.com
foundryfriend login --device-code
foundryfriend login -t 00000000-0000-0000-0000-000000000000 -dc
```

## Adding a New Command

1. Create a class in `Commands/<Group>/` inheriting from `CommandBase`
2. Define options/arguments in the constructor, wire the handler via `this.SetAction(CommandHandler)`
3. Create a service interface in `FoundryFriend.Core/Interfaces/` and implementation in `Services/`
4. Register the service in `Program.cs` and add a resolution helper in `ServiceProviderExtensions.cs`
5. Register the command in its parent command's constructor

See the [CLI skill documentation](../../.github/skills/cli-commands/) for the full checklist and conventions.
