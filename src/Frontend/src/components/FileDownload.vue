<template>
  <div class="file-download-container">
    <h2>Access Shared File</h2>
    
    <form @submit.prevent="downloadFile" class="download-form">
      <div class="form-group">
        <label for="shareCode">Share Code:</label>
        <input
          id="shareCode"
          v-model="shareCode"
          type="text"
          placeholder="Enter your share code"
          required
        />
      </div>
      
      <button type="submit" :disabled="isLoading" class="download-button">
        {{ isLoading ? 'Loading...' : 'Download File' }}
      </button>
    </form>
    
    <div v-if="fileInfo" class="file-info">
      <h3>File Information</h3>
      <p><strong>File Name:</strong> {{ fileInfo.fileName }}</p>
      <p><strong>File Size:</strong> {{ formatFileSize(fileInfo.fileSize) }}</p>
      <p><strong>Created:</strong> {{ formatDate(fileInfo.createdAt) }}</p>
      <p><strong>Expires:</strong> {{ formatDate(fileInfo.expiresAt) }}</p>
      
      <button @click="initiateDownload" class="download-file-button">
        Download {{ fileInfo.fileName }}
      </button>
    </div>
    
    <div v-if="error" class="error">
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface FileInfo {
  id: string
  fileName: string
  contentType: string
  fileSize: number
  createdAt: string
  expiresAt: string
}

const shareCode = ref('')
const fileInfo = ref<FileInfo | null>(null)
const isLoading = ref(false)
const error = ref('')

const downloadFile = async () => {
  if (!shareCode.value.trim()) {
    error.value = 'Please enter a share code'
    return
  }
  
  isLoading.value = true
  error.value = ''
  fileInfo.value = null
  
  try {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7095'
    const response = await fetch(`${apiBaseUrl}/api/files/metadata/${shareCode.value.trim()}`)
    
    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Share code not found or has expired')
      }
      const errorData = await response.json()
      throw new Error(errorData.detail || 'Failed to get file information')
    }
    
    fileInfo.value = await response.json()
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An error occurred'
  } finally {
    isLoading.value = false
  }
}

const initiateDownload = async () => {
  if (!shareCode.value || !fileInfo.value) return
  
  try {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7095'
    const response = await fetch(`${apiBaseUrl}/api/files/download/${shareCode.value.trim()}`)
    
    if (!response.ok) {
      throw new Error('Failed to download file')
    }
    
    // Create blob and download link
    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = fileInfo.value.fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Download failed'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>

<style scoped>
.file-download-container {
  max-width: 500px;
  margin: 0 auto;
  padding: 20px;
}

.download-form {
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

.form-group input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.download-button,
.download-file-button {
  background: #28a745;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
  width: 100%;
  margin-bottom: 10px;
}

.download-button:hover:not(:disabled),
.download-file-button:hover {
  background: #218838;
}

.download-button:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.file-info {
  background: #e7f3ff;
  border: 1px solid #b3d9ff;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 20px;
}

.file-info h3 {
  margin-top: 0;
  color: #004085;
}

.error {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  padding: 15px;
  border-radius: 4px;
  color: #721c24;
}
</style>