const FALLBACK_AR = 'حدث خطأ غير متوقع، حاول مرة أخرى'

const translations: Array<[RegExp, string]> = [
  [/Booking time is outside service listing availability/i, 'وقت الحجز خارج مواعيد الإتاحة لهذه الخدمة. اختر وقتاً يسمح بانتهاء الحجز قبل نهاية وقت العمل'],
  [/No availability exists for the selected day/i, 'اليوم المحدد غير متاح لهذه الخدمة. اختر يوماً من أيام الإتاحة المعروضة'],
  [/Booking is not allowed on a blackout date/i, 'لا يمكن الحجز في هذا اليوم لأنه غير متاح لدى المزود'],
  [/Minimum booking duration/i, 'مدة الحجز أقل من الحد الأدنى المسموح لهذه الخدمة'],
  [/Booking location is outside provider service zone/i, 'موقع الحجز خارج نطاق خدمة المزود. يجب الموافقة على التكلفة الإضافية خارج النطاق'],
  [/Selected address was not found/i, 'العنوان المحدد غير موجود أو غير تابع لحسابك'],
  [/Selected address must have latitude and longitude/i, 'العنوان المحدد لا يحتوي على إحداثيات الموقع. يرجى تحديث العنوان أو إدخال الموقع يدوياً'],
  [/Governorate and district are required/i, 'يرجى إدخال المحافظة والمنطقة عند عدم استخدام عنوان محفوظ'],
  [/Latitude and longitude are required/i, 'يرجى إدخال إحداثيات الموقع عند عدم استخدام عنوان محفوظ'],
  [/User not found/i, 'المستخدم غير موجود'],
  [/Invalid Token Claims/i, 'بيانات جلسة الدخول غير صحيحة'],
  [/Your account has been deactivated/i, 'تم تعطيل حسابك، يرجى التواصل مع الدعم'],
  [/Invalid Refresh Token/i, 'جلسة الدخول غير صالحة'],
  [/Logged out successfully/i, 'تم تسجيل الخروج بنجاح'],
  [/not found/i, 'العنصر المطلوب غير موجود'],
  [/not authorized|unauthorized|forbidden/i, 'ليس لديك صلاحية لتنفيذ هذا الإجراء'],
  [/validation error/i, 'بيانات غير صحيحة'],
  [/required/i, 'هذا الحقل مطلوب'],
  [/cannot/i, 'لا يمكن تنفيذ هذا الإجراء بالحالة الحالية'],
  [/already/i, 'هذا الإجراء تم تنفيذه من قبل'],
  [/invalid/i, 'القيمة المدخلة غير صحيحة'],
  [/must be/i, 'القيمة المدخلة لا توافق الشروط المطلوبة'],
  [/minimum requirements/i, 'لا يمكن الموافقة لأن البيانات المطلوبة غير مكتملة'],
  [/photos/i, 'الصور'],
  [/operator/i, 'المشغل'],
  [/availability/i, 'مواعيد الإتاحة'],
  [/An unexpected error occurred/i, FALLBACK_AR]
]

export function translateBackendMessage(message: string | null | undefined): string {
  if (!message) return FALLBACK_AR

  const trimmed = String(message).trim()
  if (!trimmed) return FALLBACK_AR

  // Keep Arabic messages as-is.
  if (/[\u0600-\u06FF]/.test(trimmed)) return trimmed

  const exact = translations.find(([pattern]) => pattern.test(trimmed))
  if (exact) return exact[1]

  return FALLBACK_AR
}

export function getErrorMessage(error: any, fallback: string = FALLBACK_AR): string {
  const response = error?.error

  if (!response) {
    return fallback
  }

  if (typeof response === 'string') {
    return translateBackendMessage(response) || fallback
  }

  if (
    response.exceptionMessage &&
    (response.message === 'An unexpected error occurred.' || response.message === FALLBACK_AR)
  ) {
    return translateBackendMessage(response.exceptionMessage)
  }

  if (response.exceptionMessage) {
    return translateBackendMessage(response.exceptionMessage)
  }

  if (response.message) {
    return translateBackendMessage(response.message)
  }

  if (response.Message) {
    return translateBackendMessage(response.Message)
  }

  if (response.detail) {
    return translateBackendMessage(response.detail)
  }

  if (response.title) {
    return translateBackendMessage(response.title)
  }

  if (response.errors) {
    const errors = response.errors

    if (Array.isArray(errors)) {
      return errors.map(x => translateBackendMessage(String(x))).join('، ')
    }

    if (typeof errors === 'object') {
      const message = Object.values(errors)
        .flat()
        .map(x => translateBackendMessage(String(x)))
        .join('، ')
      return message || fallback
    }
  }

  return fallback
}
