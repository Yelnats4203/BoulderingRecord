import { FFmpeg } from '@ffmpeg/ffmpeg'
import { fetchFile, toBlobURL } from '@ffmpeg/util'

export const MAX_OUTPUT_SIZE_BYTES: number = 25 * 1024 * 1024
const FFMPEG_CORE_BASE_URL: string = '/ffmpeg'
const OUTPUT_FILENAME: string = 'output.mp4'
const DEFAULT_CRF: number = 28
const DEFAULT_LONG_EDGE: number = 1280
const DEFAULT_PRESET: string = 'veryfast'

export interface CompressVideoOptions {
  crf?: number
  longEdge?: number
  preset?: string
  fps?: number
  enforceSizeLimit?: boolean
}

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

export function compressedFileName(file: File): string {
  const dotIndex: number = file.name.lastIndexOf('.')
  const baseName: string = dotIndex >= 0 ? file.name.slice(0, dotIndex) : file.name
  return `${baseName}-compressed.mp4`
}

export async function compressVideo(
  file: File,
  onProgress?: (ratio: number) => void,
  options?: CompressVideoOptions,
): Promise<File> {
  const crf: number = options?.crf ?? DEFAULT_CRF
  const longEdge: number = options?.longEdge ?? DEFAULT_LONG_EDGE
  const preset: string = options?.preset ?? DEFAULT_PRESET
  const fps: number | undefined = options?.fps
  const enforceSizeLimit: boolean = options?.enforceSizeLimit ?? true

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

    const execArgs: string[] = [
      '-i',
      input,
      '-vf',
      `scale='if(gt(iw,ih),min(${longEdge},iw),-2)':'if(gt(iw,ih),-2,min(${longEdge},ih))'`,
    ]
    if (fps !== undefined) {
      execArgs.push('-r', String(fps))
    }
    execArgs.push(
      '-c:v',
      'libx264',
      '-preset',
      preset,
      '-crf',
      String(crf),
      '-c:a',
      'aac',
      '-b:a',
      '128k',
      '-movflags',
      '+faststart',
      OUTPUT_FILENAME,
    )

    const exitCode: number = await ffmpeg.exec(execArgs)

    if (exitCode !== 0) {
      throw new VideoCompressionError('COMPRESSION_FAILED', '影片壓縮失敗，請確認影片格式後再試一次')
    }

    const data: Uint8Array = (await ffmpeg.readFile(OUTPUT_FILENAME)) as Uint8Array

    await ffmpeg.deleteFile(input).catch((): void => undefined)
    await ffmpeg.deleteFile(OUTPUT_FILENAME).catch((): void => undefined)

    if (enforceSizeLimit && data.byteLength > MAX_OUTPUT_SIZE_BYTES) {
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
