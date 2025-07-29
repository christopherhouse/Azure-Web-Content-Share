<template>
  <v-form @submit.prevent="handleSubmit" ref="formRef">
    <v-card elevation="0" rounded="lg">
      <v-card-text class="pa-6">
        <div class="text-center mb-6">
          <v-icon color="primary" size="64" class="mb-3">$link</v-icon>
          <h3 class="text-h5 mb-2">Enter Share Code</h3>
          <p class="text-body-1 text-medium-emphasis">
            Enter the share code you received to access the file
          </p>
        </div>

        <v-text-field
          v-model="shareCode"
          label="Share Code"
          placeholder="Enter your share code here..."
          variant="outlined"
          prepend-inner-icon="$link"
          :rules="shareCodeRules"
          :loading="loading"
          :error="!!error"
          :error-messages="error"
          class="mb-4"
          @keyup.enter="handleSubmit"
        ></v-text-field>

        <v-btn
          type="submit"
          color="primary"
          size="large"
          block
          :loading="loading"
          :disabled="!shareCode"
          prepend-icon="$download"
        >
          Access File
        </v-btn>
      </v-card-text>
    </v-card>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'

const formRef = ref()
const shareCode = ref('')
const loading = ref(false)
const error = ref('')

const shareCodeRules = [
  (value: string) => {
    if (!value) return 'Share code is required'
    if (value.length < 6) return 'Share code must be at least 6 characters'
    return true
  }
]

const handleSubmit = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  loading.value = true
  error.value = ''

  try {
    // First, get file metadata
    const metadataResponse = await axios.get(`/api/files/metadata/${shareCode.value}`)
    const fileInfo = metadataResponse.data

    // Then initiate download
    const downloadResponse = await axios.get(`/api/files/download/${shareCode.value}`, {
      responseType: 'blob',
    })

    // Create download link
    const url = window.URL.createObjectURL(new Blob([downloadResponse.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', fileInfo.originalFileName || 'download')
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)

    // Show success message
    ;(window as any).showNotification?.(`File "${fileInfo.originalFileName}" downloaded successfully! 📥`, 'success')
    
    // Clear form
    shareCode.value = ''
    formRef.value.reset()

  } catch (err: any) {
    console.error('Download failed:', err)
    
    if (err.response?.status === 404) {
      error.value = 'Invalid share code or file not found'
    } else if (err.response?.status === 410) {
      error.value = 'This share code has expired'
    } else {
      error.value = 'Failed to access file. Please try again.'
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.v-form {
  max-width: 500px;
  margin: 0 auto;
}
</style>