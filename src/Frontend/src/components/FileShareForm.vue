<template>
  <div class="file-share-container">
    <h2>Share a File</h2>
    
    <form @submit.prevent="shareFile" class="share-form">
      <div class="form-group">
        <label for="file">Select File:</label>
        <input
          id="file"
          ref="fileInput"
          type="file"
          @change="handleFileSelect"
          required
        />
      </div>
      
      <div class="form-group">
        <label for="email">Recipient Email:</label>
        <input
          id="email"
          v-model="recipientEmail"
          type="email"
          placeholder="Enter recipient's email"
          required
        />
      </div>
      
      <div class="form-group">
        <label for="expiration">Expiration (hours):</label>
        <select id="expiration" v-model="expirationHours">
          <option value="1">1 hour</option>
          <option value="4">4 hours</option>
          <option value="24">24 hours</option>
          <option value="72">72 hours</option>
          <option value="168">1 week</option>
        </select>
      </div>
      
      <button type="submit" :disabled="isUploading" class="share-button">
        {{ isUploading ? 'Sharing...' : 'Share File' }}
      </button>
    </form>
    
    <div v-if="shareResult" class="share-result">
      <h3>File Shared Successfully!</h3>
      <p><strong>Share Code:</strong> {{ shareResult.shareCode }}</p>
      <p><strong>Expires:</strong> {{ formatDate(shareResult.expiresAt) }}</p>
      <p><strong>File:</strong> {{ shareResult.fileName }}</p>
    </div>
    
    <div v-if="error" class="error">
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface ShareResult {
  shareId: string
  shareCode: string
  expiresAt: string
  fileName: string
}

const fileInput = ref<HTMLInputElement>()
const recipientEmail = ref('')
const expirationHours = ref(24)
const isUploading = ref(false)
const shareResult = ref<ShareResult | null>(null)
const error = ref('')
const selectedFile = ref<File | null>(null)

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  selectedFile.value = target.files?.[0] || null
}

const shareFile = async () => {
  if (!selectedFile.value || !recipientEmail.value) {
    error.value = 'Please select a file and enter recipient email'
    return
  }
  
  isUploading.value = true
  error.value = ''
  shareResult.value = null
  
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    formData.append('recipientEmail', recipientEmail.value)
    formData.append('expirationHours', expirationHours.value.toString())
    
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7095'
    const response = await fetch(`${apiBaseUrl}/api/files/share`, {
      method: 'POST',
      headers: {
        'Authorization': 'Bearer placeholder-token' // TODO: Implement real auth
      },
      body: formData
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.detail || 'Failed to share file')
    }
    
    shareResult.value = await response.json()
    
    // Reset form
    recipientEmail.value = ''
    expirationHours.value = 24
    if (fileInput.value) {
      fileInput.value.value = ''
    }
    selectedFile.value = null
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An error occurred'
  } finally {
    isUploading.value = false
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}
</script>

<style scoped>
.file-share-container {
  max-width: 500px;
  margin: 0 auto;
  padding: 20px;
}

.share-form {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.share-button {
  background: #007bff;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
  width: 100%;
}

.share-button:hover:not(:disabled) {
  background: #0056b3;
}

.share-button:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.share-result {
  background: #d4edda;
  border: 1px solid #c3e6cb;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 20px;
}

.share-result h3 {
  margin-top: 0;
  color: #155724;
}

.error {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  padding: 15px;
  border-radius: 4px;
  color: #721c24;
}
</style>