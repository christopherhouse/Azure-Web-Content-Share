<template>
  <v-container fluid class="home-container">
    <!-- Hero Section -->
    <v-row class="hero-section" no-gutters>
      <v-col cols="12">
        <div class="hero-content text-center">
          <h1 class="display-1 font-weight-bold mb-4 text-white">
            🌐 Secure File Sharing
          </h1>
          <p class="headline text-white mb-6 opacity-90">
            Share files securely with time-limited access and enterprise-grade encryption
          </p>
          
          <!-- Action buttons for unauthenticated users -->
          <div v-if="!authStore.isAuthenticated" class="hero-actions">
            <v-btn
              size="large"
              color="white"
              variant="elevated"
              class="mr-4 mb-2"
              prepend-icon="$login"
              :loading="authStore.isLoading"
              @click="handleLogin"
            >
              Sign In with Microsoft
            </v-btn>
            <v-btn
              size="large"
              variant="outlined"
              color="white"
              class="mb-2"
              prepend-icon="$download"
              @click="scrollToFileAccess"
            >
              Access Shared File
            </v-btn>
          </div>
        </div>
      </v-col>
    </v-row>

    <!-- Main Content Area -->
    <v-row class="main-content" justify="center">
      <v-col cols="12" lg="10" xl="8">
        
        <!-- Unauthenticated User Section -->
        <template v-if="!authStore.isAuthenticated">
          <v-card 
            ref="fileAccessCard"
            class="mb-6"
            elevation="8"
            rounded="xl"
          >
            <v-card-title class="text-center pa-6">
              <v-icon class="mr-3" color="primary" size="large">$download</v-icon>
              <span class="text-h4">📥 Access Shared File</span>
            </v-card-title>
            
            <v-card-text class="pa-6">
              <FileAccessForm />
            </v-card-text>
          </v-card>

          <!-- Features showcase for unauthenticated users -->
          <v-row class="features-section">
            <v-col cols="12" md="4" v-for="feature in features" :key="feature.title">
              <v-card class="feature-card h-100" elevation="4" rounded="lg">
                <v-card-text class="text-center pa-6">
                  <div class="feature-icon mb-4">{{ feature.icon }}</div>
                  <h3 class="text-h6 mb-3">{{ feature.title }}</h3>
                  <p class="text-body-1">{{ feature.description }}</p>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
        </template>

        <!-- Authenticated User Dashboard -->
        <template v-else>
          <div class="dashboard-header mb-6">
            <h2 class="text-h3 font-weight-bold mb-2">
              {{ getWelcomeMessage() }}
            </h2>
            <p class="text-h6 text-medium-emphasis">
              {{ getDashboardSubtitle() }}
            </p>
          </div>

          <!-- Administrator Dashboard -->
          <template v-if="authStore.isAdministrator">
            <v-row>
              <!-- User Management -->
              <v-col cols="12" lg="6">
                <DashboardCard
                  title="👥 User Management"
                  subtitle="Manage users and roles"
                  icon="$users"
                  color="primary"
                  @click="navigateTo('/admin/users')"
                >
                  <template #actions>
                    <v-btn variant="text" color="primary" prepend-icon="$user">
                      Add User
                    </v-btn>
                  </template>
                </DashboardCard>
              </v-col>

              <!-- Content Management -->
              <v-col cols="12" lg="6">
                <DashboardCard
                  title="📁 All Content"
                  subtitle="Manage all shared content"
                  icon="$folder"
                  color="warning"
                  @click="navigateTo('/admin/content')"
                >
                  <template #actions>
                    <v-btn variant="text" color="warning" prepend-icon="$security">
                      View All
                    </v-btn>
                  </template>
                </DashboardCard>
              </v-col>
            </v-row>
          </template>

          <!-- Content Owner & Administrator File Operations -->
          <v-row class="mt-4">
            <!-- Upload Files -->
            <v-col cols="12" lg="6">
              <DashboardCard
                title="📤 Share Files"
                subtitle="Upload and share files securely"
                icon="$upload"
                color="success"
                @click="showUploadDialog = true"
              >
                <template #actions>
                  <v-btn variant="text" color="success" prepend-icon="$upload">
                    Upload File
                  </v-btn>
                </template>
              </DashboardCard>
            </v-col>

            <!-- Manage My Files -->
            <v-col cols="12" lg="6">
              <DashboardCard
                title="📋 My Files"
                subtitle="Manage your shared files"
                icon="$file"
                color="info"
                @click="navigateTo('/files')"
              >
                <template #actions>
                  <v-btn variant="text" color="info" prepend-icon="$edit">
                    Manage
                  </v-btn>
                </template>
              </DashboardCard>
            </v-col>
          </v-row>

          <!-- Recent Activity -->
          <v-card class="mt-6" elevation="4" rounded="xl">
            <v-card-title class="pa-6">
              <v-icon class="mr-3" color="primary">$calendar</v-icon>
              📊 Recent Activity
            </v-card-title>
            <v-card-text>
              <RecentActivity />
            </v-card-text>
          </v-card>
        </template>
      </v-col>
    </v-row>

    <!-- Upload Dialog -->
    <FileUploadDialog 
      v-model="showUploadDialog"
      @upload-success="handleUploadSuccess"
    />
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import FileAccessForm from '@/components/FileAccessForm.vue'
import DashboardCard from '@/components/DashboardCard.vue'
import FileUploadDialog from '@/components/FileUploadDialog.vue'
import RecentActivity from '@/components/RecentActivity.vue'

const router = useRouter()
const authStore = useAuthStore()

const showUploadDialog = ref(false)
const fileAccessCard = ref<HTMLElement>()

const features = [
  {
    icon: '🔐',
    title: 'Secure Encryption',
    description: 'All files are encrypted with enterprise-grade AES encryption and stored securely in Azure.'
  },
  {
    icon: '⏰',
    title: 'Time-Limited Access',
    description: 'Set expiration dates for shared files with automatic cleanup after the specified period.'
  },
  {
    icon: '📧',
    title: 'Email Integration',
    description: 'Recipients receive secure links via email with clear instructions for file access.'
  }
]

const getWelcomeMessage = () => {
  const name = authStore.user?.name?.split(' ')[0] || 'User'
  const hour = new Date().getHours()
  const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'
  
  return `${greeting}, ${name}! 👋`
}

const getDashboardSubtitle = () => {
  if (authStore.isAdministrator) {
    return '🛡️ Administrator Dashboard - Manage users and all content'
  }
  return '📤 Content Owner Dashboard - Share and manage your files'
}

const handleLogin = async () => {
  try {
    await authStore.login()
  } catch (error) {
    console.error('Login failed:', error)
  }
}

const scrollToFileAccess = () => {
  fileAccessCard.value?.scrollIntoView({ behavior: 'smooth' })
}

const navigateTo = (path: string) => {
  router.push(path)
}

const handleUploadSuccess = () => {
  showUploadDialog.value = false
  // Show success notification
  ;(window as any).showNotification?.('File uploaded and shared successfully! 🎉', 'success')
}
</script>

<style scoped>
.home-container {
  padding: 0;
  min-height: 100vh;
}

.hero-section {
  background: linear-gradient(135deg, #1976D2 0%, #1565C0 50%, #0D47A1 100%);
  min-height: 60vh;
  display: flex;
  align-items: center;
  position: relative;
}

.hero-section::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><defs><pattern id="grain" width="100" height="100" patternUnits="userSpaceOnUse"><circle cx="50" cy="50" r="1" fill="white" opacity="0.1"/></pattern></defs><rect width="100" height="100" fill="url(%23grain)"/></svg>');
  opacity: 0.3;
}

.hero-content {
  position: relative;
  z-index: 1;
  padding: 2rem;
}

.main-content {
  margin-top: -100px;
  position: relative;
  z-index: 2;
  padding: 0 1rem;
}

.dashboard-header {
  text-align: center;
  margin-bottom: 2rem;
}

.features-section {
  margin-top: 2rem;
}

.feature-card {
  transition: all 0.3s ease;
  border: 1px solid rgba(0,0,0,0.1);
}

.feature-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15) !important;
}

.feature-icon {
  font-size: 3rem;
  line-height: 1;
}

.hero-actions {
  margin-top: 2rem;
}

@media (max-width: 960px) {
  .hero-section {
    min-height: 50vh;
  }
  
  .main-content {
    margin-top: -50px;
  }
  
  .hero-content {
    padding: 1rem;
  }
  
  .display-1 {
    font-size: 2.5rem !important;
  }
}
</style>
