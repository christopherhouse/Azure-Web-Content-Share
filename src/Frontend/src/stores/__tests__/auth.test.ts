import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'

// Mock MSAL
vi.mock('@azure/msal-browser', () => ({
  PublicClientApplication: vi.fn().mockImplementation(() => ({
    initialize: vi.fn().mockResolvedValue(undefined),
    getAllAccounts: vi.fn().mockReturnValue([]),
    setActiveAccount: vi.fn(),
    loginPopup: vi.fn().mockResolvedValue({
      accessToken: 'mock-token',
      idTokenClaims: {
        oid: 'mock-user-id',
        email: 'test@example.com',
        name: 'Test User'
      }
    }),
    logoutPopup: vi.fn().mockResolvedValue(undefined),
    acquireTokenSilent: vi.fn().mockResolvedValue({
      accessToken: 'mock-token',
      idTokenClaims: {
        oid: 'mock-user-id',
        email: 'test@example.com',
        name: 'Test User'
      }
    }),
    getActiveAccount: vi.fn().mockReturnValue({
      username: 'test@example.com'
    })
  }))
}))

describe('Auth Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    // Mock window.location.origin for redirect URI
    vi.stubGlobal('window', {
      location: {
        origin: 'http://localhost:3000'
      }
    })
  })

  it('should initialize MSAL correctly with redirect URI', async () => {
    const authStore = useAuthStore()
    
    await authStore.initializeMsal()
    
    expect(authStore.isAuthenticated).toBe(false)
  })

  it('should handle login correctly', async () => {
    const authStore = useAuthStore()
    
    await authStore.initializeMsal()
    await authStore.login()
    
    expect(authStore.isAuthenticated).toBe(true)
    expect(authStore.user?.email).toBe('test@example.com')
    expect(authStore.accessToken).toBe('mock-token')
  })

  it('should handle logout correctly', async () => {
    const authStore = useAuthStore()
    
    await authStore.initializeMsal()
    await authStore.login()
    
    expect(authStore.isAuthenticated).toBe(true)
    
    await authStore.logout()
    
    expect(authStore.isAuthenticated).toBe(false)
    expect(authStore.user).toBeNull()
    expect(authStore.accessToken).toBeNull()
  })

  it('should generate correct auth header', async () => {
    const authStore = useAuthStore()
    
    await authStore.initializeMsal()
    await authStore.login()
    
    const authHeader = authStore.getAuthHeader()
    
    expect(authHeader).toEqual({
      Authorization: 'Bearer mock-token'
    })
  })

  it('should return empty auth header when not authenticated', () => {
    const authStore = useAuthStore()
    
    const authHeader = authStore.getAuthHeader()
    
    expect(authHeader).toEqual({})
  })
})