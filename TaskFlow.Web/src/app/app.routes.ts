import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { BoardListComponent } from './features/boards/board-list/board-list';
import { authGuard } from './core/guards/auth-guard';
import { BoardDetailComponent } from './features/boards/board-detail/board-detail';
import { MainLayoutComponent } from './core/layout/main-layout/main-layout';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'boards', component: BoardListComponent },
      { path: 'boards/:id', component: BoardDetailComponent },
    ]
  }
];
