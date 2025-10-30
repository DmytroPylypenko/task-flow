import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';
import { CdkDragDrop, moveItemInArray, transferArrayItem, DragDropModule } from '@angular/cdk/drag-drop';
import { Task } from '../../../models/task.model';

/**
 * Displays the details of a single board.
 */
@Component({
  selector: 'app-board-detail',
  imports: [DragDropModule],
  templateUrl: './board-detail.html',
  styleUrl: './board-detail.scss',
})
export class BoardDetailComponent {
  private readonly boardService = inject(BoardService);
  private readonly route = inject(ActivatedRoute);

  // --- Component state management ---
  board: Board | null = null;
  isLoading = true;
  errorMessage: string | null = null;

  /**
   * Initializes the component and fetches the board details based on the provided ID in the URL.
   * If no ID is provided, displays an error message.
   */
  ngOnInit(): void {
    const boardId : string | null = this.route.snapshot.paramMap.get('id');

    if (boardId) {
      this.boardService.getBoardById(+boardId).subscribe({
        next: (data) => {
          this.board = data;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = 'Failed to load board details.';
          console.error('Failed to fetch board', err);
        },
        complete: () => {
          this.isLoading = false;
        },
      });
    } else {
      this.errorMessage = 'Board ID not found in URL.';
      this.isLoading = false;
    }
  }

  /**
   * Handles the drop event when a task is moved to a new list.
   * @param event The CdkDragDrop event.
   */
  drop(event: CdkDragDrop<Task[]>) {
    // Case 1: The task is dropped within the same column
    if (event.previousContainer === event.container) {
      // moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }

    // Case 2: The task is moved to a different column.
    const taskToMove = event.previousContainer.data[event.previousIndex];
    const newColumnId = Number(event.container.id); // Drop list ID = column ID

    // Update the UI instantly for a smooth user experience.
    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );

    // Notify backend of the column change.
    this.boardService.moveTask(taskToMove.id, newColumnId).subscribe({
      error: (err) => {
        console.error('Failed to move task', err);
        
        // If the API call fails, revert the UI change. 
        transferArrayItem(
          event.container.data,
          event.previousContainer.data,
          event.currentIndex,
          event.previousIndex
        );
      }
    });
  }
}
