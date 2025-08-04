import { defineStore } from 'pinia'
import { ref } from 'vue'
import { siteService } from '@/services/siteService'
import type { SiteStatusResponse } from '@/types'

export const useSiteStore = defineStore('site', () => {
  // State
  const isClaimed = ref<boolean | null>(null) // null = unknown, true = claimed, false = not claimed
  const siteName = ref<string>('Azure Web Content Share')
  const isLoading = ref(false)

  // Actions
  const checkSiteStatus = async (): Promise<SiteStatusResponse> => {
    isLoading.value = true
    try {
      const status = await siteService.getSiteStatus()
      isClaimed.value = status.isClaimed
      siteName.value = status.siteName
      return status
    } catch (error) {
      console.error('Failed to check site status:', error)
      // Default to claimed = true to avoid showing claim view on error
      isClaimed.value = true
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const markAsClaimed = () => {
    isClaimed.value = true
  }

  const reset = () => {
    isClaimed.value = null
    siteName.value = 'Azure Web Content Share'
    isLoading.value = false
  }

  return {
    // State
    isClaimed,
    siteName,
    isLoading,

    // Actions
    checkSiteStatus,
    markAsClaimed,
    reset,
  }
})