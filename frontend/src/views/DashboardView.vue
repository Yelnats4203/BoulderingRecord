<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { getSessions } from '../api/sessions'
import { getRecentFriendVideos } from '../api/friends'
import type { SessionResponse } from '../types/sessions'
import type { FriendVideo } from '../types/friends'
import { calculateWeeklyFrequency, countByGym, getDefaultDateRange, groupByDay, groupByGrade } from '../utils/sessionStats'
import BarChart from '../components/BarChart.vue'
import DashboardFilterForm from '../components/DashboardFilterForm.vue'

const COMPLETED_COLOR = '#2563eb'
const UNCOMPLETED_COLOR = '#9ca3af'

const { dateFrom: defaultDateFrom, dateTo: defaultDateTo } = getDefaultDateRange()

const sessions = ref<SessionResponse[]>([])
const isLoading = ref<boolean>(false)
const errorMessage = ref<string>('')
const dateFrom = ref<string>(defaultDateFrom)
const dateTo = ref<string>(defaultDateTo)

async function fetchSessions(range: { dateFrom: string; dateTo: string }): Promise<void> {
  dateFrom.value = range.dateFrom
  dateTo.value = range.dateTo
  isLoading.value = true
  errorMessage.value = ''
  try {
    sessions.value = await getSessions(range.dateFrom, range.dateTo)
  } catch {
    errorMessage.value = '讀取抱石紀錄失敗，請稍後再試。'
  } finally {
    isLoading.value = false
  }
}

const friendVideos = ref<FriendVideo[]>([])
const friendVideosLoading = ref<boolean>(false)
const friendVideosErrorMessage = ref<string>('')

async function fetchFriendVideos(): Promise<void> {
  friendVideosLoading.value = true
  friendVideosErrorMessage.value = ''
  try {
    friendVideos.value = await getRecentFriendVideos()
  } catch {
    friendVideosErrorMessage.value = '讀取好友動態失敗，請稍後再試。'
  } finally {
    friendVideosLoading.value = false
  }
}

onMounted(() => {
  void fetchSessions({ dateFrom: defaultDateFrom, dateTo: defaultDateTo })
  void fetchFriendVideos()
})

const dailyStats = computed(() => groupByDay(sessions.value))
const gradeStats = computed(() => groupByGrade(sessions.value))
const weeklyFrequency = computed(() => calculateWeeklyFrequency(sessions.value, dateFrom.value, dateTo.value))
const gymStats = computed(() => countByGym(sessions.value))

const dailyDatasets = computed(() => [
  { label: '完攀', data: dailyStats.value.map((stat) => stat.completed), color: COMPLETED_COLOR },
  { label: '未完攀', data: dailyStats.value.map((stat) => stat.uncompleted), color: UNCOMPLETED_COLOR },
])

const gradeDatasets = computed(() => [
  { label: '完攀', data: gradeStats.value.map((stat) => stat.completed), color: COMPLETED_COLOR },
  { label: '未完攀', data: gradeStats.value.map((stat) => stat.uncompleted), color: UNCOMPLETED_COLOR },
])
</script>

<template>
  <div class="page dashboard-page">
    <div class="page-header">
      <h2>儀表板</h2>
    </div>

    <section class="dashboard-section friend-activity-section">
      <h3>好友動態</h3>
      <p v-if="friendVideosErrorMessage" class="error-text">{{ friendVideosErrorMessage }}</p>
      <p v-else-if="friendVideosLoading" class="hint-text">載入中...</p>
      <div v-else-if="friendVideos.length > 0" class="friend-video-grid">
        <RouterLink
          v-for="item in friendVideos"
          :key="item.video.id"
          class="card friend-video-card"
          :to="{ name: 'friendProfile', params: { userId: item.friendUserId }, query: { username: item.friendUsername } }"
        >
          <img class="video-thumbnail" :src="item.video.thumbnailUrl" alt="影片縮圖" />
          <div class="friend-video-caption">{{ item.friendUsername }} · {{ item.video.gymName ?? '-' }}</div>
        </RouterLink>
      </div>
      <p v-else class="card empty-state">好友尚無公開影片。</p>
    </section>

    <DashboardFilterForm
      :is-loading="isLoading"
      :initial-date-from="defaultDateFrom"
      :initial-date-to="defaultDateTo"
      @filter="fetchSessions"
    />

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    <p v-else-if="isLoading" class="hint-text">載入中...</p>

    <div v-else class="dashboard-content">
      <section class="dashboard-section">
        <h3>每日完攀／未完攀路線數量</h3>
        <BarChart
          v-if="dailyStats.length > 0"
          :labels="dailyStats.map((stat) => stat.label)"
          :datasets="dailyDatasets"
          stacked
        />
        <p v-else class="card empty-state">尚未有資料。</p>
      </section>

      <section class="dashboard-section">
        <h3>各難度完攀／未完攀路線數量</h3>
        <BarChart
          v-if="gradeStats.length > 0"
          :labels="gradeStats.map((stat) => stat.label)"
          :datasets="gradeDatasets"
          stacked
        />
        <p v-else class="card empty-state">尚未有資料。</p>
      </section>

      <section class="dashboard-section">
        <h3>每週抱石頻率</h3>
        <p class="card frequency-value">平均每週 {{ weeklyFrequency.toFixed(1) }} 次</p>
      </section>

      <section class="dashboard-section">
        <h3>岩館造訪次數</h3>
        <ul v-if="gymStats.length > 0" class="card gym-list">
          <li v-for="gym in gymStats" :key="gym.gymName">{{ gym.gymName }}：{{ gym.count }} 次</li>
        </ul>
        <p v-else class="card empty-state">此區間內尚無岩館紀錄。</p>
      </section>
    </div>
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

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.friend-activity-section {
  margin: 16px 0 24px;
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

.dashboard-section h3 {
  margin: 0 0 12px;
  font-size: 1rem;
}

.frequency-value {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  text-align: center;
}

.gym-list {
  list-style: none;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}
</style>
