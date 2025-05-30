import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { MenubarModule } from 'primeng/menubar';
import { TableModule } from 'primeng/table';
import { Toast } from 'primeng/toast';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-login',
  imports: [ButtonModule, MenubarModule, Toast, TableModule, InputTextModule, FormsModule, CardModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService) {

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, customEmailValidator()]],
      password: ['', Validators.required]
    });

    // User is already logged-in
    if (authService.isLoggedIn$.value === true)
      this.router.navigate(['/'])
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (res) => {
          if (res.success) {
            this.router.navigate(['/'])
          } else {
            console.log("Login failed: ", res.message);
            this.messageService.add({ severity: 'error', summary: 'Login failed', detail: res.message });
          }
        },
        error: (err: Error) => {
          console.error('Login failed', err)
          this.messageService.add({ severity: 'error', summary: 'Login failed', detail: err.message });
        }
      });
    }
  }  
}

// Custom email validator requiring a domain with a dot (e.g., .com, .net)
export function customEmailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    const valid = emailRegex.test(control.value);
    return valid ? null : { invalidEmail: true };
  };
}
