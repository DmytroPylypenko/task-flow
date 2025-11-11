import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { DialogRef } from '@angular/cdk/dialog';

@Component({
  selector: 'app-create-board-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './create-board-modal.html',
  styleUrl: './create-board-modal.scss',
})
export class CreateBoardModalComponent {
  private readonly fb = inject(FormBuilder);
  public readonly dialogRef = inject(DialogRef<string>);

  boardForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]]
  });

  create(): void {
    if (this.boardForm.valid && this.boardForm.value.name) {
      this.dialogRef.close(this.boardForm.value.name);
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
