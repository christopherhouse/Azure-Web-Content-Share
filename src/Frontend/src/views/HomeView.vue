<template>
  <div class="home">
    <div class="hero">
      <h1>Azure Web Content Share</h1>
      <p>Secure file sharing made simple</p>
    </div>
    
    <div class="main-content">
      <div class="tabs">
        <button 
          :class="{ active: activeTab === 'share' }" 
          @click="activeTab = 'share'"
        >
          Share a File
        </button>
        <button 
          :class="{ active: activeTab === 'download' }" 
          @click="activeTab = 'download'"
        >
          Download File
        </button>
      </div>
      
      <div class="tab-content">
        <FileShareForm v-if="activeTab === 'share'" />
        <FileDownload v-if="activeTab === 'download'" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import FileShareForm from '@/components/FileShareForm.vue'
import FileDownload from '@/components/FileDownload.vue'

const activeTab = ref('share')
</script>

<style scoped>
.home {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
}

.hero {
  text-align: center;
  color: white;
  margin-bottom: 40px;
}

.hero h1 {
  font-size: 3rem;
  margin-bottom: 10px;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
}

.hero p {
  font-size: 1.2rem;
  opacity: 0.9;
}

.main-content {
  max-width: 800px;
  margin: 0 auto;
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
  overflow: hidden;
}

.tabs {
  display: flex;
  background: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
}

.tabs button {
  flex: 1;
  padding: 15px 20px;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 16px;
  font-weight: 500;
  color: #6c757d;
  transition: all 0.3s ease;
}

.tabs button:hover {
  background: #e9ecef;
  color: #495057;
}

.tabs button.active {
  background: white;
  color: #007bff;
  border-bottom: 3px solid #007bff;
}

.tab-content {
  padding: 0;
}
</style>
