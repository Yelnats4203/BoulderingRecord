<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  acceptFriendRequest,
  deleteFriendRequest,
  getFriends,
  getPendingFriendRequests,
  sendFriendRequest,
} from '../api/friends'
import { searchUsers } from '../api/users'
import type { FriendRequestSummary, FriendSummary } from '../types/friends'
import type { UserSearchResult } from '../types/users'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import { useFriendRequestsStore } from '../stores/friendRequests'

const router = useRouter()
const friendRequestsStore = useFriendRequestsStore()

const friends = ref<FriendSummary[]>([])
const friendsLoading = ref<boolean>(false)
const friendsErrorMessage = ref<string>('')

const pendingRequests = ref<FriendRequestSummary[]>([])
const pendingRequestsLoading = ref<boolean>(false)
const pendingRequestsErrorMessage = ref<string>('')
const respondingRequestId = ref<string>('')

const searchKeyword = ref<string>('')
const searchResults = ref<UserSearchResult[]>([])
const isSearching = ref<boolean>(false)
const searchErrorMessage = ref<string>('')
const sendingRequestToUserId = ref<string>('')
let searchTimeoutId: ReturnType<typeof setTimeout> | undefined

const rejectingRequest = ref<FriendRequestSummary | null>(null)
const isRejecting = ref<boolean>(false)

const removingFriend = ref<FriendSummary | null>(null)
const isRemovingFriend = ref<boolean>(false)

async function fetchFriends(): Promise<void> {
  friendsLoading.value = true
  friendsErrorMessage.value = ''
  try {
    friends.value = await getFriends()
  } catch {
    friendsErrorMessage.value = '讀取好友清單失敗，請稍後再試。'
  } finally {
    friendsLoading.value = false
  }
}

async function fetchPendingRequests(): Promise<void> {
  pendingRequestsLoading.value = true
  pendingRequestsErrorMessage.value = ''
  try {
    pendingRequests.value = await getPendingFriendRequests()
  } catch {
    pendingRequestsErrorMessage.value = '讀取好友邀請失敗，請稍後再試。'
  } finally {
    pendingRequestsLoading.value = false
  }
}

function onSearchInput(): void {
  if (searchTimeoutId) {
    clearTimeout(searchTimeoutId)
  }
  const keyword = searchKeyword.value.trim()
  if (!keyword) {
    searchResults.value = []
    return
  }
  searchTimeoutId = setTimeout(() => {
    void runSearch(keyword)
  }, 400)
}

async function runSearch(keyword: string): Promise<void> {
  isSearching.value = true
  searchErrorMessage.value = ''
  try {
    searchResults.value = await searchUsers(keyword)
  } catch {
    searchErrorMessage.value = '搜尋使用者失敗，請稍後再試。'
  } finally {
    isSearching.value = false
  }
}

async function handleSendRequest(user: UserSearchResult): Promise<void> {
  sendingRequestToUserId.value = user.id
  try {
    await sendFriendRequest({ addresseeId: user.id })
    const result = searchResults.value.find((r) => r.id === user.id)
    if (result) {
      result.relationStatus = 'RequestSentByMe'
    }
  } catch {
    searchErrorMessage.value = '送出好友邀請失敗，請稍後再試。'
  } finally {
    sendingRequestToUserId.value = ''
  }
}

async function handleAcceptFromSearch(user: UserSearchResult): Promise<void> {
  if (!user.friendRequestId) {
    return
  }
  sendingRequestToUserId.value = user.id
  try {
    await acceptFriendRequest(user.friendRequestId)
    const result = searchResults.value.find((r) => r.id === user.id)
    if (result) {
      result.relationStatus = 'Friends'
    }
    await Promise.all([fetchFriends(), fetchPendingRequests()])
    void friendRequestsStore.refreshPendingCount()
  } catch {
    searchErrorMessage.value = '接受好友邀請失敗，請稍後再試。'
  } finally {
    sendingRequestToUserId.value = ''
  }
}

async function handleAcceptRequest(request: FriendRequestSummary): Promise<void> {
  respondingRequestId.value = request.id
  try {
    await acceptFriendRequest(request.id)
    pendingRequests.value = pendingRequests.value.filter((r) => r.id !== request.id)
    await fetchFriends()
    void friendRequestsStore.refreshPendingCount()
  } catch {
    pendingRequestsErrorMessage.value = '接受好友邀請失敗，請稍後再試。'
  } finally {
    respondingRequestId.value = ''
  }
}

function confirmRejectRequest(request: FriendRequestSummary): void {
  rejectingRequest.value = request
}

async function handleRejectConfirmed(): Promise<void> {
  if (!rejectingRequest.value) {
    return
  }
  isRejecting.value = true
  try {
    await deleteFriendRequest(rejectingRequest.value.id)
    pendingRequests.value = pendingRequests.value.filter((r) => r.id !== rejectingRequest.value?.id)
    rejectingRequest.value = null
    void friendRequestsStore.refreshPendingCount()
  } catch {
    pendingRequestsErrorMessage.value = '拒絕好友邀請失敗，請稍後再試。'
    rejectingRequest.value = null
  } finally {
    isRejecting.value = false
  }
}

function confirmRemoveFriend(friend: FriendSummary): void {
  removingFriend.value = friend
}

async function handleRemoveFriendConfirmed(): Promise<void> {
  if (!removingFriend.value) {
    return
  }
  isRemovingFriend.value = true
  try {
    await deleteFriendRequest(removingFriend.value.id)
    friends.value = friends.value.filter((f) => f.id !== removingFriend.value?.id)
    removingFriend.value = null
  } catch {
    friendsErrorMessage.value = '刪除好友失敗，請稍後再試。'
    removingFriend.value = null
  } finally {
    isRemovingFriend.value = false
  }
}

function goToFriendProfile(friend: FriendSummary): void {
  void router.push({ name: 'friendProfile', params: { userId: friend.userId }, query: { username: friend.username } })
}

onMounted(() => {
  void fetchFriends()
  void fetchPendingRequests()
})
</script>

<template>
  <div class="page friends-page">
    <div class="page-header">
      <h2>好友</h2>
    </div>

    <section class="card friends-section">
      <h3>搜尋新增好友</h3>
      <div class="form-field">
        <input v-model="searchKeyword" type="text" placeholder="輸入使用者名稱" @input="onSearchInput" />
      </div>
      <p v-if="searchErrorMessage" class="error-text">{{ searchErrorMessage }}</p>
      <p v-else-if="isSearching" class="hint-text">搜尋中...</p>
      <ul v-else-if="searchResults.length > 0" class="friend-result-list">
        <li v-for="user in searchResults" :key="user.id" class="friend-result-item">
          <span>{{ user.username }}</span>
          <button
            v-if="user.relationStatus === 'None'"
            class="btn-secondary"
            type="button"
            :disabled="sendingRequestToUserId === user.id"
            @click="handleSendRequest(user)"
          >
            加好友
          </button>
          <button v-else-if="user.relationStatus === 'RequestSentByMe'" class="btn-secondary" type="button" disabled>
            邀請中
          </button>
          <button
            v-else-if="user.relationStatus === 'RequestReceivedFromThem'"
            class="btn-primary friend-inline-btn"
            type="button"
            :disabled="sendingRequestToUserId === user.id"
            @click="handleAcceptFromSearch(user)"
          >
            接受邀請
          </button>
          <button v-else class="btn-secondary" type="button" disabled>已是好友</button>
        </li>
      </ul>
      <p v-else-if="searchKeyword.trim()" class="hint-text">查無符合的使用者。</p>
    </section>

    <section class="card friends-section">
      <h3>收到的邀請</h3>
      <p v-if="pendingRequestsErrorMessage" class="error-text">{{ pendingRequestsErrorMessage }}</p>
      <p v-else-if="pendingRequestsLoading" class="hint-text">載入中...</p>
      <ul v-else-if="pendingRequests.length > 0" class="friend-result-list">
        <li v-for="request in pendingRequests" :key="request.id" class="friend-result-item">
          <span>{{ request.otherUsername }}</span>
          <div class="friend-request-actions">
            <button
              class="btn-primary friend-inline-btn"
              type="button"
              :disabled="respondingRequestId === request.id"
              @click="handleAcceptRequest(request)"
            >
              接受
            </button>
            <button class="btn-secondary" type="button" @click="confirmRejectRequest(request)">拒絕</button>
          </div>
        </li>
      </ul>
      <p v-else class="hint-text">目前沒有待處理的好友邀請。</p>
    </section>

    <section class="card friends-section">
      <h3>好友清單</h3>
      <p v-if="friendsErrorMessage" class="error-text">{{ friendsErrorMessage }}</p>
      <p v-else-if="friendsLoading" class="hint-text">載入中...</p>
      <ul v-else-if="friends.length > 0" class="friend-result-list">
        <li v-for="friend in friends" :key="friend.id" class="friend-result-item">
          <button type="button" class="friend-name-link" @click="goToFriendProfile(friend)">{{ friend.username }}</button>
          <button class="btn-danger" type="button" @click="confirmRemoveFriend(friend)">刪除好友</button>
        </li>
      </ul>
      <p v-else class="hint-text">目前還沒有好友。</p>
    </section>

    <ConfirmDialog
      v-if="rejectingRequest"
      title="拒絕好友邀請"
      :message="`確定要拒絕 ${rejectingRequest.otherUsername} 的好友邀請嗎？`"
      :confirm-loading="isRejecting"
      confirm-loading-text="處理中..."
      @confirm="handleRejectConfirmed"
      @cancel="rejectingRequest = null"
    />

    <ConfirmDialog
      v-if="removingFriend"
      title="刪除好友"
      :message="`確定要刪除好友 ${removingFriend.username} 嗎？`"
      :confirm-loading="isRemovingFriend"
      confirm-loading-text="刪除中..."
      @confirm="handleRemoveFriendConfirmed"
      @cancel="removingFriend = null"
    />
  </div>
</template>

<style scoped>
.friends-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.friends-section h3 {
  margin-top: 0;
}

.friend-result-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.friend-result-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid var(--color-border);
}

.friend-result-item:last-child {
  border-bottom: none;
}

.friend-request-actions {
  display: flex;
  gap: 8px;
}

.friend-inline-btn {
  width: auto;
}

.friend-name-link {
  background: none;
  border: none;
  padding: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-primary);
  cursor: pointer;
  text-align: left;
}
</style>
