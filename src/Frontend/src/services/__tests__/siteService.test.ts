import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { SiteService } from '../siteService'

// Mock the runtimeConfig module
vi.mock('../runtimeConfig', () => ({
  runtimeConfig: {
    api: {
      baseUrl: 'http://localhost:8080'
    }
  }
}))

describe('SiteService', () => {
  let siteService: SiteService
  let fetchMock: ReturnType<typeof vi.fn>

  beforeEach(() => {
    // Create a fresh instance for each test
    siteService = new SiteService()
    
    // Mock global fetch
    fetchMock = vi.fn()
    global.fetch = fetchMock
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  describe('URL Construction', () => {
    it('should construct correct URL for getSiteStatus', async () => {
      // Arrange
      const mockResponse = {
        ok: true,
        json: vi.fn().mockResolvedValue({ isClaimed: false, siteName: 'Test Site' })
      }
      fetchMock.mockResolvedValue(mockResponse)

      // Act
      await siteService.getSiteStatus()

      // Assert
      expect(fetchMock).toHaveBeenCalledWith(
        'http://localhost:8080/api/site/status',
        expect.objectContaining({
          method: 'GET',
          headers: {
            'Content-Type': 'application/json'
          }
        })
      )
    })

    it('should construct correct URL for claimSite', async () => {
      // Arrange
      const mockResponse = {
        ok: true,
        json: vi.fn().mockResolvedValue({ 
          success: true, 
          message: 'Site claimed', 
          claimedAt: new Date().toISOString() 
        })
      }
      fetchMock.mockResolvedValue(mockResponse)
      const authHeader = { 'Authorization': 'Bearer test-token' }

      // Act
      await siteService.claimSite(authHeader)

      // Assert
      expect(fetchMock).toHaveBeenCalledWith(
        'http://localhost:8080/api/site/claim',
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            'Authorization': 'Bearer test-token'
          })
        })
      )
    })
  })

  describe('Error Handling', () => {
    it('should handle 404 error gracefully for getSiteStatus', async () => {
      // Arrange
      const mockResponse = {
        ok: false,
        status: 404,
        statusText: 'Not Found'
      }
      fetchMock.mockResolvedValue(mockResponse)

      // Act & Assert
      await expect(siteService.getSiteStatus()).rejects.toThrow('Failed to get site status: 404 Not Found')
    })

    it('should handle 409 conflict error for claimSite', async () => {
      // Arrange
      const mockResponse = {
        ok: false,
        status: 409,
        statusText: 'Conflict'
      }
      fetchMock.mockResolvedValue(mockResponse)
      const authHeader = { 'Authorization': 'Bearer test-token' }

      // Act & Assert
      await expect(siteService.claimSite(authHeader)).rejects.toThrow('Site has already been claimed by another administrator')
    })
  })
})