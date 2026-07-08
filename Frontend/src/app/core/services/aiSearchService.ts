import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'

export type AiConversationRole = 'user' | 'assistant'

export interface AiConversationTurn {
  role: AiConversationRole
  content: string
}

export interface AiSearchRequest {
  rawQuery: string
  conversationHistory?: AiConversationTurn[]
  sessionId?: string
}

export interface AiSearchIntent {
  target: 'Ambiguous' | 'ServiceListing' | 'Marketplace' | number
  equipmentType?: string | null
  location?: string | null
  taskDescription?: string | null
  language?: string | number
}

export interface AiClarificationPrompt {
  missingParameters: string[]
  message: string
  language: string | number
}

export interface AiServiceListingSnapshot {
  id: string
  providerProfileId: string
  title: string
  description: string
  categoryId: number
  hourlyRate: number
  dailyRate?: number | null
  providerCompanyName: string
  providerAverageRating: number
  distanceKm?: number | null
}

export interface AiMarketplaceListingSnapshot {
  id: string
  sellerId: string
  title: string
  description: string
  categoryId: number
  price: number
  condition: string
  sellerCompanyName: string
  sellerAverageRating: number
  distanceKm?: number | null
}

export interface AiSearchResultItem {
  serviceListing?: AiServiceListingSnapshot | null
  marketplaceListing?: AiMarketplaceListingSnapshot | null
  similarityScore: number
  matchExplanation: string
}

export interface AiSearchResponse {
  status: 'ResultsReady' | 'ClarificationNeeded' | number
  intent?: AiSearchIntent | null
  results?: AiSearchResultItem[] | null
  hasZeroResults: boolean
  clarification?: AiClarificationPrompt | null
  processingMs: number
}

@Injectable({ providedIn: 'root' })
export class AiSearchService {
  private readonly storageKey = 'heveq_ai_search_results'

  constructor(private http: HttpClient) {}

  search(request: AiSearchRequest) {
    return this.http.post<AiSearchResponse>(`${API_BASE_URL}/public/ai-search`, request)
  }

  saveResults(response: AiSearchResponse): void {
    sessionStorage.setItem(this.storageKey, JSON.stringify(response))
  }

  readResults(): AiSearchResponse | null {
    const raw = sessionStorage.getItem(this.storageKey)
    if (!raw) return null

    try {
      return JSON.parse(raw) as AiSearchResponse
    } catch {
      sessionStorage.removeItem(this.storageKey)
      return null
    }
  }

  clearResults(): void {
    sessionStorage.removeItem(this.storageKey)
  }

  isMarketplaceTarget(target: AiSearchIntent['target'] | undefined): boolean {
    return target === 'Marketplace' || target === 2
  }

  isResultsReady(response: AiSearchResponse): boolean {
    return response.status === 'ResultsReady' || response.status === 0
  }

  needsClarification(response: AiSearchResponse): boolean {
    return response.status === 'ClarificationNeeded' || response.status === 1
  }
}
