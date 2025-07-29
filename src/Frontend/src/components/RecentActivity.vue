<template>
  <div class="recent-activity">
    <v-data-table
      v-if="!loading && activities.length > 0"
      :headers="headers"
      :items="activities"
      :items-per-page="5"
      class="elevation-0"
      no-data-text="No recent activity"
    >
      <template v-slot:item.type="{ item }">
        <v-chip
          :color="getActivityColor(item.type)"
          size="small"
          variant="tonal"
        >
          {{ getActivityIcon(item.type) }} {{ item.type }}
        </v-chip>
      </template>

      <template v-slot:item.timestamp="{ item }">
        <span class="text-caption">{{ formatDate(item.timestamp) }}</span>
      </template>

      <template v-slot:item.actions="{ item }">
        <v-btn
          v-if="item.shareCode"
          icon="$link"
          size="small"
          variant="text"
          @click="copyShareCode(item.shareCode)"
        ></v-btn>
      </template>
    </v-data-table>

    <div v-else-if="loading" class="text-center pa-6">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-3 text-body-2">Loading recent activity...</p>
    </div>

    <v-card v-else variant="tonal" color="info" class="pa-6">
      <div class="text-center">
        <v-icon size="48" color="info" class="mb-3">$info</v-icon>
        <h4 class="text-h6 mb-2">No Recent Activity</h4>
        <p class="text-body-2">
          {{ authStore.isAuthenticated 
            ? 'Start sharing files to see your activity here!' 
            : 'Sign in to view your activity history.' 
          }}
        </p>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

interface Activity {
  id: string
  type: 'Upload' | 'Download' | 'Expired' | 'Deleted'
  fileName: string
  recipientEmail?: string
  timestamp: string
  shareCode?: string
}

const authStore = useAuthStore()
const loading = ref(true)
const activities = ref<Activity[]>([])

const headers = [
  { title: 'Activity', key: 'type', sortable: false },
  { title: 'File Name', key: 'fileName', sortable: false },
  { title: 'Recipient', key: 'recipientEmail', sortable: false },
  { title: 'Date', key: 'timestamp', sortable: true },
  { title: 'Actions', key: 'actions', sortable: false, align: 'end' as const },
]

const getActivityColor = (type: string) => {
  switch (type) {
    case 'Upload': return 'success'
    case 'Download': return 'info'
    case 'Expired': return 'warning'
    case 'Deleted': return 'error'
    default: return 'primary'
  }
}

const getActivityIcon = (type: string) => {
  switch (type) {
    case 'Upload': return '📤'
    case 'Download': return '📥'
    case 'Expired': return '⏰'
    case 'Deleted': return '🗑️'
    default: return '📄'
  }
}

const formatDate = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))
  
  if (diffInHours < 1) {
    return 'Just now'
  } else if (diffInHours < 24) {
    return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`
  } else if (diffInHours < 24 * 7) {
    const days = Math.floor(diffInHours / 24)
    return `${days} day${days > 1 ? 's' : ''} ago`
  } else {
    return date.toLocaleDateString()
  }
}

const copyShareCode = async (shareCode: string) => {
  try {
    await navigator.clipboard.writeText(shareCode)
    ;(window as Window & { showNotification?: (message: string, type?: 'success' | 'error' | 'warning' | 'info') => void }).showNotification?.('Share code copied to clipboard! 📋', 'success')
  } catch (error) {
    console.error('Failed to copy share code:', error)
    ;(window as Window & { showNotification?: (message: string, type?: 'success' | 'error' | 'warning' | 'info') => void }).showNotification?.('Failed to copy share code', 'error')
  }
}

const loadRecentActivity = async () => {
  if (!authStore.isAuthenticated) {
    loading.value = false
    return
  }

  try {
    // TODO: Replace with actual API call
    // const response = await axios.get('/api/files/my-activity', {
    //   headers: authStore.getAuthHeader()
    // })
    // activities.value = response.data

    // Mock data for demonstration
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    activities.value = [
      {
        id: '1',
        type: 'Upload',
        fileName: 'project-proposal.pdf',
        recipientEmail: 'colleague@company.com',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
        shareCode: 'ABC123DEF'
      },
      {
        id: '2',
        type: 'Download',
        fileName: 'budget-report.xlsx',
        recipientEmail: 'manager@company.com',
        timestamp: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
      },
      {
        id: '3',
        type: 'Expired',
        fileName: 'old-document.docx',
        recipientEmail: 'client@external.com',
        timestamp: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
      }
    ]
  } catch (error) {
    console.error('Failed to load recent activity:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadRecentActivity()
})
</script>

<style scoped>
.recent-activity {
  min-height: 200px;
}
</style>