import { Task } from './task.model';

export interface Column {
  id: number;
  name: string;
  tasks: Task[];
}