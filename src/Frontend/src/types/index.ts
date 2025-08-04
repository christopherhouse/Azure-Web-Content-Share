export interface User {
  id: string
  email: string
  name: string
  role: UserRole
  createdAt: string
  lastLoginAt?: string
}

export enum UserRole {
  Administrator = 'Administrator',
  ContentOwner = 'ContentOwner'
}

export interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  accessToken: string | null
}

export interface ShareFileRequest {
  recipientEmail: string
  expirationDays: number
  message?: string
}

export interface ShareFileResponse {
  shareCode: string
  expiresAt: string
  downloadUrl: string
}

export interface FileShareMetadata {
  id: string
  fileName: string
  originalFileName: string
  fileSize: number
  contentType: string
  shareCode: string
  recipientEmail: string
  createdBy: string
  createdAt: string
  expiresAt: string
  lastAccessedAt?: string
  isActive: boolean
  message?: string
}

export interface AccessFileRequest {
  shareCode: string
}

export interface ApiError {
  title: string
  status: number
  detail: string
  traceId?: string
}

export interface SiteStatusResponse {
  isClaimed: boolean
  siteName: string
}

export interface ClaimSiteResponse {
  success: boolean
  message: string
  claimedAt: string
}