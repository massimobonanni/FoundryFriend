# FoundryFriend

FoundryFriend is a project built on Azure AI Foundry, deployable using the [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/).

## Project Structure

```
FoundryFriend/
├── src/          # Application source code
├── infra/        # Infrastructure as Code (Bicep)
└── azure.yaml    # Azure Developer CLI project configuration
```

## Getting Started

### Prerequisites

- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- An active Azure subscription

### Deploy with AZD

```bash
azd up
```