export function getErrorMessage(error: any, fallback: string): string {
  const response = error?.error

  if (!response) {
    return fallback
  }

  if (typeof response === 'string') {
    return response
  }

  if (
    response.exceptionMessage &&
    response.message === 'An unexpected error occurred.'
  ) {
    return response.exceptionMessage
  }

  if (response.exceptionMessage) {
    return response.exceptionMessage
  }

  if (response.message) {
    return response.message
  }

  if (response.Message) {
    return response.Message
  }

  if (response.errors) {
    const errors = response.errors

    if (Array.isArray(errors)) {
      return errors.join('، ')
    }

    if (typeof errors === 'object') {
      return Object.values(errors)
        .flat()
        .join('، ')
    }
  }

  return fallback
}