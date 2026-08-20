import { createFile, DataStream, Endianness, MP4BoxBuffer } from 'mp4box'
import type { ISOFile, Movie, Sample, Track } from 'mp4box'
import { ArrayBufferTarget, Muxer } from 'mp4-muxer'
import { VideoCompressionError, MAX_OUTPUT_SIZE_BYTES, compressedFileName } from './videoCompression'

const DEFAULT_LONG_EDGE: number = 720
const DEFAULT_BITRATE: number = 2_000_000
const DEFAULT_AUDIO_BITRATE: number = 128_000
const AUDIO_ENCODER_CODEC: string = 'mp4a.40.2'
const CANDIDATE_ENCODER_CODECS: string[] = ['avc1.640028', 'avc1.4d0028', 'avc1.42001f']

export interface CompressVideoWebCodecsOptions {
  longEdge?: number
  bitrate?: number
  fps?: number
  audioBitrate?: number
  enforceSizeLimit?: boolean
}

interface TargetSize {
  width: number
  height: number
}

interface ParsedTracks {
  videoTrack: Track
  audioTrack: Track | null
  videoSamples: Sample[]
  audioSamples: Sample[]
}

interface AacDescriptorEntry {
  esds?: {
    esd: {
      findDescriptor: (tag: number) => { findDescriptor: (tag: number) => { data: Uint8Array } | undefined } | undefined
    }
  }
}

interface AvcDescriptorEntry {
  avcC?: { write: (stream: DataStream) => void }
  hvcC?: { write: (stream: DataStream) => void }
}

function isWebCodecsSupported(): boolean {
  return typeof VideoDecoder !== 'undefined' && typeof VideoEncoder !== 'undefined'
}

function toSigned32(value: number): number {
  return value | 0
}

// 手機拍攝的直式影片，畫面本身多半以橫式（感光元件方向）編碼，
// 靠 tkhd 的 3x3 矩陣記錄「播放時要轉幾度」；解碼出來的 VideoFrame 像素仍是橫式，
// 若不把這個旋轉角度也寫回輸出檔的 tkhd，畫面會變成沒有旋轉提示的橫式，播放時看起來像轉了 90 度。
function getRotationDegrees(matrix: number[] | Int32Array | Uint32Array): 0 | 90 | 180 | 270 {
  const a: number = toSigned32(matrix[0] as number)
  const b: number = toSigned32(matrix[1] as number)
  const angleDeg: number = Math.round((Math.atan2(b, a) * 180) / Math.PI)
  const normalized: number = ((angleDeg % 360) + 360) % 360
  if (normalized === 90 || normalized === 180 || normalized === 270) {
    return normalized
  }
  return 0
}

function computeTargetSize(width: number, height: number, longEdge: number): TargetSize {
  const originalLongEdge: number = Math.max(width, height)
  const scale: number = Math.min(1, longEdge / originalLongEdge)
  const scaledWidth: number = Math.max(2, Math.round((width * scale) / 2) * 2)
  const scaledHeight: number = Math.max(2, Math.round((height * scale) / 2) * 2)
  return { width: scaledWidth, height: scaledHeight }
}

function getAvcDescription(isoFile: ISOFile, trackId: number): Uint8Array {
  const trak = isoFile.getTrackById(trackId)
  const entries = trak.mdia.minf.stbl.stsd.entries as unknown as AvcDescriptorEntry[]
  for (const entry of entries) {
    const box = entry.avcC ?? entry.hvcC
    if (box) {
      const stream: DataStream = new DataStream(undefined, 0, Endianness.BIG_ENDIAN)
      box.write(stream)
      return new Uint8Array(stream.buffer, 8, stream.byteLength - 8)
    }
  }
  throw new VideoCompressionError('COMPRESSION_FAILED', '無法解析影片編碼參數（找不到 avcC/hvcC）')
}

function getAacDescription(isoFile: ISOFile, trackId: number): Uint8Array {
  const trak = isoFile.getTrackById(trackId)
  const entries = trak.mdia.minf.stbl.stsd.entries as unknown as AacDescriptorEntry[]
  for (const entry of entries) {
    const esds = entry.esds
    if (esds) {
      const decoderConfigDescr = esds.esd.findDescriptor(4)
      const decoderSpecificInfo = decoderConfigDescr?.findDescriptor(5)
      if (decoderSpecificInfo?.data) {
        return decoderSpecificInfo.data
      }
    }
  }
  throw new VideoCompressionError('COMPRESSION_FAILED', '無法解析音訊編碼參數（找不到 esds）')
}

async function pickEncoderCodec(width: number, height: number, bitrate: number, framerate: number): Promise<string> {
  for (const codec of CANDIDATE_ENCODER_CODECS) {
    const support: VideoEncoderSupport = await VideoEncoder.isConfigSupported({ codec, width, height, bitrate, framerate })
    if (support.supported) {
      return codec
    }
  }
  throw new VideoCompressionError('COMPRESSION_FAILED', '此瀏覽器找不到可用的視訊編碼器設定')
}

function parseTracks(isoFile: ISOFile, sourceBuffer: ArrayBuffer): Promise<ParsedTracks> {
  return new Promise<ParsedTracks>((resolve, reject) => {
    const videoSamples: Sample[] = []
    const audioSamples: Sample[] = []
    isoFile.onReady = (info: Movie): void => {
      const videoTrack: Track | undefined = info.videoTracks[0]
      if (!videoTrack || !videoTrack.video) {
        reject(new VideoCompressionError('COMPRESSION_FAILED', '找不到影片視訊軌'))
        return
      }
      const audioTrack: Track | null = info.audioTracks[0] ?? null

      // 必須在同一次 appendBuffer 呼叫內（onReady 觸發當下）設定 extraction options 並呼叫 start()，
      // 否則 mp4box 已經處理過 mdat、之後才設定 extraction options 會永遠等不到 onSamples。
      isoFile.onSamples = (trackId: number, _user: unknown, newSamples: Sample[]): void => {
        if (trackId === videoTrack.id) {
          videoSamples.push(...newSamples)
        } else if (audioTrack && trackId === audioTrack.id) {
          audioSamples.push(...newSamples)
        }
      }
      isoFile.setExtractionOptions(videoTrack.id, undefined, { nbSamples: videoTrack.nb_samples })
      if (audioTrack) {
        isoFile.setExtractionOptions(audioTrack.id, undefined, { nbSamples: audioTrack.nb_samples })
      }
      isoFile.start()
      resolve({ videoTrack, audioTrack, videoSamples, audioSamples })
    }
    isoFile.onError = (_module: string, message: string): void => {
      reject(new VideoCompressionError('COMPRESSION_FAILED', `影片解析失敗：${message}`))
    }
    const mp4boxBuffer: MP4BoxBuffer = MP4BoxBuffer.fromArrayBuffer(sourceBuffer, 0)
    isoFile.appendBuffer(mp4boxBuffer)
    isoFile.flush()
  })
}

export async function compressVideoWebCodecs(
  file: File,
  onProgress?: (ratio: number) => void,
  options?: CompressVideoWebCodecsOptions,
): Promise<File> {
  if (!isWebCodecsSupported()) {
    throw new VideoCompressionError('COMPRESSION_FAILED', '此瀏覽器不支援 WebCodecs')
  }

  const longEdge: number = options?.longEdge ?? DEFAULT_LONG_EDGE
  const bitrate: number = options?.bitrate ?? DEFAULT_BITRATE
  const requestedFps: number | undefined = options?.fps
  const audioBitrate: number = options?.audioBitrate ?? DEFAULT_AUDIO_BITRATE
  const enforceSizeLimit: boolean = options?.enforceSizeLimit ?? true

  let decoder: VideoDecoder | undefined
  let encoder: VideoEncoder | undefined
  let audioDecoder: AudioDecoder | undefined
  let audioEncoder: AudioEncoder | undefined

  try {
    const sourceBuffer: ArrayBuffer = await file.arrayBuffer()
    const isoFile: ISOFile = createFile()

    const { videoTrack, audioTrack, videoSamples, audioSamples } = await parseTracks(isoFile, sourceBuffer)

    const description: Uint8Array = getAvcDescription(isoFile, videoTrack.id)
    const sourceWidth: number = videoTrack.video!.width
    const sourceHeight: number = videoTrack.video!.height
    const { width: targetWidth, height: targetHeight } = computeTargetSize(sourceWidth, sourceHeight, longEdge)
    const needsScale: boolean = targetWidth !== sourceWidth || targetHeight !== sourceHeight
    const rotation: 0 | 90 | 180 | 270 = getRotationDegrees(videoTrack.matrix)

    const durationSeconds: number = videoTrack.timescale > 0 ? videoTrack.duration / videoTrack.timescale : 0
    const sourceFps: number = durationSeconds > 0 ? videoTrack.nb_samples / durationSeconds : 30
    // mp4-muxer 要求 video.frameRate 必須是正整數；實際拍攝影片的平均 fps 常因時間戳記精度而非整數（如 30.0048），四捨五入即可。
    const targetFps: number = Math.max(1, Math.round(requestedFps ?? sourceFps))

    const hasAudio: boolean = audioTrack !== null && audioTrack.audio !== undefined && audioSamples.length > 0
    const audioDescription: Uint8Array | null = hasAudio ? getAacDescription(isoFile, audioTrack!.id) : null

    const target: ArrayBufferTarget = new ArrayBufferTarget()
    const muxer: Muxer<ArrayBufferTarget> = new Muxer({
      target,
      video: { codec: 'avc', width: targetWidth, height: targetHeight, frameRate: targetFps, rotation },
      audio: hasAudio ? { codec: 'aac', numberOfChannels: audioTrack!.audio!.channel_count, sampleRate: audioTrack!.audio!.sample_rate } : undefined,
      fastStart: 'in-memory',
      // 來源影片第一個影格的 cts 不一定精確為 0（例如有初始 B-frame 排序造成的顯示順序偏移），
      // mp4-muxer 預設 strict 模式要求第一個 chunk 時間戳記必須是 0，否則會直接丟出例外。
      firstTimestampBehavior: 'offset',
    })

    const encoderCodec: string = await pickEncoderCodec(targetWidth, targetHeight, bitrate, targetFps)

    let decodeError: unknown = null
    let encodeError: unknown = null
    let audioDecodeError: unknown = null
    let audioEncodeError: unknown = null
    let framesProcessed: number = 0
    const totalSamples: number = videoTrack.nb_samples

    encoder = new VideoEncoder({
      output: (chunk: EncodedVideoChunk, metadata?: EncodedVideoChunkMetadata): void => {
        muxer.addVideoChunk(chunk, metadata)
      },
      error: (error: DOMException): void => {
        encodeError = error
      },
    })
    encoder.configure({ codec: encoderCodec, width: targetWidth, height: targetHeight, bitrate, framerate: targetFps })

    const canvas: OffscreenCanvas | null = needsScale ? new OffscreenCanvas(targetWidth, targetHeight) : null
    const canvasCtx: OffscreenCanvasRenderingContext2D | null = canvas ? canvas.getContext('2d') : null
    if (needsScale && !canvasCtx) {
      throw new VideoCompressionError('COMPRESSION_FAILED', '無法建立縮放用畫布')
    }

    decoder = new VideoDecoder({
      output: (frame: VideoFrame): void => {
        try {
          let frameToEncode: VideoFrame = frame
          if (canvas && canvasCtx) {
            canvasCtx.drawImage(frame, 0, 0, targetWidth, targetHeight)
            frameToEncode = new VideoFrame(canvas, { timestamp: frame.timestamp, duration: frame.duration ?? undefined })
          }
          encoder!.encode(frameToEncode)
          if (frameToEncode !== frame) {
            frameToEncode.close()
          }
          frame.close()
          framesProcessed += 1
          if (onProgress) {
            onProgress(totalSamples > 0 ? Math.min(framesProcessed / totalSamples, 1) : 0)
          }
        } catch (error) {
          decodeError = error
        }
      },
      error: (error: DOMException): void => {
        decodeError = error
      },
    })
    decoder.configure({ codec: videoTrack.codec, description })

    if (hasAudio && audioTrack && audioTrack.audio) {
      audioEncoder = new AudioEncoder({
        output: (chunk: EncodedAudioChunk, metadata?: EncodedAudioChunkMetadata): void => {
          muxer.addAudioChunk(chunk, metadata)
        },
        error: (error: DOMException): void => {
          audioEncodeError = error
        },
      })
      audioEncoder.configure({
        codec: AUDIO_ENCODER_CODEC,
        sampleRate: audioTrack.audio.sample_rate,
        numberOfChannels: audioTrack.audio.channel_count,
        bitrate: audioBitrate,
      })

      audioDecoder = new AudioDecoder({
        output: (audioData: AudioData): void => {
          try {
            audioEncoder!.encode(audioData)
          } finally {
            audioData.close()
          }
        },
        error: (error: DOMException): void => {
          audioDecodeError = error
        },
      })
      audioDecoder.configure({
        codec: audioTrack.codec,
        sampleRate: audioTrack.audio.sample_rate,
        numberOfChannels: audioTrack.audio.channel_count,
        description: audioDescription ?? undefined,
      })
    }

    for (const sample of videoSamples) {
      if (!sample.data) {
        continue
      }
      decoder.decode(
        new EncodedVideoChunk({
          type: sample.is_sync ? 'key' : 'delta',
          timestamp: Math.round((sample.cts / sample.timescale) * 1_000_000),
          duration: Math.round((sample.duration / sample.timescale) * 1_000_000),
          data: sample.data,
        }),
      )
    }

    if (audioDecoder) {
      for (const sample of audioSamples) {
        if (!sample.data) {
          continue
        }
        audioDecoder.decode(
          new EncodedAudioChunk({
            type: sample.is_sync ? 'key' : 'delta',
            timestamp: Math.round((sample.cts / sample.timescale) * 1_000_000),
            duration: Math.round((sample.duration / sample.timescale) * 1_000_000),
            data: sample.data,
          }),
        )
      }
    }

    if (decodeError) {
      throw decodeError
    }
    await decoder.flush()
    if (decodeError) {
      throw decodeError
    }
    await encoder.flush()
    if (encodeError) {
      throw encodeError
    }

    if (audioDecoder && audioEncoder) {
      if (audioDecodeError) {
        throw audioDecodeError
      }
      await audioDecoder.flush()
      if (audioDecodeError) {
        throw audioDecodeError
      }
      await audioEncoder.flush()
      if (audioEncodeError) {
        throw audioEncodeError
      }
    }

    muxer.finalize()

    const data: Uint8Array = new Uint8Array(target.buffer)
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
    for (const codec of [decoder, encoder, audioDecoder, audioEncoder]) {
      try {
        if (codec && codec.state !== 'closed') {
          codec.close()
        }
      } catch {
        // codec may already be in an error state; ignore cleanup failure
      }
    }
  }
}
