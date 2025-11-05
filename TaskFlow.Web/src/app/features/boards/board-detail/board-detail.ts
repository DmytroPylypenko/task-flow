import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import { Task } from '../../../models/task.model';
import { TaskReorder } from '../../../models/task-reorder.model';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Column } from '../../../models/column.model';
import { TaskCreate } from '../../../models/task-create.model';
import { Dialog } from '@angular/cdk/dialog';
import { TaskDetailModalComponent } from '../../tasks/task-detail-modal/task-detail-modal';
import { TaskDialogResult } from '../../../models/task-dialog-result.model';
import { FormControl } from '@angular/forms';

/**
 * Displays the details of a single board.
 */
@Component({
  selector: 'app-board-detail',
  imports: [DragDropModule, ReactiveFormsModule],
  templateUrl: './board-detail.html',
  styleUrl: './board-detail.scss',
})
export class BoardDetailComponent {
  private readonly boardService = inject(BoardService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(Dialog);
  private readonly router = inject(Router);

  // --- Component state management ---
  board: Board | null = null;
  isLoading = true;
  errorMessage: string | null = null;

  // A Map to store a FormGroup for each column, keyed by its column ID.
  newTaskForms = new Map<number, FormGroup>();

  // Initialize form control for title and set isEditingTitle flag for UI
  titleControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.maxLength(50)],
  });
  isEditingTitle = false;

  /**
   * Initializes the component and fetches the board details based on the provided ID in the URL.
   * If no ID is provided, displays an error message.
   */
  ngOnInit(): void {
    const boardId: string | null = this.route.snapshot.paramMap.get('id');

    if (boardId) {
      this.boardService.getBoardById(+boardId).subscribe({
        next: (data) => {
          this.board = data;

          // After loading the board, create a form for each column.
          this.board.columns.forEach((column) => {
            this.newTaskForms.set(
              column.id,
              this.fb.group({
                title: ['', [Validators.required, Validators.maxLength(100)]],
              })
            );
          });
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
      // Reorder the tasks locally in the current column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

      // Prepare the payload for the backend — each task ID paired with its new position.
      const reorderPayload: TaskReorder[] = event.container.data.map((task, index) => ({
        taskId: task.id,
        newPosition: index,
      }));

      const columnId = Number(event.container.id);

      // Send the new order to the backend to persist changes.
      this.boardService.reorderTasks(columnId, reorderPayload).subscribe({
        error: (err) => {
          console.error('Failed to reorder tasks', err);

          // Revert the UI change if the server update fails.
          moveItemInArray(event.container.data, event.currentIndex, event.previousIndex);
        },
      });
    } else {
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
        },
      });
    }
  }

  /**
   * Handles creating a new task in a specific column.
   * @param column The column where the new task will be added.
   */
  onCreateTask(column: Column): void {
    // Retrieve the FormGroup for the given column.
    const form = this.newTaskForms.get(column.id);
    if (!form || form.invalid) {
      return;
    }

    // Prepare the payload for the backend API.
    const taskPayload: TaskCreate = {
      title: form.value.title.trim(),
      description: form.value.description,
      columnId: column.id,
    };

    this.boardService.createTask(taskPayload).subscribe({
      next: (newTask) => {
        column.tasks.push(newTask);
        form.reset();
      },
      error: (err) => {
        console.error('Failed to create task', err);
      },
    });
  }

  /**
   * Opens the TaskDetailModalComponent for editing a task.
   * @param task The task to be edited.
   */
  openTaskModal(task: Task): void {
    const dialogRef = this.dialog.open<TaskDialogResult>(TaskDetailModalComponent, {
      width: '500px',
      data: task, // Pass the task data to the modal
    });

    // Subscribe to the modal's close event
    dialogRef.closed.subscribe((result) => {
      // Check if a result was returned (i.e., not canceled)
      if (!result) {
        return;
      }

      switch (result.action) {
        case 'update':
          this.boardService.updateTask(task.id, result.payload).subscribe({
            next: (updatedTaskFromServer) => {
              task.title = updatedTaskFromServer.title;
              task.description = updatedTaskFromServer.description;
            },
            error: (err) => console.error('Failed to update task', err),
          });
          break;

        case 'delete':
          this.boardService.deleteTask(task.id).subscribe({
            next: () => {
              this.removeTaskFromUI(task);
            },
            error: (err) => console.error('Failed to delete task', err),
          });
          break;
      }
    });
  }

  /**
   * Helper method to remove a task from the local board object.
   */
  private removeTaskFromUI(taskToRemove: Task): void {
    if (!this.board) return;

    for (const column of this.board.columns) {
      const index = column.tasks.findIndex((task) => task.id === taskToRemove.id);
      if (index !== -1) {
        column.tasks.splice(index, 1);
        return;
      }
    }
  }

  /**
   * Deletes the current board and navigates back to the board list.
   */
  onDeleteBoard(): void {
    if (!this.board) return;

    // Show a confirmation dialog to the user
    if (
      confirm(
        `Are you sure you want to delete the board "${this.board.name}"? This cannot be undone.`
      )
    ) {
      this.isLoading = true;

      this.boardService.deleteBoard(this.board.id).subscribe({
        next: () => {
          this.router.navigate(['/boards']);
        },
        error: (err) => {
          console.error('Failed to delete board', err);
          this.errorMessage = 'Failed to delete board. Please try again.';
          this.isLoading = false;
        },
      });
    }
  }

  /**
   * Starts editing the title of the board.
   */
  startEditingTitle(): void {
    if (this.board) {
      this.isEditingTitle = true;
      this.titleControl.setValue(this.board.name);
    }
  }

  /**
   * Cancels editing the title of the board.
   */
  cancelEditingTitle(): void {
    this.isEditingTitle = false;
  }

  /**
   * Saves the changes made to the title of the board.
   * Updates the board name and triggers an optimistic update.
   */
  saveTitle(): void {
    if (!this.board || this.titleControl.invalid) {
      this.cancelEditingTitle();
      return;
    }

    const newName = this.titleControl.value.trim();
    if (newName === this.board.name || !newName) {
      this.cancelEditingTitle();
      return;
    }

    const oldName = this.board.name;
    this.board.name = newName;
    this.isEditingTitle = false;

    this.boardService.updateBoard(this.board.id, newName).subscribe({
      error: (err) => {
        console.error('Failed to rename board', err);
        if (this.board) this.board.name = oldName;
      },
    });
  }
}
