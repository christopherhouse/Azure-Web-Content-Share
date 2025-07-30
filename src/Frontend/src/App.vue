<template>
  <v-app>
    <v-app-bar 
      color="primary"
      dark
      prominent
      elevate-on-scroll
    >
      <v-toolbar-title class="d-flex align-center">
        <v-icon class="mr-2" color="white">$cloud</v-icon>
        <span class="text-h5 font-weight-bold">Azure Web Content Share</span>
        <v-chip class="ml-3" size="small" color="accent" variant="outlined">
          🚀 Secure
        </v-chip>
      </v-toolbar-title>

      <v-spacer></v-spacer>

      <!-- User menu when authenticated -->
      <template v-if="authStore.isAuthenticated">
        <v-chip 
          class="mr-4" 
          :color="authStore.isAdministrator ? 'warning' : 'success'"
          variant="outlined"
          prepend-icon="$user"
        >
          {{ authStore.user?.role === 'Administrator' ? '👑 Admin' : '📤 Content Owner' }}
        </v-chip>
        
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn
              v-bind="props"
              variant="text"
              prepend-icon="$user"
            >
              {{ authStore.user?.name }}
              <v-icon>mdi-chevron-down</v-icon>
            </v-btn>
          </template>
          
          <v-list>
            <v-list-item 
              prepend-icon="$logout"
              title="Sign Out"
              @click="handleLogout"
            ></v-list-item>
          </v-list>
        </v-menu>
      </template>

      <!-- Login button when not authenticated -->
      <v-btn
        v-else
        variant="outlined"
        color="white"
        prepend-icon="$login"
        :loading="authStore.isLoading"
        @click="handleLogin"
      >
        Sign In with Microsoft
      </v-btn>
    </v-app-bar>

    <v-main>
      <RouterView />
    </v-main>

    <!-- Loading overlay -->
    <v-overlay 
      v-model="authStore.isLoading"
      class="align-center justify-center"
    >
      <v-progress-circular
        color="primary"
        indeterminate
        size="64"
      ></v-progress-circular>
    </v-overlay>

    <!-- Global snackbar for notifications -->
    <v-snackbar
      v-model="snackbar.show"
      :color="snackbar.color"
      :timeout="snackbar.timeout"
      location="top"
    >
      <v-icon class="mr-2">{{ snackbar.icon }}</v-icon>
      {{ snackbar.message }}
      
      <template v-slot:actions>
        <v-btn
          variant="text"
          @click="snackbar.show = false"
        >
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// Global notification system
const snackbar = reactive({
  show: false,
  message: '',
  color: 'info',
  icon: '$info',
  timeout: 4000,
})

const showNotification = (message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info') => {
  snackbar.message = message
  snackbar.color = type
  snackbar.icon = type === 'success' ? '$success' : 
                 type === 'error' ? '$error' : 
                 type === 'warning' ? '$warning' : '$info'
  snackbar.show = true
}

const handleLogin = async () => {
  try {
    await authStore.login()
    showNotification('Successfully signed in! 🎉', 'success')
  } catch (error) {
    console.error('Login failed:', error)
    
    // Show more specific error messages to users
    let errorMessage = 'Failed to sign in. Please try again.'
    if (error instanceof Error) {
      if (error.message.includes('popup was blocked')) {
        errorMessage = 'Login popup was blocked. Please allow popups for this site and try again.'
      } else if (error.message.includes('cancelled')) {
        errorMessage = 'Login was cancelled. Please try again when ready.'
      } else if (error.message.includes('network')) {
        errorMessage = 'Network connection error. Please check your internet connection.'
      } else if (error.message.includes('configuration')) {
        errorMessage = 'Authentication is not properly configured. Please contact support.'
      }
    }
    
    showNotification(errorMessage, 'error')
  }
}

const handleLogout = async () => {
  try {
    await authStore.logout()
    showNotification('Successfully signed out! 👋', 'info')
  } catch (error) {
    console.error('Logout failed:', error)
    showNotification('Error during sign out', 'error')
  }
}

// Make showNotification available globally
// In a real app, you might want to use provide/inject or a composable
;(window as Window & { showNotification?: (message: string, type?: 'success' | 'error' | 'warning' | 'info') => void }).showNotification = showNotification
</script>

<style scoped>
/* Custom styles for the app bar */
.v-toolbar-title {
  white-space: nowrap;
}

/* Responsive adjustments */
@media (max-width: 960px) {
  .v-toolbar-title span {
    font-size: 1.1rem !important;
  }
  
  .v-chip {
    font-size: 0.75rem;
  }
}
</style>
