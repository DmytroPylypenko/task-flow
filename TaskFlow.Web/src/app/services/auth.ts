import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environments';
import { UserLogin, UserRegister } from '../models/user-credentials.model';
import { AuthToken } from '../models/token.model';

/**
 * AuthService class provides methods for user authentication.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/Auth`;

  private readonly http = inject(HttpClient);

  /**
   * Sends a registration request to the API.
   * @param userRegisterData The user's registration details.
   */
  register(userRegisterData: UserRegister): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, userRegisterData);
  }

  /**
   * Sends a login request to the API.
   * @param userLoginData The user's login credentials.
   * @returns An Observable that emits an AuthToken if the login is successful.
   */
  login(userLoginData: UserLogin): Observable<AuthToken> {
    return this.http.post<AuthToken>(`${this.apiUrl}/login`, userLoginData);
  }
}
