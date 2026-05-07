@description('Location delle risorse.')
param location string

@description('Tag applicati alle risorse.')
param tags object

@description('Nome dell\'account Azure AI Foundry.')
param foundryAccountName string

@description('Nome del progetto Foundry.')
param foundryProjectName string

@description('Display name del progetto Foundry.')
param foundryProjectDisplayName string

@description('Nome del deployment GPT-4.1.')
param gpt41DeploymentName string

@description('Versione del modello GPT-4.1.')
param gpt41ModelVersion string

@description('Capacity del deployment GPT-4.1.')
param gpt41Capacity int

@description('SKU del deployment GPT-4.1.')
param gpt41SkuName string

resource foundry 'Microsoft.CognitiveServices/accounts@2025-06-01' = {
  name: foundryAccountName
  location: location
  tags: tags
  kind: 'AIServices'
  sku: {
    name: 'S0'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: foundryAccountName
    allowProjectManagement: true
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

resource project 'Microsoft.CognitiveServices/accounts/projects@2025-06-01' = {
  parent: foundry
  name: foundryProjectName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    displayName: foundryProjectDisplayName
    description: 'Foundry Friend project'
  }
}

resource gpt41 'Microsoft.CognitiveServices/accounts/deployments@2025-06-01' = {
  parent: foundry
  name: gpt41DeploymentName
  sku: {
    name: gpt41SkuName
    capacity: gpt41Capacity
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4.1'
      version: gpt41ModelVersion
    }
    raiPolicyName: 'Microsoft.DefaultV2'
  }
}

output foundryAccountName string = foundry.name
output foundryEndpoint string = foundry.properties.endpoint
output projectName string = project.name
output projectEndpoint string = project.properties.endpoints['AI Foundry API']
output gpt41DeploymentName string = gpt41.name
