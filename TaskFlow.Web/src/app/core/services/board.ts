import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environments';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Board } from '../../models/board.model';

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

  /**
   * Fetches a single board by its ID, including its columns and tasks.
   * @param id The ID of the board to fetch.
   */
  getBoardById(id: number): Observable<Board> {
    return this.http.get<Board>(`${this.apiUrl}/${id}`);
  }
}
