import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { MessageService } from 'primeng/api';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-password-reset',
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, InputTextModule, ButtonModule, ToastModule],
  providers: [MessageService], // Provide MessageService
})
export class PasswordResetComponent {
  passwordResetForm: FormGroup;

  constructor(private fb: FormBuilder, private userService: UserService, private messageService: MessageService) {
    this.passwordResetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    console.log('onSubmit() triggered!');

    if (this.passwordResetForm.invalid) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please enter a valid email.' });
      console.log('Invalid email input.');
      return;
    }

    const email = this.passwordResetForm.value.email;
    console.log('Email entered:', email);

    this.userService.checkEmailExists(email).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: response.success ? 'success' : 'error',
          summary: response.success ? 'Success' : 'Error',
          detail: response.message,
        });
      },
      error: (err) => {
        console.error('API Error:', err);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Something went wrong. Please try again.' });
      },
    });
  }

  handleButtonClick(event: Event) {
    event.preventDefault();
    console.log('Reset Password Button Clicked!');
    this.onSubmit();
  }
}
