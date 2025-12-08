import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BoardService } from '../../../core/services/board';
import { Board } from '../../../models/board.model';
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
  DragDropModule,
} from '@angular/cdk/drag-drop';
import { LucideAngularModule, Plus, Ellipsis, ArrowLeft, GripVertical, Edit, Trash2 } from 'lucide-angular';
import { Task } from '../../../models/task.model';
import { TaskReorder } from '../../../models/task-reorder.model';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Column } from '../../../models/column.model';
import { TaskCreate } from '../../../models/task-create.model';
import { Dialog } from '@angular/cdk/dialog';
import { TaskDetailModalComponent } from '../../tasks/task-detail-modal/task-detail-modal';
import { TaskDialogResult } from '../../../models/task-dialog-result.model';
import { FormControl } from '@angular/forms';
import { CreateColumnModalComponent } from '../create-column-modal/create-column-modal';
import { OnInit } from '@angular/core';
/**
 * Displays the details of a single board.
 */
@Component({
  selector: 'app-board-detail',
  imports: [DragDropModule, ReactiveFormsModule, LucideAngularModule, RouterLink],
  templateUrl: './board-detail.html',
  styleUrl: './board-detail.scss',
})
export class BoardDetailComponent implements OnInit {
  private readonly boardService = inject(BoardService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(Dialog);
  private readonly router = inject(Router);

  readonly PlusIcon = Plus;
  readonly MoreIcon = Ellipsis;
  readonly ArrowLeftIcon = ArrowLeft;
  readonly GripVerticalIcon = GripVertical;
  readonly EditIcon = Edit;
  readonly TrashIcon = Trash2;

  // Holds a list of all column IDs for the CDK to connect the drop lists.
  public columnIds: string[] = [];

  // --- Component state management ---
  board: Board | null = null;
  isLoading = true;
  errorMessage: string | null = null;

  // A Map to store a FormGroup for each column, keyed by its column ID.
  newTaskForms = new Map<number, FormGroup>();

  // Tracks which column's "Add Task" form is currently visible
  addingTaskToColumnId: number | null = null;

  // Tracks which column's action menu is currently open.
  openColumnMenuId: number | null = null;

  // Initialize form control for title and set isEditingTitle flag for UI
  titleControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.maxLength(50)],
  });
  isEditingTitle = false;

  // Track which column is being edited and hold the temporary name
  editingColumnId: number | null = null;
  editColumnNameControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(50)],
  });

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

          // When the board loads, create the list of IDs 
          this.columnIds = this.board.columns.map(c => c.id.toString());

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
   * Toggles the action menu for a specific column.
   * Stops event propagation to prevent the main click handler from closing it.
   * @param event The mouse event that triggered the toggle.
   * @param columnId The ID of the column to toggle the action menu for.
   */
  toggleColumnMenu(event: MouseEvent, columnId: number): void {
    event.stopPropagation(); // Prevent the main click handler from closing it
    this.openColumnMenuId = this.openColumnMenuId === columnId ? null : columnId;
  }

  /**
   * Closes all open menus or forms.
   * This is called by the main container's click event.
   */
  closeAllPopups(): void {
    this.openColumnMenuId = null;
    this.addingTaskToColumnId = null;
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
   * Shows the 'Add Task' form for a specific column.
   * @param columnId The ID of the column to add the task to.
   */
  showAddTaskForm(columnId: number): void {
    this.addingTaskToColumnId = columnId;
  }

  /**
   * Hides the 'Add Task' form.
   * @param columnId The ID of the column to cancel the task addition.
   */
  cancelAddTask(columnId: number): void {
    this.addingTaskToColumnId = null;
    this.newTaskForms.get(columnId)?.reset();
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
        this.addingTaskToColumnId = null;
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

  /**
   * Opens the CreateColumnModalComponent for creating a column.
   */
  openCreateColumnModal(): void {
    const dialogRef = this.dialog.open<string>(CreateColumnModalComponent, {
      panelClass: 'custom-modal-panel',
    });

    dialogRef.closed.subscribe((name) => {
      if (name && this.board) {
        this.boardService.createColumn(this.board.id, name).subscribe({
          next: (newColumn) => {
            newColumn.tasks = [];
            this.board?.columns.push(newColumn);

            this.newTaskForms.set(
              newColumn.id,
              this.fb.group({
                title: ['', Validators.required],
              })
            );
          },
          error: (err) => console.error('Failed to create column', err),
        });
      }
    });
  }

  /**
   * Enables edit mode for a specific column.
   */
  startEditingColumn(column: Column): void {
    this.editingColumnId = column.id;
    this.editColumnNameControl.setValue(column.name);
    this.openColumnMenuId = null;
  }

  /**
   * Updates the column name.
   */
  onUpdateColumnName(column: Column): void {
    if (this.editingColumnId !== column.id || this.editColumnNameControl.invalid) {
      this.editingColumnId = null;
      return;
    }

    const newName = this.editColumnNameControl.value.trim();

    if (!newName || newName === column.name) {
      this.editingColumnId = null;
      return;
    }

    const oldName = column.name;
    column.name = newName;
    this.editingColumnId = null;

    this.boardService.updateColumn(column.id, newName).subscribe({
      error: (err) => {
        console.error('Failed to update column name', err);
        column.name = oldName;
      },
    });
  }

  /**
   * Deletes a column after confirmation.
   */
  onDeleteColumn(column: Column): void {
    this.openColumnMenuId = null; // Close the menu
    if (!confirm(`Are you sure you want to delete "${column.name}" and all its tasks?`)) return;

    this.boardService.deleteColumn(column.id).subscribe({
      next: () => {
        if (this.board) {
          this.board.columns = this.board.columns.filter((c) => c.id !== column.id);
        }
      },
      error: (err) => console.error('Failed to delete column', err),
    });
  }
}
