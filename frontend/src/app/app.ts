import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { LoadingScreenComponent } from './shared/components/loading-screen/loading-screen.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, LoadingScreenComponent],
  template: `
    @if (authService.initializing()) {
      <app-loading-screen />
    } @else {
      <router-outlet />
    }
  `,
})
export class App {
  readonly authService = inject(AuthService);
}
