<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getRecentFriendVideos } from '../api/friends'
import type { FriendVideo } from '../types/friends'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'

const authStore = useAuthStore()
const toastStore = useToastStore()

const friendVideos = ref<FriendVideo[]>([])
const friendVideosLoading = ref<boolean>(false)

async function fetchFriendVideos(): Promise<void> {
  friendVideosLoading.value = true
  try {
    friendVideos.value = await getRecentFriendVideos()
  } catch {
    toastStore.showToast('讀取好友動態失敗，請稍後再試。', 'error')
  } finally {
    friendVideosLoading.value = false
  }
}

onMounted(() => {
  void fetchFriendVideos()
})
</script>

<template>
  <div class="page home-page">
    <div class="page-header">
      <h2>歡迎 {{ authStore.username }} 回來</h2>
    </div>

    <section class="home-section friend-activity-section">
      <h3>好友動態</h3>
      <p v-if="friendVideosLoading" class="hint-text">載入中...</p>
      <div v-else-if="friendVideos.length > 0" class="friend-video-grid">
        <RouterLink
          v-for="item in friendVideos"
          :key="item.video.id"
          class="card friend-video-card"
          :to="{ name: 'friendProfile', params: { userId: item.friendUserId }, query: { username: item.friendUsername } }"
        >
          <img class="video-thumbnail" :src="item.video.thumbnailUrl" alt="完攀影片縮圖" />
          <div class="friend-video-caption">{{ item.friendUsername }} · {{ item.video.gymName ?? '-' }}</div>
        </RouterLink>
      </div>
      <p v-else class="card empty-state">好友尚無公開影片。</p>
    </section>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.page-header h2 {
  margin: 0;
}

.friend-activity-section {
  margin: 16px 0 24px;
}

.home-section h3 {
  margin: 0 0 12px;
  font-size: 1rem;
}

.friend-video-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
}

.friend-video-card {
  display: flex;
  flex-direction: column;
  aspect-ratio: 3 / 4;
  padding: 0;
  overflow: hidden;
  text-decoration: none;
  color: inherit;
}

.friend-video-card .video-thumbnail {
  width: 100%;
  height: 80%;
  object-fit: cover;
  flex-shrink: 0;
  background: var(--color-bg);
}

.friend-video-caption {
  height: 20%;
  display: flex;
  align-items: center;
  padding: 0 10px;
  font-size: 0.85rem;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}
</style>
