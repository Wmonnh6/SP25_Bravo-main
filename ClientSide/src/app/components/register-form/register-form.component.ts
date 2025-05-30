import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ValidationErrors, Validators, FormBuilder, FormGroup } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { CreateAccountRequest } from '../../models/createAccountRequest';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Router } from '@angular/router';
import { LoginRequest } from '../../models/loginRequest';

@Component({
  selector: 'app-register-form',
  imports: [CommonModule, ToastModule],
  templateUrl: './register-form.component.html',
  styleUrls: ['./register-form.component.scss'],
  providers: [MessageService],
})

export class RegisterFormComponent implements OnInit {
  registerForm: FormGroup;
  email: string = "";
  showPasswordTooltip: boolean = false;
  showConfirmPasswordTooltip: boolean = false;
  showEmailTooltip: boolean = false;
  passwordError: boolean = false;
  passwordValid: boolean = false;
  hasUpperCase: boolean = false;
  hasNumber: boolean = false;
  hasSpecialChar: boolean = false;
  hasMinLength: boolean = false;
  passwordsMatch: boolean = false;
  confirmPasswordLength: number = 0;
  passwordLength: number = 0;
  emailValid: boolean = false;
  tokenValid: boolean = false;
  showTokenTooltip: boolean = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private messageService: MessageService, private router: Router) {}

  // Gets form fields data and injects to the database
  createAccount(){
    console.log(this.registerForm);

    const email = (document.getElementById('email') as HTMLInputElement).value;
    const firstName = (document.getElementById('firstName') as HTMLInputElement).value;
    const lastName = (document.getElementById('lastName') as HTMLInputElement).value;
    const password = (document.getElementById('password') as HTMLInputElement).value;
    const token = (document.getElementById('invitationToken') as HTMLInputElement).value;

    var user = new CreateAccountRequest();
    user.email = email;
    user.firstName = firstName;
    user.lastName = lastName;
    user.password = password;
    user.invitationToken = token;
 
    console.log(user);

    this.authService.userAccount(user)
      .subscribe({
        // on success
        next: x => {
          console.log(x);

          // Displays backend success messages
          if(x.success && x.message){
            this.showMessage(x.message, 'success');
            var loginUser = new LoginRequest();
            loginUser.email = email;
            loginUser.password = password;

            this.authService.login(loginUser).subscribe({
              next: (res) => {
                if (res.success) {
                  this.router.navigate(['/']);
                } else {
                  this.showError("Automatic login failed: " + res.message);
                }
              },
              error: (err: Error) => {
                console.error('Automatic login failed', err);
                this.showError("Automatic login failed: " + err.message);
              }
            });
          }
          // Displays backend unsuccessful messages
          if(!x.success && x.message){
            this.showError(x.message);
          }
        },
        // on failure
        error: err => {
          console.log(err);
          const { Password, Email } = err.error.errors;
          // Display validation errors from backend
          if(Password || Email){
            if(Password){
              Password.forEach(msg => this.showError(msg));
            }
            if(Email){
              Email.forEach(msg => this.showError(msg));
            }
          // If it makes it passed validation on the backend then it needs to handle the ManagerResult.Unsuccessful errors
          } else {
            this.showError(err.message);
          }
        }
      });
  }
  // Displays the error message
  showError(error: string) {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: error });
  }

  // Displays the success message
  showMessage(message: string, severity: 'error' | 'success') {
  this.messageService.add({ severity: severity, summary: severity === 'error' ? 'Error' : 'Success', detail: message });
  }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      invitationToken: ['', Validators.required],
    }, { validators: this.passwordMatchValidator });
  }

  // Custom validator to ensure password and confirm password match
  passwordMatchValidator(form: FormGroup): ValidationErrors | null {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    if (password?.value !== confirmPassword?.value) {
      confirmPassword?.setErrors({ mismatch: true });
    } else {
      confirmPassword?.setErrors(null); // Clears error if passwords match
    }
    return null;
  }

  // Email validation on input
  onEmailChange(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const emailValue = inputElement.value;
  
    this.emailValid = this.validateEmail(emailValue);
  }

  // validates that email format is as expected
  validateEmail(email: string): boolean {
    const atSymbolCount = (email.match(/@/g) || []).length; // Count @ occurrences
    const atIndex = email.indexOf("@");
    const dotIndex = email.lastIndexOf(".");
    
    // Ensure exactly one '@', at least one character before and after '@', and a '.' after '@'
    return (
      atSymbolCount === 1 && 
      atIndex > 0 && 
      atIndex < email.length - 1 && 
      dotIndex > atIndex + 1 &&
      dotIndex < email.length - 1
    );
  }

  // Called when the password input changes
  onPasswordChange(event: Event): void {
    const passwordControl = this.registerForm.get('password');
  
    if (passwordControl) {
      const inputElement = event.target as HTMLInputElement;
      const passwordValue = inputElement.value;
      
      // Check individual conditions
      this.hasMinLength = passwordValue.length >= 8;
      this.hasUpperCase = /[A-Z]/.test(passwordValue);
      this.hasNumber = /\d/.test(passwordValue);
      this.hasSpecialChar = /[!@#$%*]/.test(passwordValue);

      // The password is valid only if all conditions are met
      this.passwordValid = this.hasMinLength && this.hasUpperCase && this.hasNumber && this.hasSpecialChar;
    
    }
  }

  // Called when the confirm password input changes
  onConfirmPasswordChange(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const confirmPasswordValue = inputElement.value; // Get the current confirm password value
    const passwordElement = document.getElementById('password') as HTMLInputElement;
    const passwordValue = passwordElement?.value; // Get the original password value from the form

    this.confirmPasswordLength = confirmPasswordValue.length;
    this.checkPasswordsMatch(passwordValue, confirmPasswordValue);
  }

  // Check if passwords match
  checkPasswordsMatch(passwordValue: string | undefined, confirmPasswordValue: string): void {
    // Check if passwords match and confirm password has a value
    this.passwordsMatch = confirmPasswordValue.length > 0 && passwordValue === confirmPasswordValue;
  }

  onTokenChange(event: Event): void {
    const input = (event.target as HTMLInputElement).value;
    this.tokenValid = input.length > 0; // Token is valid if at least one character is entered
  }

  // Checks Form fields Validity
  isFormValid(): boolean {
    const firstName = document.getElementById('firstName') as HTMLInputElement;
    const firstNameValid = firstName.value.length > 0;
    const lastName = document.getElementById('lastName') as HTMLInputElement;
    const lastNameValid = lastName.value.length > 0;
    const passwordValidAndMatch = this.passwordValid && this.passwordsMatch;
    const emailValid = this.emailValid;
    const tokenValid = this.tokenValid;
    // Dynamically check each field's validity
    return firstNameValid && lastNameValid && passwordValidAndMatch && emailValid && tokenValid;
  }
  // Navigate User to login page when sign in is clicked
  navLoginPage(){
    this.router.navigate(['/login']);
  }

  // Getter for first name
  get firstName() {
    return this.registerForm.get('firstName');
  }
  // Getter for last name
  get lastName() {
    return this.registerForm.get('lastName');
  }
  // Getter for password
  get password() {
    return this.registerForm.get('password');
  }

  // Getter for confirmPassword
  get confirmPassword() {
    return this.registerForm.get('confirmPassword');
  }
}
