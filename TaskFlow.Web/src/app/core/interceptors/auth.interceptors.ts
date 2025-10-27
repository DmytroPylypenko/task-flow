import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Intercepts outgoing HTTP requests to attach the JWT Authorization header.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authToken = localStorage.getItem('authToken');

  if (!authToken) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${authToken}`,
    },
  });

  return next(authReq);
};
