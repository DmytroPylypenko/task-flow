import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environments';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Board } from '../../models/board.models';

/**
 * BoardService handles board-related operations.
 */
@Injectable({
  providedIn: 'root'
})
export class BoardService {
  private readonly apiUrl = `${environment.apiUrl}/boards`;
  private readonly http = inject(HttpClient);

  /**
   * Fetches all boards for the currently authenticated user.
   * The JWT interceptor will automatically add the auth header.
   */
  getBoards(): Observable<Board[]> {
    return this.http.get<Board[]>(this.apiUrl);
  }
}
