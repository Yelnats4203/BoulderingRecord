<script setup lang="ts">
import { ref } from 'vue'
import { compressVideo, VideoCompressionError } from '../utils/videoCompression'
import { compressVideoWebCodecs } from '../utils/videoCompressionWebCodecs'
import LoadingSpinner from '../components/LoadingSpinner.vue'

interface CompressionCombo {
  label: string
  method: 'ffmpeg' | 'webcodecs'
  longEdge: number
  crf?: number
  preset?: string
  fps?: number
  bitrate?: number
}

interface CompressionResult {
  label: string
  originalBytes: number
  compressedBytes: number
  ratioPercent: number
  elapsedSeconds: number
  errorMessage: string
}

const COMBOS: CompressionCombo[] = [
  { label: 'webcodecs_long720_2mbps', method: 'webcodecs', longEdge: 720, bitrate: 2_000_000 },
  { label: 'webcodecs_long720_4mbps', method: 'webcodecs', longEdge: 720, bitrate: 4_000_000 },
  { label: 'crf28_long720_veryfast', method: 'ffmpeg', crf: 28, longEdge: 720, preset: 'veryfast' },
  { label: 'crf28_long720_ultrafast', method: 'ffmpeg', crf: 28, longEdge: 720, preset: 'ultrafast' },
]

const videoFile = ref<File | null>(null)
const isRunning = ref<boolean>(false)
const currentComboLabel = ref<string>('')
const currentProgress = ref<number>(0)
const results = ref<CompressionResult[]>([])

function handleFileChange(event: Event): void {
  const input = event.target as HTMLInputElement
  videoFile.value = input.files && input.files.length > 0 ? input.files[0] : null
  results.value = []
}

function formatBytes(bytes: number): string {
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`
}

function baseName(file: File): string {
  const dotIndex: number = file.name.lastIndexOf('.')
  return dotIndex >= 0 ? file.name.slice(0, dotIndex) : file.name
}

function downloadFile(file: File): void {
  const url: string = URL.createObjectURL(file)
  const anchor: HTMLAnchorElement = document.createElement('a')
  anchor.href = url
  anchor.download = file.name
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)
  URL.revokeObjectURL(url)
}

async function runAllCombos(): Promise<void> {
  if (!videoFile.value) {
    return
  }
  const sourceFile: File = videoFile.value
  const originalBytes: number = sourceFile.size

  isRunning.value = true
  results.value = []

  for (const combo of COMBOS) {
    currentComboLabel.value = combo.label
    currentProgress.value = 0

    const startedAt: number = performance.now()
    try {
      const progressCallback = (ratio: number): void => {
        currentProgress.value = ratio
      }
      const compressed: File =
        combo.method === 'webcodecs'
          ? await compressVideoWebCodecs(sourceFile, progressCallback, {
              longEdge: combo.longEdge,
              bitrate: combo.bitrate,
              fps: combo.fps,
              enforceSizeLimit: false,
            })
          : await compressVideo(sourceFile, progressCallback, {
              crf: combo.crf,
              longEdge: combo.longEdge,
              preset: combo.preset,
              fps: combo.fps,
              enforceSizeLimit: false,
            })
      const elapsedSeconds: number = (performance.now() - startedAt) / 1000

      const outputFile: File = new File([compressed], `${baseName(sourceFile)}_${combo.label}.mp4`, { type: 'video/mp4' })
      downloadFile(outputFile)

      results.value.push({
        label: combo.label,
        originalBytes,
        compressedBytes: outputFile.size,
        ratioPercent: (outputFile.size / originalBytes) * 100,
        elapsedSeconds,
        errorMessage: '',
      })
    } catch (error) {
      console.error(`[compression-test] ${combo.label} 失敗`, error, error instanceof VideoCompressionError ? error.cause : undefined)
      const elapsedSeconds: number = (performance.now() - startedAt) / 1000
      const message: string = error instanceof VideoCompressionError ? error.message : '壓縮失敗'
      results.value.push({
        label: combo.label,
        originalBytes,
        compressedBytes: 0,
        ratioPercent: 0,
        elapsedSeconds,
        errorMessage: message,
      })
    }
  }

  currentComboLabel.value = ''
  currentProgress.value = 0
  isRunning.value = false
}
</script>

<template>
  <div class="page compression-test-page">
    <div class="card compression-test-form">
      <h2>影片壓縮測試（開發用）</h2>
      <p class="hint-text">
        選擇測試影片後，會依序用 {{ COMBOS.length }} 組 CRF／解析度設定壓縮，每組完成後自動下載到瀏覽器預設下載資料夾，檔名帶有設定組合。
      </p>

      <div class="form-field">
        <label for="testVideo">測試影片檔案</label>
        <input id="testVideo" type="file" accept="video/mp4,video/quicktime,.mp4,.mov" :disabled="isRunning" @change="handleFileChange" />
      </div>

      <button class="btn-primary" :class="{ 'btn-loading': isRunning }" type="button" :disabled="!videoFile || isRunning" @click="runAllCombos">
        <LoadingSpinner v-if="isRunning" :size="16" />
        <span v-if="isRunning">
          測試中（{{ currentComboLabel }}{{ currentProgress > 0 ? ` ${Math.round(currentProgress * 100)}%` : '...' }}）
        </span>
        <span v-else>開始測試</span>
      </button>

      <table v-if="results.length > 0" class="results-table">
        <thead>
          <tr>
            <th>設定</th>
            <th>原始大小</th>
            <th>壓縮後大小</th>
            <th>壓縮比例</th>
            <th>耗時</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="result in results" :key="result.label">
            <td>{{ result.label }}</td>
            <td>{{ formatBytes(result.originalBytes) }}</td>
            <td v-if="result.errorMessage" class="error-text" colspan="2">{{ result.errorMessage }}</td>
            <template v-else>
              <td>{{ formatBytes(result.compressedBytes) }}</td>
              <td>{{ result.ratioPercent.toFixed(1) }}%</td>
            </template>
            <td>{{ result.elapsedSeconds.toFixed(1) }} 秒</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.compression-test-form {
  max-width: 640px;
  margin: 0 auto 24px;
}

.compression-test-form h2 {
  margin-top: 0;
}

.hint-text {
  color: #6b7280;
  font-size: 0.9rem;
  margin: 0 0 16px;
}

.results-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
  font-size: 0.9rem;
}

.results-table th,
.results-table td {
  border: 1px solid #e5e7eb;
  padding: 6px 10px;
  text-align: left;
}
</style>
