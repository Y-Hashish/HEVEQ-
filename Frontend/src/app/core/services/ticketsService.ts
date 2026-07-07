import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  AddTicketMessageRequest,
  AddTicketMessageResponse,
  CreateTicketRequest,
  CreateTicketResponse,
  MyTicketsResponse,
  TicketDetails
} from '../models/ticketModels'

@Injectable({
  providedIn: 'root'
})
export class TicketsService {
  constructor(private http: HttpClient) {}

  getMyTickets() {
    return this.http.get<MyTicketsResponse>(`${API_BASE_URL}/tickets/my`)
  }

  getTicketDetails(id: string) {
    return this.http.get<TicketDetails>(`${API_BASE_URL}/tickets/${id}`)
  }

  createTicket(request: CreateTicketRequest) {
    return this.http.post<CreateTicketResponse>(
      `${API_BASE_URL}/tickets`,
      request
    )
  }

  addMessage(ticketId: string, request: AddTicketMessageRequest) {
    return this.http.post<AddTicketMessageResponse>(
      `${API_BASE_URL}/tickets/${ticketId}/messages`,
      request
    )
  }
}