import { ApplicationRef, NgZone, inject } from '@angular/core'
import { HttpInterceptorFn } from '@angular/common/http'
import { finalize } from 'rxjs'

// Some pages in this project use standalone Angular without Zone.js.
// This interceptor schedules one safe UI tick after every HTTP request so
// loading states and server results render immediately without requiring a click.
export const uiRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const appRef = inject(ApplicationRef)
  const zone = inject(NgZone)

  return next(req).pipe(
    finalize(() => {
      zone.run(() => {
        queueMicrotask(() => {
          try {
            appRef.tick()
          } catch {
            // Ignore ticks attempted while Angular is already checking the view.
          }
        })

        setTimeout(() => {
          try {
            appRef.tick()
          } catch {
            // Ignore ticks attempted while Angular is already checking the view.
          }
        }, 50)
      })
    })
  )
}
