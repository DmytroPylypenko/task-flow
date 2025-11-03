import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { Task } from '../../../models/task.model';
import { TaskUpdate } from '../../../models/task-update.model';
import { TaskDialogResult } from '../../../models/task-dialog-result.model';
/**
 * Modal component for editing task details.
 * Receives a Task via DIALOG_DATA and returns a TaskUpdate on save.
 */
@Component({
  selector: 'app-task-detail-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './task-detail-modal.html',
  styleUrl: './task-detail-modal.scss',
})
export class TaskDetailModalComponent {
  private readonly fb = inject(FormBuilder);

  // DialogRef is used to close the modal and return the TaskUpdate payload.
  private readonly dialogRef = inject(DialogRef<TaskDialogResult>);
  private readonly task: Task = inject(DIALOG_DATA);

  /**
   * Reactive form for editing task details.
   * Initialized immediately using the injected task data.
   */
  readonly editTaskForm = this.fb.nonNullable.group({
    title: [this.task.title, [Validators.required, Validators.maxLength(100)]],
    description: [this.task.description ?? '', [Validators.maxLength(500)]],
  });

  /**
   * Saves the updated task details and closes the dialog.
   * Emits the updated form value back to the parent component.
   */
  save(): void {
    if (this.editTaskForm.invalid) {
      this.editTaskForm.markAllAsTouched();
      return;
    }

    const payload: TaskUpdate = this.editTaskForm.value as TaskUpdate;
    this.dialogRef.close({ action: 'update', payload });
  }

  /**
   * Closes the dialog with the 'delete' action.
   */
  delete(): void {
    this.dialogRef.close({ action: 'delete' });
  }

  /**
   * Closes the dialog without saving any changes.
   */
  cancel(): void {
    this.dialogRef.close();
  }
}
