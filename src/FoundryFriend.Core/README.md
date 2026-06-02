# FoundryFriend.Core

Core library for the FoundryFriend application. This project contains shared contracts, entities, configuration management, and security utilities that are referenced by the CLI project. It has **no dependency** on Azure AI SDKs or System.CommandLine — it defines the abstractions that the CLI layer implements.

## Architecture

```
FoundryFriend.Core/
├── Interfaces/                 # Public service contracts
│   ├── ISessionManager.cs      # Session settings persistence and secret management
│   ├── IChatService.cs         # Model deployment chat operations (streaming)
│   ├── IAgentChatService.cs    # Agent conversation operations (streaming)
│   └── IAgentService.cs        # Agent CRUD operations (create, list, delete)
├── Entities/                   # Shared data types and enums
│   ├── AuthenticationMode.cs   # Identity vs Key authentication enum
│   └── AgentInfo.cs            # AgentInfo and AgentVersionInfo DTOs
├── Configuration/              # Settings and configuration models
│   ├── SessionManager.cs       # ISessionManager implementation — file-based settings with encrypted secrets
│   └── AgentConfiguration.cs   # JSON-serializable agent definition (loaded from config files)
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

### Entities

Simple records and enums shared across projects:

- **`AuthenticationMode`** — Enum with `Identity` (DefaultAzureCredential) and `Key` (access key) values.
- **`AgentInfo`** — Lightweight record (`Id`, `Name`) returned by list operations.
- **`AgentVersionInfo`** — Record (`Id`, `Name`, `Version`) returned by agent creation.

### Configuration

- **`SessionManager`** — Persists settings to a local JSON file and stores secrets using platform-specific encryption (Windows DPAPI or Linux equivalent). Implements `ISessionManager`.
- **`AgentConfiguration`** — Deserializes agent definition JSON files. Supports instructions as either a single string or an array of strings.

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
