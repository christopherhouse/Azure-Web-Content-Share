<template>
  <v-dialog
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    max-width="600px"
    persistent
  >
    <v-card rounded="xl">
      <v-card-title class="pa-6 pb-2">
        <div class="d-flex align-center">
          <v-icon color="success" class="mr-3" size="large">$upload</v-icon>
          <span class="text-h4">📤 Share a File</span>
        </div>
      </v-card-title>

      <v-card-text class="pa-6">
        <v-form @submit.prevent="handleSubmit" ref="formRef">
          <!-- File Upload Area -->
          <div class="file-upload-area mb-6">
            <v-file-input
              v-model="selectedFile"
              label="Select File"
              variant="outlined"
              prepend-icon="$file"
              :rules="fileRules"
              accept="*/*"
              show-size
              @change="handleFileSelect"
            ></v-file-input>
            
            <!-- Drag and drop area -->
            <div 
              v-if="!selectedFile"
              class="drop-zone"
              @dragover.prevent
              @drop="handleFileDrop"
            >
              <v-icon size="48" color="primary" class="mb-3">$cloud</v-icon>
              <p class="text-body-1 mb-2">Drag and drop a file here</p>
              <p class="text-caption text-medium-emphasis">Maximum file size: 100MB</p>
            </div>
          </div>

          <!-- Recipient Email -->
          <v-text-field
            v-model="recipientEmail"
            label="Recipient Email"
            variant="outlined"
            prepend-inner-icon="$email"
            :rules="emailRules"
            type="email"
            class="mb-4"
          ></v-text-field>

          <!-- Expiration Days -->
          <v-select
            v-model="expirationDays"
            label="Expiration"
            variant="outlined"
            prepend-inner-icon="$calendar"
            :items="expirationOptions"
            :rules="expirationRules"
            class="mb-4"
          ></v-select>

          <!-- Optional Message -->
          <v-textarea
            v-model="message"
            label="Message (Optional)"
            variant="outlined"
            prepend-inner-icon="$info"
            rows="3"
            counter="500"
            :rules="messageRules"
            class="mb-4"
          ></v-textarea>

          <!-- Upload Progress -->
          <v-progress-linear
            v-if="uploading"
            :model-value="uploadProgress"
            color="success"
            height="6"
            rounded
            class="mb-4"
          ></v-progress-linear>

          <!-- Actions -->
          <div class="d-flex justify-end gap-3">
            <v-btn
              variant="outlined"
              @click="$emit('update:modelValue', false)"
              :disabled="uploading"
            >
              Cancel
            </v-btn>
            <v-btn
              type="submit"
              color="success"
              :loading="uploading"
              :disabled="!selectedFile || !recipientEmail"
              prepend-icon="$share"
            >
              Share File
            </v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

interface Props {
  modelValue: boolean
}

defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'upload-success': [response: any]
}>()

const authStore = useAuthStore()
const formRef = ref()

// Form data
const selectedFile = ref<File[]>([])
const recipientEmail = ref('')
const expirationDays = ref(7)
const message = ref('')

// Upload state
const uploading = ref(false)
const uploadProgress = ref(0)

const expirationOptions = [
  { title: '1 Day', value: 1 },
  { title: '3 Days', value: 3 },
  { title: '7 Days', value: 7 },
]

// Validation rules
const fileRules = [
  (value: File[]) => {
    if (!value || value.length === 0) return 'File is required'
    const file = value[0]
    if (file.size > 100 * 1024 * 1024) return 'File size must be less than 100MB'
    return true
  }
]

const emailRules = [
  (value: string) => {
    if (!value) return 'Recipient email is required'
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailPattern.test(value)) return 'Please enter a valid email address'
    return true
  }
]

const expirationRules = [
  (value: number) => {
    if (!value) return 'Expiration time is required'
    if (value < 1 || value > 7) return 'Expiration must be between 1 and 7 days'
    return true
  }
]

const messageRules = [
  (value: string) => {
    if (value && value.length > 500) return 'Message must be less than 500 characters'
    return true
  }
]

const handleFileSelect = () => {
  // File selection is handled by v-file-input
}

const handleFileDrop = (event: DragEvent) => {
  event.preventDefault()
  const files = event.dataTransfer?.files
  if (files && files.length > 0) {
    selectedFile.value = [files[0]]
  }
}

const handleSubmit = async () => {
  const { valid } = await formRef.value.validate()
  if (!valid) return

  uploading.value = true
  uploadProgress.value = 0

  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value[0])
    formData.append('recipientEmail', recipientEmail.value)
    formData.append('expirationDays', expirationDays.value.toString())
    if (message.value) {
      formData.append('message', message.value)
    }

    const response = await axios.post('/api/files/share', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        ...authStore.getAuthHeader(),
      },
      onUploadProgress: (progressEvent) => {
        if (progressEvent.total) {
          uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        }
      },
    })

    emit('upload-success', response.data)
    
    // Reset form
    selectedFile.value = []
    recipientEmail.value = ''
    expirationDays.value = 7
    message.value = ''
    formRef.value.reset()

  } catch (error: any) {
    console.error('Upload failed:', error)
    
    let errorMessage = 'Failed to upload file. Please try again.'
    if (error.response?.data?.detail) {
      errorMessage = error.response.data.detail
    } else if (error.response?.status === 401) {
      errorMessage = 'Authentication required. Please sign in again.'
    }
    
    ;(window as any).showNotification?.(errorMessage, 'error')
  } finally {
    uploading.value = false
    uploadProgress.value = 0
  }
}
</script>

<style scoped>
.file-upload-area {
  position: relative;
}

.drop-zone {
  border: 2px dashed #1976D2;
  border-radius: 8px;
  padding: 2rem;
  text-align: center;
  background: rgba(25, 118, 210, 0.05);
  transition: all 0.3s ease;
  cursor: pointer;
}

.drop-zone:hover {
  background: rgba(25, 118, 210, 0.1);
  border-color: #1565C0;
}

.gap-3 {
  gap: 12px;
}
</style>