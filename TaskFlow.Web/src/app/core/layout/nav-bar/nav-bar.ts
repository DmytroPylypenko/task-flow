import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LayoutDashboard, LogOut, User, Settings, ChevronDown } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth';

/**
 * Handles the main top navigation bar for the application.
 * Includes navigation links and the user profile dropdown.
 */
@Component({
  selector: 'app-nav-bar',
  imports: [LucideAngularModule, RouterLinkActive, RouterLink],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.scss',
})
export class NavBarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Expose icons to the template as readonly properties for type safety
  readonly DashboardIcon = LayoutDashboard;
  readonly LogOutIcon = LogOut;
  readonly UserIcon = User;
  readonly SettingsIcon = Settings;
  readonly ChevronDownIcon = ChevronDown;

  // Manages the visibility state of the profile dropdown menu. 
  isProfileOpen = false;

  /**
   * Toggles the visibility of the profile dropdown menu.
   */
  toggleProfile(): void {
    this.isProfileOpen = !this.isProfileOpen;
  }

  /**
   * Logs the user out by clearing the auth token and redirecting to the login page.
   */
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
