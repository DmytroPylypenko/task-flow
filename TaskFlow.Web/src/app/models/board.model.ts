import { Column } from "./column.model";

export interface Board {
  id: number;
  name: string;
  userId: number;
  columns: Column[];
}