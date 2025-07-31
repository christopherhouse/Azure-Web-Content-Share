import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { PublicClientApplication, AuthenticationResult, SilentRequest } from '@azure/msal-browser'
import type { User, UserRole } from '@/types'
import { runtimeConfig } from '@/services/runtimeConfig'

// MSAL configuration using runtime config
const msalConfig = {
  auth: {
    clientId: runtimeConfig.azure.clientId || 'your-client-id',
    authority: `https://login.microsoftonline.com/${runtimeConfig.azure.tenantId || 'your-tenant-id'}`,
    redirectUri: `${window.location.origin}/auth-redirect.html`,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
}

const loginRequest = {
  scopes: ['openid', 'profile', 'email', `api://${runtimeConfig.api.clientId || 'api-client-id'}/access_as_user`],
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const accessToken = ref<string | null>(null)
  const msalInstance = ref<PublicClientApplication | null>(null)

  // Computed
  const isAuthenticated = computed(() => !!user.value && !!accessToken.value)
  const isAdministrator = computed(() => user.value?.role === 'Administrator')
  const isContentOwner = computed(() => user.value?.role === 'ContentOwner' || isAdministrator.value)

  // Actions
  const initializeMsal = async () => {
    try {
      msalInstance.value = new PublicClientApplication(msalConfig)
      await msalInstance.value.initialize()
      
      // Check if user is already logged in
      const accounts = msalInstance.value.getAllAccounts()
      if (accounts.length > 0) {
        msalInstance.value.setActiveAccount(accounts[0])
        await acquireTokenSilent()
      }
    } catch (error) {
      console.error('Failed to initialize MSAL:', error)
    }
  }

  const login = async () => {
    if (!msalInstance.value) {
      throw new Error('MSAL not initialized')
    }

    isLoading.value = true
    try {
      const response = await msalInstance.value.loginPopup(loginRequest)
      await handleAuthenticationResult(response)
    } catch (error) {
      console.error('Login failed:', error)
      
      // Provide more specific error messages based on the error type
      if (error instanceof Error) {
        if (error.message.includes('popup_window_error') || error.message.includes('user_cancelled')) {
          throw new Error('Login was cancelled or popup was blocked. Please ensure popups are allowed and try again.')
        } else if (error.message.includes('network')) {
          throw new Error('Network error during login. Please check your connection and try again.')
        } else if (error.message.includes('invalid_client')) {
          throw new Error('Authentication configuration error. Please contact support.')
        } else if (error.message.includes('AADSTS650053')) {
          // Handle the specific scope error from the issue
          throw new Error(
            'API scope configuration error. The API app registration is missing the required "access_as_user" scope. ' +
            'Please check the Azure AD app registration configuration or contact your administrator. ' +
            'See docs/ENTRA_ID_TROUBLESHOOTING.md for detailed instructions.'
          )
        } else if (error.message.includes('scope') && error.message.includes('does not exist')) {
          throw new Error(
            'Scope configuration error. The requested API scope is not properly configured in Azure AD. ' +
            'Please verify the API app registration exposes the correct scopes. ' +
            'See docs/ENTRA_ID_TROUBLESHOOTING.md for help.'
          )
        }
      }
      
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    if (!msalInstance.value) return

    try {
      await msalInstance.value.logoutPopup()
      user.value = null
      accessToken.value = null
    } catch (error) {
      console.error('Logout failed:', error)
      // Even if logout fails, clear local state
      user.value = null
      accessToken.value = null
    }
  }

  const acquireTokenSilent = async () => {
    if (!msalInstance.value) return

    const activeAccount = msalInstance.value.getActiveAccount()
    if (!activeAccount) return

    try {
      const silentRequest: SilentRequest = {
        ...loginRequest,
        account: activeAccount,
      }
      
      const response = await msalInstance.value.acquireTokenSilent(silentRequest)
      await handleAuthenticationResult(response)
    } catch (error) {
      console.error('Silent token acquisition failed:', error)
      // If silent acquisition fails, the user needs to login again
      user.value = null
      accessToken.value = null
    }
  }

  const handleAuthenticationResult = async (result: AuthenticationResult) => {
    accessToken.value = result.accessToken
    
    // Extract user information from ID token claims
    interface IdTokenClaims {
      oid?: string
      sub?: string
      email?: string
      preferred_username?: string
      name?: string
    }
    
    const claims = result.idTokenClaims as IdTokenClaims
    if (claims) {
      // For now, we'll create a basic user object
      // In a real implementation, you'd fetch additional user details from your API
      user.value = {
        id: claims.oid || claims.sub || 'unknown',
        email: claims.email || claims.preferred_username || 'unknown@unknown.com',
        name: claims.name || claims.preferred_username || 'Unknown User',
        role: 'ContentOwner' as UserRole, // Default role - should be fetched from your API
        createdAt: new Date().toISOString(),
      }

      // TODO: Fetch user role from your API using the access token
      await fetchUserProfile()
    }
  }

  const fetchUserProfile = async () => {
    if (!accessToken.value) return

    try {
      // This would call your API to get the user's role and other details
      // For now, we'll simulate this
      console.log('TODO: Fetch user profile from API')
      
      // Example API call:
      // const response = await fetch('/api/users/me', {
      //   headers: {
      //     'Authorization': `Bearer ${accessToken.value}`
      //   }
      // })
      // const userProfile = await response.json()
      // user.value = { ...user.value, ...userProfile }
    } catch (error) {
      console.error('Failed to fetch user profile:', error)
    }
  }

  const getAuthHeader = () => {
    return accessToken.value ? { Authorization: `Bearer ${accessToken.value}` } : {}
  }

  const getConfigurationInfo = () => {
    return {
      clientId: runtimeConfig.azure.clientId || 'not-configured',
      tenantId: runtimeConfig.azure.tenantId || 'not-configured',
      apiClientId: runtimeConfig.api.clientId || 'not-configured',
      authority: msalConfig.auth.authority,
      redirectUri: msalConfig.auth.redirectUri,
      requestedScopes: loginRequest.scopes,
      expectedApiScope: `api://${runtimeConfig.api.clientId || 'api-client-id'}/access_as_user`
    }
  }

  const validateConfiguration = () => {
    const issues = []
    
    if (!runtimeConfig.azure.clientId || runtimeConfig.azure.clientId === 'your-client-id') {
      issues.push('VITE_AZURE_CLIENT_ID is not configured')
    }
    
    if (!runtimeConfig.azure.tenantId || runtimeConfig.azure.tenantId === 'your-tenant-id') {
      issues.push('VITE_AZURE_TENANT_ID is not configured')
    }
    
    if (!runtimeConfig.api.clientId || runtimeConfig.api.clientId === 'api-client-id') {
      issues.push('VITE_API_CLIENT_ID is not configured')
    }
    
    return {
      isValid: issues.length === 0,
      issues,
      config: getConfigurationInfo()
    }
  }

  return {
    // State
    user,
    isLoading,
    accessToken,
    
    // Computed
    isAuthenticated,
    isAdministrator,
    isContentOwner,
    
    // Actions
    initializeMsal,
    login,
    logout,
    acquireTokenSilent,
    getAuthHeader,
    
    // Diagnostic utilities
    getConfigurationInfo,
    validateConfiguration,
  }
})