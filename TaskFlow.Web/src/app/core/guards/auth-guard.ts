import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

/**
 * A route guard that prevents unauthenticated users from accessing protected routes.
 * If no valid auth token is found, it redirects the user to the login page.
 */
export const authGuard: CanActivateFn = () => {
  const router = inject(Router);

  const authToken = localStorage.getItem('authToken');

  if (authToken) {
    return true;
  } else {
    router.navigate(['/login']);
    return false;
  }
};
