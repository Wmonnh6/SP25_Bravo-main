import { Component, ViewChild } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ResetPasswordRequest } from '../../models/resetPasswordRequest'; 
import { ResetPasswordService } from '../../services/reset-password.service.'
import { AuthService } from '../../services/auth.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-set-new-password',
  imports: [
    InputTextModule,
    FormsModule,
    CardModule,
    ButtonModule,
    NgIf
  ],
  templateUrl: './set-new-password.component.html',
  styleUrl: './set-new-password.component.scss'
})
export class SetNewPasswordComponent {

  //input field and condition variables
  token: string = '';
  password: string = ''; 
  resetSuccess: boolean = false;
  submitClicked = false;

  //error message variable that may be displayed
  errorMsg = "";

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private resetPWService : ResetPasswordService
  ) {}

  // Get the token from the URL
  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token');
  }
 
  @ViewChild('resetForm') resetForm!: NgForm;

  onSubmit() {
    if (this.resetForm.valid) {

      //make a new reset password request
      //with the token and new password
      const request = new ResetPasswordRequest();
      request.resetToken = this.token;
      request.newPassword = this.password;

      //send request to backend with service method
      //and handle response
      this.resetPWService.resetPassword(request).subscribe({
        next: response => {

            //status variable gets updated so the form can respond
            this.resetSuccess = response.success;

            //set appropriate error message to display
            //if the reset request was unsuccessful
            this.errorMsg = response.message;

            console.log(this.errorMsg);
            
            if (response.success) {
              setTimeout(() => {
                this.router.navigate(['/'])
                this.authService.logout();
              }, 1000);
            }
        },
        error: error => {
          this.errorMsg = "There was an unexpected error with the request.";
          console.log(error);
        }
      });
    } else { //inputs have errors
      this.resetForm.form.markAllAsTouched(); 
    }
  }

  // Click event to track if the submit button was pressed
  onClickSubmit() {
    this.submitClicked = true;
  }

}
