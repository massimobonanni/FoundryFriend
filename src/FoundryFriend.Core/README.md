# FoundryFriend.Core

Core library for the FoundryFriend application. This project contains shared contracts, entities, configuration management, and security utilities that are referenced by the CLI project. It has **no dependency** on Azure AI SDKs or System.CommandLine — it defines the abstractions that the CLI layer implements.

## Architecture

```
FoundryFriend.Core/
├── Interfaces/                 # Public service contracts
│   ├── ISessionManager.cs      # Session settings persistence and secret management
│   ├── IChatService.cs         # Model deployment chat operations (streaming)
│   ├── IAgentChatService.cs    # Agent conversation operations (streaming)
│   ├── IAgentService.cs        # Agent CRUD operations (create, list, delete)
│   └── IAzCliService.cs        # Runs Azure CLI commands as external processes
├── Entities/                   # Shared data types and enums
│   ├── AuthenticationMode.cs   # Identity vs Key authentication enum
│   ├── AgentInfo.cs            # AgentInfo and AgentVersionInfo DTOs
│   └── StreamingResponseChunk.cs  # A single chunk from a streaming chat/agent response
├── Configuration/              # Settings and configuration models
│   ├── SessionManager.cs       # ISessionManager implementation — file-based settings with encrypted secrets
│   └── SessionSettings.cs      # Session settings model (endpoint, auth mode, access key, custom settings, secrets)
├── Security/                   # Platform-specific secret protection
│   ├── IProtectedDataProvider.cs           # Abstraction for data encryption/decryption
│   ├── WindowsProtectedDataProvider.cs     # Windows DPAPI implementation
│   ├── LinuxProtectedDataProvider.cs       # Linux implementation
│   └── ProtectedDataProviderFactory.cs     # Factory that selects the correct provider
└── Extensions/
    └── StringExtensions.cs     # General-purpose string helpers
```

## Key Components

### Interfaces

The interfaces define the service contracts that the CLI project implements. This separation allows the Core project to remain SDK-free while the CLI project handles the actual Azure SDK calls.

| Interface | Purpose |
|-----------|---------|
| `ISessionManager` | Load, save, and clear session settings (endpoint, auth mode, access key). Manages secrets via platform-specific encryption. |
| `IChatService` | Initialize a chat client for a model deployment, send messages with streaming responses, reset conversation history. |
| `IAgentChatService` | Initialize a conversation with a Foundry agent, send messages with streaming responses, reset state. |
| `IAgentService` | Create, list, and delete agents in a Foundry project. |
| `IAzCliService` | Run `az` CLI commands as an external process and return the exit code. |

### Entities

Simple records and enums shared across projects:

- **`AuthenticationMode`** — Enum with `Identity` (DefaultAzureCredential) and `Key` (access key) values.
- **`AgentInfo`** — Lightweight record (`Id`, `Name`) returned by list operations.
- **`AgentVersionInfo`** — Record (`Id`, `Name`, `Version`) returned by agent creation.
- **`StreamingResponseChunk`** — Record (`Text`, `UpdateType`) representing a single chunk from a streaming agent response.

### Configuration

- **`SessionManager`** — Persists settings to a local JSON file and stores secrets using platform-specific encryption (Windows DPAPI or Linux equivalent). Implements `ISessionManager`.
- **`SessionSettings`** — POCO holding the current session's authentication mode, endpoint, access key, custom settings, and secrets (secrets are excluded from JSON serialization).

> **Note:** `AgentConfiguration.cs` uses the `FoundryFriend.Core.Configuration` namespace but is physically located in the `FoundryFriend.CLI` project (`Commands/Agent/AgentConfiguration.cs`), since it is only used by the agent-related CLI commands.

### Security

The `Security/` folder provides cross-platform secret protection:

- **`IProtectedDataProvider`** — Interface for encrypting and decrypting byte arrays.
- **`WindowsProtectedDataProvider`** — Uses `System.Security.Cryptography.ProtectedData` (DPAPI).
- **`LinuxProtectedDataProvider`** — Provides an alternative for Linux environments.
- **`ProtectedDataProviderFactory`** — Selects the appropriate provider based on the current OS.

## Dependencies

| Package | Purpose |
|---------|---------|
| `System.Security.Cryptography.ProtectedData` 10.0.7 | Windows DPAPI for secret encryption |

This project intentionally has minimal dependencies to keep it lightweight and reusable.

## Usage

This project is not run directly — it is referenced by `FoundryFriend.CLI`. Service interfaces defined here are implemented in the CLI project's `Services/` folder and registered in `Program.cs` via dependency injection.
