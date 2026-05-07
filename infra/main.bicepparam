using './main.bicep'

param environmentName = readEnvironmentVariable('AZURE_ENV_NAME', 'foundryfriend')
param location = readEnvironmentVariable('AZURE_LOCATION', 'swedencentral')
