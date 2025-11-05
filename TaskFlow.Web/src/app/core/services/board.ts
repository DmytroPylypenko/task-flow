import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environments';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Board } from '../../models/board.model';
import { TaskReorder } from '../../models/task-reorder.model';
import { TaskCreate } from '../../models/task-create.model';
import { Task } from '../../models/task.model';
import { TaskUpdate } from '../../models/task-update.model';

/**
 * BoardService handles board-related operations.
 */
@Injectable({
  providedIn: 'root',
})
export class BoardService {
  private readonly boardsApiUrl = `${environment.apiUrl}/boards`;
  private readonly tasksApiUrl = `${environment.apiUrl}/tasks`;
  private readonly columnsApiUrl = `${environment.apiUrl}/columns`;
  private readonly http = inject(HttpClient);

  // ========================================
  // Boards Region
  // ========================================

  /**
   * Fetches all boards for the currently authenticated user.
   * The JWT interceptor will automatically add the auth header.
   */
  getBoards(): Observable<Board[]> {
    return this.http.get<Board[]>(this.boardsApiUrl);
  }

  /**
   * Fetches a single board by its ID, including its columns and tasks.
   * @param id The ID of the board to fetch.
   */
  getBoardById(id: number): Observable<Board> {
    return this.http.get<Board>(`${this.boardsApiUrl}/${id}`);
  }

  /**
   * Creates a new board for the authenticated user.
   * @param boardName The name of the new board.
   */
  createBoard(boardName: string): Observable<Board> {
    return this.http.post<Board>(this.boardsApiUrl, { name: boardName });
  }

  /**
   * Updates an existing board's name.
   * @param boardId The ID of the board to update.
   * @param newName The new name for the board.
   */
  updateBoard(boardId: number, newName: string): Observable<Board> {
    return this.http.put<Board>(`${this.boardsApiUrl}/${boardId}`, { name: newName });
  }

  /**
   * Deletes a board from the database.
   * @param boardId The ID of the board to delete.
   */
  deleteBoard(boardId: number): Observable<void> {
    return this.http.delete<void>(`${this.boardsApiUrl}/${boardId}`);
  }

  // ========================================
  // Tasks Region
  // ========================================

  /**
   * Moves a task to a different column.
   * @param taskId The ID of the task to move.
   * @param newColumnId The ID of the destination column.
   */
  moveTask(taskId: number, newColumnId: number): Observable<void> {
    const moveUrl = `${this.tasksApiUrl}/${taskId}/move`;
    return this.http.patch<void>(moveUrl, { newColumnId });
  }

  /**
   * Updates the position of tasks within a single column.
   * @param columnId The ID of the column being reordered.
   * @param payload The array of tasks with their new positions.
   */
  reorderTasks(columnId: number, payload: TaskReorder[]): Observable<void> {
    const reorderUrl = `${this.columnsApiUrl}/${columnId}/reorder`;
    return this.http.patch<void>(reorderUrl, payload);
  }

  /**
   * Creates a new task in a specific column.
   * @param payload The data for the new task.
   */
  createTask(payload: TaskCreate): Observable<Task> {
    return this.http.post<Task>(this.tasksApiUrl, payload);
  }

  /**
   * Updates an existing task's title and description.
   * @param taskId The ID of the task to update.
   * @param payload The new title and description.
   */
  updateTask(taskId: number, payload: TaskUpdate): Observable<Task> {
    return this.http.put<Task>(`${this.tasksApiUrl}/${taskId}`, payload);
  }

  /**
   * Deletes a task from the database.
   * @param taskId The ID of the task to delete.
   */
  deleteTask(taskId: number): Observable<void> {
    return this.http.delete<void>(`${this.tasksApiUrl}/${taskId}`);
  }
}
