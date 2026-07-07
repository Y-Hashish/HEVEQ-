# Frontend Implementation Guide (AI Target)

This guide provides the instructions needed for an AI Frontend Agent to implement the client-side user interface.

## Inferred Application Pages & Routes

```
/
├── auth/
│   ├── login                  --> Credentials input, stores JWT in LocalStorage/HttpOnly
│   └── register               --> Registers as Customer or Provider
│
├── customer/
│   ├── dashboard              --> Summary of bookings, trust score, search query panel
│   ├── bookings/
│   │   ├── :id                --> Detailed tracking of job lifecycle, chat shortcut
│   │   └── create             --> Booking request form (governorate, street, dates, contact)
│   └── marketplace/
│       └── orders/            --> Buyer marketplace order list and status
│
├── provider/
│   ├── dashboard              --> Earnings graph, active schedules, response SLA alerts
│   ├── listings/
│   │   ├── create             --> Service listing form (hourly rate, equipment specifications)
│   │   └── manage/:id         --> Link operators, edit availability slots, add blackout dates
│   └── bookings/              --> Incoming booking requests (Accept / Reject triggers)
│
├── admin/
│   ├── dashboard              --> System metrics, reindexing controller, user activations
│   ├── verification/          --> Document auditing interface (tax cards, registers)
│   └── disputes/              --> Escrow freeze control, field verification dispatching
│
└── search/                    --> Semantic AI search panel with conversational turns
```

## Stripe Checkout Redirect Flow
When a customer confirms a booking or time adjustment requiring payment:
1. Call `/api/bookings/{bookingId}/checkout` (or corresponding payment checkout endpoint).
2. The response will contain the `checkoutUrl` (hosted Stripe Checkout session).
3. Redirect the browser: `window.location.href = response.checkoutUrl;`.
4. Stripe handles card entries and redirects the user back to the success route configured in options.
5. The application confirms capture via standard webhooks or page landing confirmation calls (`/api/bookings/{bookingId}/confirm-payment`).

## Real-Time Integration (SignalR)
- **Hub Endpoint**: `/hubs/realtime`
- **Handshake Connection**: Append `access_token` query parameter:
  ```typescript
  const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/hubs/realtime?access_token=" + jwtToken)
      .build();
  ```
- **Events to Subscribe to**:
  - `ReceiveNotification`: Listens for dynamic application updates (e.g. "Booking Confirmed", "Dispute Opened").
  - `ReceiveMessage`: Real-time chat messaging event within conversations.

## State Management Suggestions
- **User Session**: Hold user details (JWT, name, role) in a global state context (e.g. NgRx, Redux, or React Context).
- **Active Chat**: Keep a buffer of messages sorted by timestamp. Push incoming `ReceiveMessage` payloads directly to the buffer.
