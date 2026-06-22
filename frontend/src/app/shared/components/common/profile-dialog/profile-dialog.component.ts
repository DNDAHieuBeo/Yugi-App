import { Component, inject, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { resolveApiError } from '../../../../core/utils/error.utils';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const pw = control.get('newPassword')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return pw && confirm && pw !== confirm ? { mismatch: true } : null;
}

@Component({
  selector: 'app-profile-dialog',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './profile-dialog.component.html',
})
export class ProfileDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly authService = inject(AuthService);

  readonly visible = signal(false);
  readonly activeTab = signal<'profile' | 'password'>('profile');
  readonly profileLoading = signal(false);
  readonly passwordLoading = signal(false);
  readonly profileError = signal('');
  readonly profileSuccess = signal('');
  readonly passwordError = signal('');
  readonly passwordSuccess = signal('');
  readonly showCurrent = signal(false);
  readonly showNew = signal(false);

  readonly profileForm = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email:    ['', [Validators.required, Validators.email]],
  });

  readonly passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword:     ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordMatchValidator });

  open(): void {
    const user = this.authService.currentUser();
    this.profileForm.patchValue({ username: user?.username ?? '', email: user?.email ?? '' });
    this.profileForm.markAsPristine();
    this.passwordForm.reset();
    this.profileError.set('');
    this.profileSuccess.set('');
    this.passwordError.set('');
    this.passwordSuccess.set('');
    this.activeTab.set('profile');
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }

  switchTab(tab: 'profile' | 'password'): void {
    this.activeTab.set(tab);
  }

  saveProfile(): void {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    this.profileLoading.set(true);
    this.profileError.set('');
    this.profileSuccess.set('');

    const { username, email } = this.profileForm.value;
    this.authService.updateProfile({ username: username!, email: email! }).subscribe({
      next: () => {
        this.profileLoading.set(false);
        this.profileSuccess.set('Profile updated successfully.');
        this.profileForm.markAsPristine();
      },
      error: err => {
        this.profileLoading.set(false);
        this.profileError.set(resolveApiError(err));
      }
    });
  }

  savePassword(): void {
    if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
    this.passwordLoading.set(true);
    this.passwordError.set('');
    this.passwordSuccess.set('');

    const { currentPassword, newPassword } = this.passwordForm.value;
    this.authService.changePassword({ currentPassword: currentPassword!, newPassword: newPassword! }).subscribe({
      next: () => {
        this.passwordLoading.set(false);
        this.passwordSuccess.set('Password changed successfully.');
        this.passwordForm.reset();
      },
      error: err => {
        this.passwordLoading.set(false);
        this.passwordError.set(resolveApiError(err));
      }
    });
  }

  isInvalid(form: 'profile' | 'password', field: string): boolean {
    const c = form === 'profile'
      ? this.profileForm.get(field)
      : this.passwordForm.get(field);
    return !!(c?.invalid && c?.touched);
  }

  get passwordMismatch(): boolean {
    return !!(this.passwordForm.hasError('mismatch') && this.passwordForm.get('confirmPassword')?.touched);
  }
}
