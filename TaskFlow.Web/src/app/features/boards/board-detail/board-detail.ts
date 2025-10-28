import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';

/**
 * Displays the details of a single board.
 */
@Component({
  selector: 'app-board-detail',
  imports: [],
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
}
