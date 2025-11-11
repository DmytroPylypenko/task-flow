import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { LayoutDashboard, LucideAngularModule, Plus } from 'lucide-angular';
import { Dialog } from '@angular/cdk/dialog';
import { CreateBoardModalComponent } from '../create-board-modal/create-board-modal';

/**
 * Displays a list of the user's boards.
 */
@Component({
  selector: 'app-board-list',
  imports: [RouterLink, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './board-list.html',
  styleUrl: './board-list.scss',
})
export class BoardListComponent {
  private readonly boardService = inject(BoardService);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(Dialog);

  readonly LayoutDashboard = LayoutDashboard;
  readonly PlusIcon = Plus;

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
   * Opens the CreateBoardModalComponent for creating a board.
   */
  openCreateModal(): void {
    const dialogRef = this.dialog.open<string>(CreateBoardModalComponent, {
      panelClass: 'custom-modal-panel' 
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        this.boardService.createBoard(result).subscribe({
           next: (newBoard) => this.boards.unshift(newBoard),
           error: (err) => console.error(err)
        });
      }
    });
  }
}
