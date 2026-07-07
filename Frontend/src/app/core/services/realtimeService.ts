import { Injectable, NgZone } from '@angular/core'
import * as signalR from '@microsoft/signalr'
import { Subject } from 'rxjs'
import { REALTIME_HUB_URL } from '../constants/api.constants'
import { NotificationItem } from '../models/notificationModels'
import { RealtimeMessage } from '../models/realtimeModels'
import { NotificationsService } from './notificationsService'
import { TokenStorage } from './token-storage'

@Injectable({
  providedIn: 'root'
})
export class RealtimeService {
  private connection?: signalR.HubConnection
  private isConnecting = false

  private messageReceivedSubject = new Subject<RealtimeMessage>()
  messageReceived$ = this.messageReceivedSubject.asObservable()

  constructor(
    private tokenStorage: TokenStorage,
    private notificationsService: NotificationsService,
    private ngZone: NgZone
  ) {}

  connect(): void {
    const token = this.tokenStorage.getAccessToken()

    if (!token) {
      return
    }

    if (
      this.connection &&
      this.connection.state === signalR.HubConnectionState.Connected
    ) {
      return
    }

    if (this.isConnecting) {
      return
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(REALTIME_HUB_URL, {
        accessTokenFactory: () => this.tokenStorage.getAccessToken() ?? ''
      })
      .withAutomaticReconnect()
      .build()

    this.registerHandlers()

    this.isConnecting = true

    this.ngZone.runOutsideAngular(() => {
      this.connection!
        .start()
        .then(() => {
          this.ngZone.run(() => {
            this.isConnecting = false
            console.log('SignalR connected')
          })
        })
        .catch(error => {
          this.ngZone.run(() => {
            this.isConnecting = false
            console.error('SignalR connection failed', error)
          })
        })
    })

    this.connection.onreconnected(() => {
      this.ngZone.run(() => {
        this.notificationsService.loadUnreadCount()
      })
    })
  }

  disconnect(): void {
    if (!this.connection) {
      return
    }

    this.connection.off('ReceiveNotification')
    this.connection.off('ReceiveMessage')
    this.connection.off('Pong')

    this.connection.stop().catch(() => {})
    this.connection = undefined
    this.isConnecting = false
  }

  ping(): void {
    if (!this.connection) {
      return
    }

    if (this.connection.state !== signalR.HubConnectionState.Connected) {
      return
    }

    this.connection.invoke('Ping').catch(() => {})
  }

  private registerHandlers(): void {
    if (!this.connection) {
      return
    }

    this.connection.on('ReceiveNotification', (notification: NotificationItem) => {
      this.ngZone.run(() => {
        console.log('ReceiveNotification', notification)
        this.notificationsService.pushRealtimeNotification(notification)
      })
    })

    this.connection.on('ReceiveMessage', (message: RealtimeMessage) => {
      this.ngZone.run(() => {
        console.log('ReceiveMessage', message)
        this.messageReceivedSubject.next(message)
      })
    })

    this.connection.on('Pong', serverTime => {
      this.ngZone.run(() => {
        console.log('SignalR Pong', serverTime)
      })
    })
  }
}