<template>
  <v-card
    class="dashboard-card h-100"
    :color="color"
    variant="tonal"
    rounded="xl"
    elevation="4"
    hover
    @click="$emit('click')"
  >
    <v-card-text class="pa-6">
      <div class="d-flex align-center mb-4">
        <v-icon 
          :icon="icon" 
          size="large" 
          :color="color"
          class="mr-3"
        ></v-icon>
        <div>
          <h3 class="text-h5 font-weight-bold mb-1">{{ title }}</h3>
          <p class="text-body-1 text-medium-emphasis mb-0">{{ subtitle }}</p>
        </div>
      </div>
      
      <slot name="content"></slot>
    </v-card-text>
    
    <v-card-actions v-if="$slots.actions" class="pa-6 pt-0">
      <slot name="actions"></slot>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
interface Props {
  title: string
  subtitle: string
  icon: string
  color?: string
}

defineProps<Props>()
defineEmits<{
  click: []
}>()
</script>

<style scoped>
.dashboard-card {
  cursor: pointer;
  transition: all 0.3s ease;
}

.dashboard-card:hover {
  transform: translateY(-2px);
}
</style>