import { runtimeConfig } from './runtimeConfig'

export interface SiteStatusResponse {
  isClaimed: boolean
  siteName: string
}

export interface ClaimSiteResponse {
  success: boolean
  message: string
  claimedAt: string
}

/**
 * Site management API service
 */
export class SiteService {
  private readonly baseUrl: string

  constructor() {
    this.baseUrl = runtimeConfig.api.baseUrl || '/api'
  }

  /**
   * Get site status (anonymous access)
   */
  async getSiteStatus(): Promise<SiteStatusResponse> {
    const response = await fetch(`${this.baseUrl}/site/status`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error(`Failed to get site status: ${response.status} ${response.statusText}`)
    }

    return response.json()
  }

  /**
   * Claim the site (requires authentication)
   */
  async claimSite(authHeader: Record<string, string>): Promise<ClaimSiteResponse> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...authHeader,
    }

    const response = await fetch(`${this.baseUrl}/site/claim`, {
      method: 'POST',
      headers,
    })

    if (!response.ok) {
      if (response.status === 409) {
        throw new Error('Site has already been claimed by another administrator')
      } else if (response.status === 401) {
        throw new Error('Authentication required to claim the site')
      } else if (response.status === 400) {
        const error = await response.json()
        throw new Error(error.detail || 'Invalid request')
      }
      throw new Error(`Failed to claim site: ${response.status} ${response.statusText}`)
    }

    return response.json()
  }
}

// Export singleton instance
export const siteService = new SiteService()