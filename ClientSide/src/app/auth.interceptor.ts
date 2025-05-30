import { HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const token = authService.getLocalToken();

  // Clone the request and attach the token if available
  const modifiedReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(modifiedReq).pipe(
    catchError((error: HttpErrorResponse): Observable<never> => {
      // 403 (Forbidden)
      if (error.status === HttpStatusCode.Forbidden) {
        router.navigate(['/']);
      }

      // 401 (Unauthorized)
      if (error.status === HttpStatusCode.Unauthorized) {
        authService.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
