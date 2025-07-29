import { createApp } from 'vue'
import { createPinia } from 'pinia'
import vuetify from './plugins/vuetify'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'
import { appInsights } from './services/applicationInsights'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(vuetify)

// Initialize Application Insights
const connectionString = import.meta.env.VITE_APPLICATIONINSIGHTS_CONNECTION_STRING
if (connectionString) {
  appInsights.initialize(connectionString)
}

// Initialize authentication
const authStore = useAuthStore()
authStore.initializeMsal()

app.mount('#app')
