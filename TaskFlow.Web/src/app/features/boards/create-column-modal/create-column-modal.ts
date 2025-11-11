import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { DialogRef } from '@angular/cdk/dialog';

@Component({
  selector: 'app-create-column-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './create-column-modal.html',
  styleUrl: './create-column-modal.scss',
})
export class CreateColumnModalComponent {
  private readonly fb = inject(FormBuilder);
  public readonly dialogRef = inject(DialogRef<string>);

  columnForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });

  create(): void {
    if (this.columnForm.valid && this.columnForm.value.name) {
      this.dialogRef.close(this.columnForm.value.name);
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
