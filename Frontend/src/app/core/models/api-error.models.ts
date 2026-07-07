import { HttpErrorResponse } from '@angular/common/http'
import { translateBackendMessage } from '../helpers/errorMessageHelper'

export interface ApiProblemDetails {
  status?: number
  title?: string
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

const FALLBACK_MESSAGE_AR = 'حدث خطأ غير متوقع، حاول مرة أخرى'

export function extractErrorMessage(err: HttpErrorResponse): string {
  const body = err.error as ApiProblemDetails | string | null | undefined

  if (!body) {
    return FALLBACK_MESSAGE_AR
  }

  if (typeof body === 'string') {
    return translateBackendMessage(body)
  }

  if (body.errors) {
    const firstFieldErrors = Object.values(body.errors)[0]
    if (firstFieldErrors?.length) {
      return translateBackendMessage(firstFieldErrors[0])
    }
  }

  return translateBackendMessage(body.detail || body.title || FALLBACK_MESSAGE_AR)
}
