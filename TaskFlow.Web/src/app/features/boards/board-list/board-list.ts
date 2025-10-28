import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.models';

/**
 * Displays a list of the user's boards.
 */
@Component({
  selector: 'app-board-list',
  imports: [],
  templateUrl: './board-list.html',
  styleUrl: './board-list.scss',
})
export class BoardListComponent {
  private readonly boardService = inject(BoardService);

  // --- Component state management ---
  boards: Board[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  /**
   * Fetches the user's boards when the component is initialized.
   */
  ngOnInit(): void {
    this.boardService.getBoards().subscribe({
      next: (data) => {
        this.boards = data;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load boards. Please try again later.';
        console.error('Failed to fetch boards', err);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
