# FINAL PATCH NOTES v20

## Account verification document names
- Fixed account verification document display names when the API returns `documentType` as a string such as `NationalId` instead of a numeric enum value.
- The customer/provider document list now correctly displays Arabic labels such as `البطاقة الشخصية` instead of falling back to `مستند آخر`.

## Chat realtime sender name
- Fixed realtime chat messages so the sender name is sent from the backend with the SignalR payload.
- `SendMessageCommandHandler` now resolves the authenticated sender using `UserManager<ApplicationUser>` and sends the real full name in:
  - realtime message payload
  - new message notification text
- This prevents the receiver from seeing `مستخدم` until refreshing the page.

## Build check
- Ran TypeScript check successfully:
  `./node_modules/.bin/tsc -p tsconfig.app.json --noEmit`

## Database changes
- No migration required.
