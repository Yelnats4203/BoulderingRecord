const STORAGE_KEY_PREFIX = 'lastGymName:'

export function getLastGymName(userId: string | null): string {
  if (!userId) {
    return ''
  }
  try {
    return localStorage.getItem(STORAGE_KEY_PREFIX + userId) ?? ''
  } catch {
    return ''
  }
}

export function setLastGymName(userId: string | null, gymName: string): void {
  if (!userId) {
    return
  }
  try {
    if (gymName === '') {
      localStorage.removeItem(STORAGE_KEY_PREFIX + userId)
    } else {
      localStorage.setItem(STORAGE_KEY_PREFIX + userId, gymName)
    }
  } catch {
    // 忽略 localStorage 寫入失敗（例如無痕模式、容量已滿）
  }
}
