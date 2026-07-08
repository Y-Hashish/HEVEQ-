export interface ExtractedCoordinates {
  latitude: number
  longitude: number
}

export function extractCoordinatesFromMapLink(value: string | null | undefined): ExtractedCoordinates | null {
  const text = String(value || '').trim()
  if (!text) {
    return null
  }

  const decoded = safeDecode(text)

  const patterns = [
    /@(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/,
    /[?&](?:q|query|ll)=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/i,
    /!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)/i,
    /(?:^|[^\d-])(-?\d{1,2}\.\d{4,})\s*,\s*(-?\d{1,3}\.\d{4,})(?:$|[^\d.])/,
    /(?:lat|latitude)[=:]\s*(-?\d+(?:\.\d+)?).*?(?:lng|lon|longitude)[=:]\s*(-?\d+(?:\.\d+)?)/i
  ]

  for (const pattern of patterns) {
    const match = decoded.match(pattern)
    if (!match) {
      continue
    }

    const latitude = Number(match[1])
    const longitude = Number(match[2])

    if (isValidCoordinate(latitude, longitude)) {
      return { latitude, longitude }
    }
  }

  return null
}

export function isValidCoordinate(latitude: number, longitude: number): boolean {
  return Number.isFinite(latitude) &&
    Number.isFinite(longitude) &&
    latitude >= -90 && latitude <= 90 &&
    longitude >= -180 && longitude <= 180
}

function safeDecode(value: string): string {
  try {
    return decodeURIComponent(value)
  } catch {
    return value
  }
}
