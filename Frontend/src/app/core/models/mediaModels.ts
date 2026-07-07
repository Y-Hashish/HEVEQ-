export interface ImageUploadResponse {
  url: string
  publicId: string
  folder: string
  format: string
  width: number
  height: number
  bytes: number
  isPublic: boolean
  message: string
}