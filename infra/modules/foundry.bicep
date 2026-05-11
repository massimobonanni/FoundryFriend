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

@description('Resource ID di Application Insights per la telemetria del progetto Foundry.')
param applicationInsightsId string

@description('Resource ID del Log Analytics Workspace per i diagnostic settings.')
param logAnalyticsWorkspaceId string

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
    #disable-next-line BCP037
    applicationInsights: applicationInsightsId
  }
}

resource foundryDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'foundry-diagnostics'
  scope: foundry
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
      }
      {
        categoryGroup: 'audit'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
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
