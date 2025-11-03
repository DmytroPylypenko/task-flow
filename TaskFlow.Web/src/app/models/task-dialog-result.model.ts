import { TaskUpdate } from './task-update.model';

/**
 * Defines the shape of the data returned from the TaskDetailModal.
 * This is a discriminated union, allowing type-safe handling of different actions.
 */
export type TaskDialogResult =
  | { action: 'update'; payload: TaskUpdate }
  | { action: 'delete' };