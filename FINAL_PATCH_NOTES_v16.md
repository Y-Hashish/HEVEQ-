# FINAL PATCH NOTES v16

## AI Search UI
- Removed the large AI/search input blocks from Marketplace and Services hero sections.
- Added a global AI search field in the Navbar.
- The Navbar search calls `POST /api/public/ai-search`.
- If the AI needs clarification, a chat-style modal opens and keeps the conversation history on the frontend.
- When the AI returns results, the response is stored in session storage and the user is routed to:
  - `/services?ai=1` for service/rental results.
  - `/marketplace?ai=1` for marketplace results.
- In AI-results mode, Services and Marketplace pages display only the AI-recommended items and hide normal filters/pagination/hero search blocks.

## OCR Prompt Update
- Replaced `DocumentOcrJob.cs` with the uploaded updated version.
- Replaced `GptVisionDocumentExtractor.cs` with the uploaded updated version.
- The new OCR flow writes AI admin notes to `AdminNote` and keeps `FailureReason` reserved for admin rejection reasons.

## Backend Search Error Handling
- Removed the direct English try/catch response from `SearchController`.
- Empty AI search query now returns an Arabic bad request message.
- Unexpected search errors are left to the global Arabic error middleware.

## Verification
- Ran TypeScript check successfully after installing frontend dependencies locally in the work directory:
  `./node_modules/.bin/tsc -p tsconfig.app.json --noEmit`
- `ng build` started but exceeded the execution timeout in this environment without showing a compile error.
- No EF migration added.
