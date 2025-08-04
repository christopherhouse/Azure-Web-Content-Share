<template>
  <div class="admin-claim-view">
    <div class="container">
      <div class="claim-card">
        <div class="header">
          <h1 class="title">🚀 Welcome to {{ siteStore.siteName }}</h1>
          <p class="subtitle">
            This site hasn't been claimed yet. As an authenticated user, you can claim administrative access.
          </p>
        </div>

        <div class="content">
          <div class="info-section">
            <h2>✨ What does claiming the site do?</h2>
            <ul class="benefits-list">
              <li>🔐 You become the site administrator</li>
              <li>👥 You can manage other users and their roles</li>
              <li>📊 You get access to administrative features</li>
              <li>🛡️ You control site-wide settings and security</li>
            </ul>
          </div>

          <div class="user-info">
            <h3>👤 Your Account Information</h3>
            <div class="user-details">
              <p><strong>Name:</strong> {{ authStore.user?.name }}</p>
              <p><strong>Email:</strong> {{ authStore.user?.email }}</p>
              <p><strong>Current Role:</strong> {{ authStore.user?.role }}</p>
            </div>
          </div>

          <div class="action-section">
            <button 
              @click="claimSite"
              :disabled="isLoading"
              :class="['claim-button', { 'loading': isLoading }]"
            >
              <span v-if="!isLoading">🎯 Claim Site as Administrator</span>
              <span v-else>⏳ Claiming...</span>
            </button>

            <p class="warning">
              ⚠️ <strong>Note:</strong> Once you claim the site, other users will need your permission to become administrators.
            </p>
          </div>

          <div v-if="error" class="error-message">
            <h4>❌ Error</h4>
            <p>{{ error }}</p>
          </div>

          <div v-if="success" class="success-message">
            <h4>✅ Success!</h4>
            <p>{{ successMessage }}</p>
            <p>Redirecting to admin dashboard...</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSiteStore } from '@/stores/site'
import { siteService } from '@/services/siteService'
import { UserRole } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const siteStore = useSiteStore()

// State
const isLoading = ref(false)
const error = ref('')
const success = ref(false)
const successMessage = ref('')

// Methods
const claimSite = async () => {
  if (!authStore.isAuthenticated) {
    error.value = 'You must be logged in to claim the site'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    const authHeader = authStore.getAuthHeader()
    const response = await siteService.claimSite(authHeader)
    
    success.value = true
    successMessage.value = response.message
    
    // Update the user's role in the auth store
    if (authStore.user) {
      authStore.user.role = UserRole.Administrator
    }
    
    // Mark site as claimed in site store
    siteStore.markAsClaimed()
    
    // Redirect to home page after a short delay
    setTimeout(() => {
      router.push('/')
    }, 2000)
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to claim site'
  } finally {
    isLoading.value = false
  }
}

const checkSiteStatus = async () => {
  try {
    await siteStore.checkSiteStatus()
    
    // If site is already claimed, redirect to home
    if (siteStore.isClaimed) {
      router.push('/')
    }
  } catch (err) {
    console.error('Failed to check site status:', err)
  }
}

// Lifecycle
onMounted(async () => {
  // Redirect if not authenticated
  if (!authStore.isAuthenticated) {
    router.push('/')
    return
  }
  
  await checkSiteStatus()
})
</script>

<style scoped>
.admin-claim-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.container {
  width: 100%;
  max-width: 600px;
}

.claim-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  animation: slideUp 0.6s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.header {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  color: white;
  padding: 40px 30px;
  text-align: center;
}

.title {
  font-size: 2.5rem;
  font-weight: 700;
  margin: 0 0 10px 0;
}

.subtitle {
  font-size: 1.2rem;
  margin: 0;
  opacity: 0.9;
}

.content {
  padding: 40px 30px;
}

.info-section {
  margin-bottom: 30px;
}

.info-section h2 {
  color: #333;
  font-size: 1.5rem;
  margin-bottom: 15px;
}

.benefits-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.benefits-list li {
  padding: 8px 0;
  font-size: 1.1rem;
  color: #555;
}

.user-info {
  margin-bottom: 30px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 10px;
  border: 2px solid #e9ecef;
}

.user-info h3 {
  color: #333;
  margin: 0 0 15px 0;
  font-size: 1.3rem;
}

.user-details {
  margin: 0;
}

.user-details p {
  margin: 8px 0;
  font-size: 1rem;
  color: #555;
}

.action-section {
  text-align: center;
  margin-bottom: 20px;
}

.claim-button {
  background: linear-gradient(135deg, #ff6b6b, #ee5a24);
  color: white;
  border: none;
  padding: 16px 40px;
  font-size: 1.2rem;
  font-weight: 600;
  border-radius: 50px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(255, 107, 107, 0.3);
  margin-bottom: 20px;
}

.claim-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 107, 107, 0.4);
}

.claim-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.claim-button.loading {
  background: linear-gradient(135deg, #74b9ff, #0984e3);
}

.warning {
  font-size: 0.9rem;
  color: #666;
  font-style: italic;
  max-width: 400px;
  margin: 0 auto;
  line-height: 1.4;
}

.error-message,
.success-message {
  margin-top: 20px;
  padding: 20px;
  border-radius: 10px;
  text-align: left;
}

.error-message {
  background: #fee;
  border: 2px solid #f5c6cb;
  color: #721c24;
}

.success-message {
  background: #d4edda;
  border: 2px solid #c3e6cb;
  color: #155724;
}

.error-message h4,
.success-message h4 {
  margin: 0 0 10px 0;
  font-size: 1.1rem;
}

.error-message p,
.success-message p {
  margin: 5px 0;
}

@media (max-width: 768px) {
  .admin-claim-view {
    padding: 10px;
  }
  
  .title {
    font-size: 2rem;
  }
  
  .subtitle {
    font-size: 1rem;
  }
  
  .content {
    padding: 30px 20px;
  }
  
  .claim-button {
    padding: 14px 30px;
    font-size: 1.1rem;
  }
}
</style>