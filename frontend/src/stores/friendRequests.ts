import { ref } from 'vue'
import { defineStore } from 'pinia'
import { getPendingFriendRequests } from '../api/friends'

export const useFriendRequestsStore = defineStore('friendRequests', () => {
  const pendingCount = ref<number>(0)

  async function refreshPendingCount(): Promise<void> {
    try {
      const pending = await getPendingFriendRequests()
      pendingCount.value = pending.length
    } catch {
      // 紅點是輔助性 UI，抓取失敗時靜默即可，不需要中斷其他頁面操作
    }
  }

  function clear(): void {
    pendingCount.value = 0
  }

  return { pendingCount, refreshPendingCount, clear }
})
