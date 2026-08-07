import { ref } from 'vue'
import { defineStore } from 'pinia'

const MAX_ENTRIES = 5

export const useVideoPlaybackCacheStore = defineStore('videoPlaybackCache', () => {
  const cache = ref<Map<string, string>>(new Map())

  function get(sendId: string): string | undefined {
    return cache.value.get(sendId)
  }

  function set(sendId: string, playbackUrl: string): void {
    cache.value.delete(sendId)
    cache.value.set(sendId, playbackUrl)
    if (cache.value.size > MAX_ENTRIES) {
      const oldestKey: string | undefined = cache.value.keys().next().value
      if (oldestKey !== undefined) {
        cache.value.delete(oldestKey)
      }
    }
  }

  return { get, set }
})
