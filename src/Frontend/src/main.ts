import { createApp } from 'vue'
import { createPinia } from 'pinia'
import vuetify from './plugins/vuetify'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'
import { appInsights } from './services/applicationInsights'
import { runtimeConfig, validateConfiguration } from './services/runtimeConfig'

// Validate configuration before starting the app
try {
  validateConfiguration()
} catch (error) {
  console.error('Configuration validation failed:', error)
  // In production, you might want to show a user-friendly error page
}

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(vuetify)

// Initialize Application Insights
const connectionString = runtimeConfig.applicationInsights.connectionString
if (connectionString) {
  appInsights.initialize(connectionString)
}

// Initialize authentication
const authStore = useAuthStore()
authStore.initializeMsal()

app.mount('#app')
