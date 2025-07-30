import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'

// Mock MSAL
const mockLoginPopup = vi.fn()
const mockLogoutPopup = vi.fn()
const mockAcquireTokenSilent = vi.fn()
const mockGetActiveAccount = vi.fn()

vi.mock('@azure/msal-browser', () => ({
  PublicClientApplication: vi.fn().mockImplementation(() => ({
    initialize: vi.fn().mockResolvedValue(undefined),
    getAllAccounts: vi.fn().mockReturnValue([]),
    setActiveAccount: vi.fn(),
    loginPopup: mockLoginPopup,
    logoutPopup: mockLogoutPopup,
    acquireTokenSilent: mockAcquireTokenSilent,
    getActiveAccount: mockGetActiveAccount
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
    
    // Reset mocks
    vi.clearAllMocks()
    
    // Set default mock implementations
    mockLoginPopup.mockResolvedValue({
      accessToken: 'mock-token',
      idTokenClaims: {
        oid: 'mock-user-id',
        email: 'test@example.com',
        name: 'Test User'
      }
    })
    
    mockLogoutPopup.mockResolvedValue(undefined)
    
    mockAcquireTokenSilent.mockResolvedValue({
      accessToken: 'mock-token',
      idTokenClaims: {
        oid: 'mock-user-id',
        email: 'test@example.com',
        name: 'Test User'
      }
    })
    
    mockGetActiveAccount.mockReturnValue({
      username: 'test@example.com'
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

  it('should handle popup blocked error with friendly message', async () => {
    const authStore = useAuthStore()
    mockLoginPopup.mockRejectedValue(new Error('popup_window_error'))
    
    await authStore.initializeMsal()
    
    await expect(authStore.login()).rejects.toThrow('Login was cancelled or popup was blocked')
  })

  it('should handle user cancelled error with friendly message', async () => {
    const authStore = useAuthStore()
    mockLoginPopup.mockRejectedValue(new Error('user_cancelled'))
    
    await authStore.initializeMsal()
    
    await expect(authStore.login()).rejects.toThrow('Login was cancelled or popup was blocked')
  })

  it('should handle network error with friendly message', async () => {
    const authStore = useAuthStore()
    mockLoginPopup.mockRejectedValue(new Error('network error occurred'))
    
    await authStore.initializeMsal()
    
    await expect(authStore.login()).rejects.toThrow('Network error during login')
  })

  it('should handle invalid client error with friendly message', async () => {
    const authStore = useAuthStore()
    mockLoginPopup.mockRejectedValue(new Error('invalid_client'))
    
    await authStore.initializeMsal()
    
    await expect(authStore.login()).rejects.toThrow('Authentication configuration error')
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