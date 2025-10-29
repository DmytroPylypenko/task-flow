import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

/**
 * Displays a list of the user's boards.
 */
@Component({
  selector: 'app-board-list',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './board-list.html',
  styleUrl: './board-list.scss',
})
export class BoardListComponent {
  private readonly boardService = inject(BoardService);
  private readonly fb = inject(FormBuilder);

  // --- Component state management ---
  boards: Board[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  // --- Form for creating a new board ---
  newBoardForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });

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
      },
    });
  }

  /**
   * Handles the submission of the new board form.
   */
  onCreateBoard(): void {
    if (this.newBoardForm.invalid) {
      return;
    }

    const boardName = this.newBoardForm.value.name?.trim();
    if (!boardName) {
      return;
    }

    this.isLoading = true;

    this.boardService.createBoard(boardName).subscribe({
      next: (newBoard) => {
        this.boards.unshift(newBoard);
        this.newBoardForm.reset();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to create board', err);
        this.isLoading = false;
      },
    });
  }
}
