# FoundryFriend

FoundryFriend is a command-line interface (CLI) tool for interacting with [Azure AI Foundry](https://learn.microsoft.com/azure/ai-foundry/). It lets you configure your Foundry session, chat directly with model deployments, and manage AI agents — all from the terminal. The infrastructure is deployable via the [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/).

## Project Structure

```
FoundryFriend/
├── azure.yaml                        # Azure Developer CLI project configuration
├── infra/                            # Infrastructure as Code (Bicep)
│   ├── main.bicep                    # Main Bicep template
│   ├── main.bicepparam               # Bicep parameters file
│   └── modules/
│       ├── foundry.bicep             # Azure AI Foundry resources
│       └── monitoring.bicep          # Monitoring resources
└── src/
    ├── FoundryFriend.slnx            # Solution file
    ├── FoundryFriend.CLI/            # CLI application (entry point)
    │   ├── Program.cs
    │   ├── Commands/                 # Command implementations
    │   │   ├── Agent/                # Agent management commands
    │   │   ├── Chat/                 # Chat command
    │   │   └── Set/                  # Configuration commands
    │   ├── AgentSamples/             # Sample agent configuration files
    │   ├── Extensions/
    │   └── Utilities/
    └── FoundryFriend.Core/           # Core library (session management, entities)
        ├── Configuration/
        ├── Entities/
        ├── Interfaces/
        └── Security/
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- An active Azure subscription
- An Azure AI Foundry project with at least one model deployment

### Deploy with AZD

The following commands provision the required Azure infrastructure and configure the project.

**1. Log in to Azure**

```bash
azd auth login
```

**2. Initialize the environment** *(first time only)*

```bash
azd init
```

**3. Provision and deploy**

```bash
azd up
```

This single command provisions the Bicep infrastructure (Azure AI Foundry resources and monitoring) and deploys the application.

**Tear down**

```bash
azd down
```

---

### Set up the `foundryfriend` alias (optional)

`Manage-ProfileCommands.ps1` is a PowerShell helper script that registers a `foundryfriend` alias in your PowerShell profile, pointing to the compiled CLI executable. This lets you run `foundryfriend` from any directory without specifying the full path to the binary.

> **Note:** Build the project first (`dotnet build`) so the executable is available at `src\FoundryFriend.CLI\bin\Debug\net10.0\foundryfriend.exe`.

**Add the alias to your profile**

```powershell
.\Manage-ProfileCommands.ps1
```

**Add without confirmation prompt**

```powershell
.\Manage-ProfileCommands.ps1 -Force
```

**Remove the alias from your profile**

```powershell
.\Manage-ProfileCommands.ps1 -Remove
```

After adding the alias, reload your profile or restart PowerShell:

```powershell
. $PROFILE
```

---

## CLI Commands

All commands follow the pattern:

```
foundryfriend <command> [subcommand] [options]
```

---

### `set` — Configure session settings

Stores the Foundry endpoint, authentication mode, and access key for the current session. Settings are persisted locally across runs.

| Option | Alias | Description | Required |
|---|---|---|---|
| `--endpoint` | `-e` | The Azure AI Foundry service endpoint URL | No |
| `--auth-mode` | `-am` | Authentication mode: `Identity` (Azure login) or `Key` (access key) | No |
| `--key` | `-k` | The Foundry service access key (only used when `--auth-mode Key`) | No |

**Examples**

```bash
# Configure endpoint with identity-based authentication (requires az login)
foundryfriend set --endpoint https://<your-foundry-endpoint> --auth-mode Identity

# Configure endpoint with access key authentication
foundryfriend set --endpoint https://<your-foundry-endpoint> --auth-mode Key --key <your-access-key>
```

---

#### `set show` — Display current settings

Shows the currently stored endpoint, authentication mode, and (masked) access key.

```bash
foundryfriend set show
```

---

#### `set clear` — Clear all settings

Removes all stored session settings after user confirmation.

```bash
foundryfriend set clear
```

---

### `chat` — Chat with a model deployment

Starts an interactive chat session with a specific model deployment in Azure AI Foundry.

| Option | Alias | Description | Required |
|---|---|---|---|
| `--model-deployment` | `-md` | The model deployment name in Azure AI Foundry | Yes |
| `--system` | `-sm` | An optional system message to set the assistant's behavior | No |

**Examples**

```bash
# Basic chat with a deployment
foundryfriend chat --model-deployment gpt-4.1

# Chat with a custom system message
foundryfriend chat --model-deployment gpt-4.1 --system "You are a helpful coding assistant."
```

> **Note:** The Foundry endpoint must be configured before using this command (`foundryfriend set --endpoint ...`).

---

### `agent` — Manage agents in Foundry

Parent command for all agent-related operations. Requires identity-based authentication (`--auth-mode Identity`); access key authentication is not supported for agent commands.

---

#### `agent create` — Create an agent from a configuration file

Creates a new AI agent in a Foundry project using a JSON configuration file.

| Argument / Option | Alias | Description | Required |
|---|---|---|---|
| `file` *(argument)* | — | Path to the agent configuration JSON file | Yes |
| `--project-name` | `-p` | The name of the Foundry project to create the agent in | Yes |

**Agent configuration file format**

```json
{
  "id": "my-agent-id",
  "name": "My Agent",
  "description": "A short description of the agent.",
  "modelDeploymentName": "gpt-4.1",
  "instructions": [
    "You are a helpful assistant.",
    "Always respond clearly and concisely."
  ]
}
```

Sample agent files are available in `src/FoundryFriend.CLI/AgentSamples/`.

**Examples**

```bash
# Create an agent from a file
foundryfriend agent create ./AgentSamples/math-tutor.json --project-name my-foundry-project

# Create a customer support agent
foundryfriend agent create ./AgentSamples/customer-support.json -p my-foundry-project
```

---

#### `agent list` — List agents in a project

Displays all agents configured in the specified Foundry project.

| Option | Alias | Description | Required |
|---|---|---|---|
| `--project-name` | `-p` | The name of the Foundry project | Yes |

**Example**

```bash
foundryfriend agent list --project-name my-foundry-project
```

---

#### `agent chat` — Chat with an agent

Starts an interactive conversation with an existing agent in a Foundry project.

| Option | Alias | Description | Required |
|---|---|---|---|
| `--agent-id` | `-id` | The ID of the agent to chat with | Yes |
| `--project-name` | `-p` | The name of the Foundry project | Yes |

**Example**

```bash
foundryfriend agent chat --agent-id asst_abc123 --project-name my-foundry-project
```

---

#### `agent delete` — Delete an agent

Deletes an agent from a Foundry project. Prompts for confirmation before proceeding.

| Option | Alias | Description | Required |
|---|---|---|---|
| `--agent-id` | `-id` | The ID of the agent to delete | Yes |
| `--project-name` | `-p` | The name of the Foundry project | Yes |

**Example**

```bash
foundryfriend agent delete --agent-id asst_abc123 --project-name my-foundry-project
```

---

## Authentication Modes

| Mode | Description | Prerequisites |
|---|---|---|
| `Identity` | Uses Azure identity (DefaultAzureCredential) | Run `az login` or `azd auth login` before use |
| `Key` | Uses an Foundry access key | Only supported for `chat`; not available for `agent` commands |

## License

This project is licensed under the terms in [LICENSE](LICENSE).