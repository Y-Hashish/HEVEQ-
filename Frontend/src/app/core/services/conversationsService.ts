import { HttpClient, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  ConversationListResponse,
  ConversationMessagesResponse,
  MarkConversationReadResponse,
  SendConversationMessageRequest,
  SendConversationMessageResponse,
  StartConversationRequest,
  StartConversationResponse
} from '../models/conversationModels'

@Injectable({
  providedIn: 'root'
})
export class ConversationsService {
  constructor(private http: HttpClient) {}

  getMyConversations() {
    return this.http.get<ConversationListResponse>(
      `${API_BASE_URL}/conversations/my`
    )
  }

  startConversation(request: StartConversationRequest) {
    return this.http.post<StartConversationResponse>(
      `${API_BASE_URL}/conversations`,
      request
    )
  }

  getMessages(conversationId: string, page = 1, pageSize = 50) {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())

    return this.http.get<ConversationMessagesResponse>(
      `${API_BASE_URL}/conversations/${conversationId}/messages`,
      { params }
    )
  }

  sendMessage(conversationId: string, request: SendConversationMessageRequest) {
    return this.http.post<SendConversationMessageResponse>(
      `${API_BASE_URL}/conversations/${conversationId}/messages`,
      request
    )
  }

  markRead(conversationId: string) {
    return this.http.patch<MarkConversationReadResponse>(
      `${API_BASE_URL}/conversations/${conversationId}/read`,
      {}
    )
  }
}