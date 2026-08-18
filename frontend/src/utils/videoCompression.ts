import { FFmpeg } from '@ffmpeg/ffmpeg'
import { fetchFile, toBlobURL } from '@ffmpeg/util'

const MAX_OUTPUT_SIZE_BYTES: number = 25 * 1024 * 1024
const FFMPEG_CORE_BASE_URL: string = '/ffmpeg'
const OUTPUT_FILENAME: string = 'output.mp4'

export type VideoCompressionErrorCode = 'COMPRESSION_FAILED' | 'OUTPUT_TOO_LARGE'

export class VideoCompressionError extends Error {
  code: VideoCompressionErrorCode

  constructor(code: VideoCompressionErrorCode, message: string, options?: { cause?: unknown }) {
    super(message, options)
    this.name = 'VideoCompressionError'
    this.code = code
  }
}

let ffmpegInstance: FFmpeg | null = null
let loadingPromise: Promise<FFmpeg> | null = null

async function loadFFmpeg(): Promise<FFmpeg> {
  const ffmpeg = new FFmpeg()
  const [coreURL, wasmURL] = await Promise.all([
    toBlobURL(`${FFMPEG_CORE_BASE_URL}/ffmpeg-core.js`, 'text/javascript'),
    toBlobURL(`${FFMPEG_CORE_BASE_URL}/ffmpeg-core.wasm`, 'application/wasm'),
  ])
  await ffmpeg.load({ coreURL, wasmURL })
  return ffmpeg
}

async function getFFmpeg(): Promise<FFmpeg> {
  if (ffmpegInstance) {
    return ffmpegInstance
  }
  if (!loadingPromise) {
    loadingPromise = loadFFmpeg()
  }
  try {
    ffmpegInstance = await loadingPromise
    return ffmpegInstance
  } catch (error) {
    loadingPromise = null
    throw error
  }
}

function inputFileName(file: File): string {
  const dotIndex: number = file.name.lastIndexOf('.')
  const extension: string = dotIndex >= 0 ? file.name.slice(dotIndex) : ''
  return `input${extension}`
}

function compressedFileName(file: File): string {
  const dotIndex: number = file.name.lastIndexOf('.')
  const baseName: string = dotIndex >= 0 ? file.name.slice(0, dotIndex) : file.name
  return `${baseName}-compressed.mp4`
}

export async function compressVideo(file: File, onProgress?: (ratio: number) => void): Promise<File> {
  let ffmpeg: FFmpeg
  try {
    ffmpeg = await getFFmpeg()
  } catch (error) {
    throw new VideoCompressionError('COMPRESSION_FAILED', '影片壓縮引擎載入失敗', { cause: error })
  }

  const progressHandler: ((event: { progress: number }) => void) | undefined = onProgress
    ? (event: { progress: number }): void => onProgress(Math.min(Math.max(event.progress, 0), 1))
    : undefined
  if (progressHandler) {
    ffmpeg.on('progress', progressHandler)
  }

  const input: string = inputFileName(file)

  try {
    await ffmpeg.writeFile(input, await fetchFile(file))

    const exitCode: number = await ffmpeg.exec([
      '-i',
      input,
      '-vf',
      "scale='if(gt(iw,ih),min(1280,iw),-2)':'if(gt(iw,ih),-2,min(1280,ih))'",
      '-c:v',
      'libx264',
      '-preset',
      'veryfast',
      '-crf',
      '28',
      '-c:a',
      'aac',
      '-b:a',
      '128k',
      '-movflags',
      '+faststart',
      OUTPUT_FILENAME,
    ])

    if (exitCode !== 0) {
      throw new VideoCompressionError('COMPRESSION_FAILED', '影片壓縮失敗，請確認影片格式後再試一次')
    }

    const data: Uint8Array = (await ffmpeg.readFile(OUTPUT_FILENAME)) as Uint8Array

    await ffmpeg.deleteFile(input).catch((): void => undefined)
    await ffmpeg.deleteFile(OUTPUT_FILENAME).catch((): void => undefined)

    if (data.byteLength > MAX_OUTPUT_SIZE_BYTES) {
      throw new VideoCompressionError('OUTPUT_TOO_LARGE', '壓縮後檔案仍超過 25MB 上限')
    }

    return new File([data as BlobPart], compressedFileName(file), { type: 'video/mp4' })
  } catch (error) {
    if (error instanceof VideoCompressionError) {
      throw error
    }
    throw new VideoCompressionError('COMPRESSION_FAILED', '影片壓縮失敗，請確認影片格式後再試一次', { cause: error })
  } finally {
    if (progressHandler) {
      ffmpeg.off('progress', progressHandler)
    }
  }
}
