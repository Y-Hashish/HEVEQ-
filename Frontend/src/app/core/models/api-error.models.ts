import { HttpErrorResponse } from '@angular/common/http'

/**
 * Mirrors the ProblemDetails shape returned by our GlobalExceptionHandler.
 * `errors` is only present when the backend threw our ValidationException
 * (400) — NotFoundException (404) and ForbiddenAccessException (403) only
 * ever populate `detail`.
 */
export interface ApiProblemDetails {
  status?: number
  title?: string
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

const FALLBACK_MESSAGE_AR = 'حدث خطأ غير متوقع، حاول مرة أخرى'

/**
 * Extracts a single displayable Arabic-friendly message from an HttpErrorResponse.
 * Backend error bodies are always ProblemDetails JSON here (never a raw string),
 * unlike the simplified `error.error` pattern used in the existing Auth flows —
 * this is deliberately more defensive since our DTOs always carry `detail`/`errors`.
 */
export function extractErrorMessage(err: HttpErrorResponse): string {
  const body = err.error as ApiProblemDetails | string | null | undefined

  if (!body) {
    return FALLBACK_MESSAGE_AR
  }

  if (typeof body === 'string') {
    return body
  }

  if (body.errors) {
    const firstFieldErrors = Object.values(body.errors)[0]
    if (firstFieldErrors?.length) {
      return firstFieldErrors[0]
    }
  }

  return body.detail || body.title || FALLBACK_MESSAGE_AR
}
