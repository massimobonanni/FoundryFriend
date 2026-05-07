targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Nome dell\'ambiente AZD; usato come suffisso nei nomi delle risorse.')
param environmentName string

@minLength(1)
@description('Location primaria per le risorse.')
param location string

@description('Nome del progetto Foundry.')
param foundryProjectName string = 'proj-${environmentName}'

@description('Display name del progetto Foundry.')
param foundryProjectDisplayName string = 'Foundry Friend Project'

@description('Nome del deployment del modello GPT-4.1.')
param gpt41DeploymentName string = 'gpt-4.1'

@description('Versione del modello GPT-4.1.')
param gpt41ModelVersion string = '2025-04-14'

@description('Capacity (TPM in migliaia) per il deployment GPT-4.1.')
param gpt41Capacity int = 10

@description('SKU per il deployment GPT-4.1.')
param gpt41SkuName string = 'GlobalStandard'

@description('Tag applicati a tutte le risorse.')
param tags object = {
  'azd-env-name': environmentName
}

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var resourceGroupName = 'rg-${environmentName}'
var foundryAccountName = 'aif-${resourceToken}'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

module foundry 'modules/foundry.bicep' = {
  scope: rg
  params: {
    location: location
    tags: tags
    foundryAccountName: foundryAccountName
    foundryProjectName: foundryProjectName
    foundryProjectDisplayName: foundryProjectDisplayName
    gpt41DeploymentName: gpt41DeploymentName
    gpt41ModelVersion: gpt41ModelVersion
    gpt41Capacity: gpt41Capacity
    gpt41SkuName: gpt41SkuName
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_AI_FOUNDRY_NAME string = foundry.outputs.foundryAccountName
output AZURE_AI_FOUNDRY_ENDPOINT string = foundry.outputs.foundryEndpoint
output AZURE_AI_FOUNDRY_PROJECT_NAME string = foundry.outputs.projectName
output AZURE_AI_FOUNDRY_PROJECT_ENDPOINT string = foundry.outputs.projectEndpoint
output AZURE_AI_FOUNDRY_GPT41_DEPLOYMENT string = foundry.outputs.gpt41DeploymentName
