/**
 * Runtime configuration utility
 * Provides access to configuration values that are injected at runtime
 * instead of build time, enabling environment-specific deployments.
 */

declare global {
  interface Window {
    __RUNTIME_CONFIG__?: {
      VITE_AZURE_CLIENT_ID?: string
      VITE_AZURE_TENANT_ID?: string
      VITE_API_BASE_URL?: string
      VITE_API_CLIENT_ID?: string
      VITE_APPLICATIONINSIGHTS_CONNECTION_STRING?: string
    }
  }
}

/**
 * Gets a configuration value from runtime config or falls back to build-time environment variables
 * @param key Configuration key
 * @param defaultValue Default value if not found
 * @returns Configuration value
 */
function getConfig(key: string, defaultValue: string = ''): string {
  // First try runtime config (injected at container startup)
  if (window.__RUNTIME_CONFIG__?.[key as keyof typeof window.__RUNTIME_CONFIG__]) {
    return window.__RUNTIME_CONFIG__[key as keyof typeof window.__RUNTIME_CONFIG__] as string
  }
  
  // Fall back to build-time environment variables (for local development)
  return import.meta.env[key] || defaultValue
}

/**
 * Runtime configuration object
 */
export const runtimeConfig = {
  azure: {
    clientId: getConfig('VITE_AZURE_CLIENT_ID'),
    tenantId: getConfig('VITE_AZURE_TENANT_ID'),
  },
  api: {
    baseUrl: getConfig('VITE_API_BASE_URL'),
    clientId: getConfig('VITE_API_CLIENT_ID'),
  },
  applicationInsights: {
    connectionString: getConfig('VITE_APPLICATIONINSIGHTS_CONNECTION_STRING'),
  },
}

/**
 * Validates that all required configuration values are present
 * @throws Error if required configuration is missing
 */
export function validateConfiguration(): void {
  const required = [
    { key: 'VITE_AZURE_CLIENT_ID', value: runtimeConfig.azure.clientId },
    { key: 'VITE_AZURE_TENANT_ID', value: runtimeConfig.azure.tenantId },
    { key: 'VITE_API_CLIENT_ID', value: runtimeConfig.api.clientId },
  ]

  const missing = required.filter(({ value }) => !value)
  
  if (missing.length > 0) {
    const missingKeys = missing.map(({ key }) => key).join(', ')
    throw new Error(`Missing required configuration: ${missingKeys}`)
  }
}