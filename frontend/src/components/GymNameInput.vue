<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getGymNames } from '../api/gyms'

const props = defineProps<{
  id: string
}>()

const modelValue = defineModel<string>({ required: true })

const gymNames = ref<string[]>([])
const datalistId = `${props.id}-gym-options`

onMounted(async () => {
  try {
    gymNames.value = await getGymNames()
  } catch {
    gymNames.value = []
  }
})
</script>

<template>
  <input :id="id" v-model="modelValue" type="text" :list="datalistId" autocomplete="off" v-bind="$attrs" />
  <datalist :id="datalistId">
    <option v-for="name in gymNames" :key="name" :value="name" />
  </datalist>
</template>
