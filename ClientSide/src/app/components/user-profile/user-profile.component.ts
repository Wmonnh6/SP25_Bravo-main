// component imports
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message'
import { PanelModule } from 'primeng/panel';
import { IftaLabelModule } from 'primeng/iftalabel';
import { ButtonModule } from 'primeng/button';

// service imports
import { UserProfileService } from '../../services/user-profile.service';
import { AuthService } from '../../services/auth.service';
import { UpdateProfileRequest } from '../../models/updateProfileRequest';
import { MessageService } from 'primeng/api';

// directive import
import { PasswordMatchModule } from '../../modules/password-match/password-match.module';
import { UserDto } from '../../models/userDto';

@Component({
  selector: 'app-user-profile',
  imports: [CommonModule, 
    ButtonModule, 
    FormsModule, 
    IftaLabelModule, 
    MessageModule,
    PanelModule, 
    ToastModule, 
    PasswordMatchModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss'
})
export class UserProfileComponent implements OnInit {
  // model data
  user: UserDto = null;
  model: UpdateProfileRequest = null;

  // tooltips
  showFirstnameTooltip: boolean = false;
  showLastnameTooltip: boolean = false;
  showPasswordTooltip: boolean = false;

  constructor(
    readonly userProfileService: UserProfileService,
    readonly authService: AuthService,
    readonly messageService: MessageService
  ) {
  }

  ngOnInit() {
    // prepopulate the values for the user info
    this.user = this.authService.getLocalUserInfo();
    this.model = new UpdateProfileRequest(
      this.user?.email,
      this.user?.firstName,
      this.user?.lastName,
      '',
      ''
      );
  }

  updateUserProfile(form: NgForm) {
    this.userProfileService.updateUserProfile(this.model)
      .subscribe({
        next: resp => {
          if(resp.success) {            
            console.log("User profile updated: ", resp.message);
            this.authService.updateToken(resp.data.token);
            
            form.resetForm();
            setTimeout(() => this.loadUserData(resp.data),50);
            this.showMessage(resp.message);
          } else {
            console.log("Update failed: ", resp.message);
            
            this.showError(resp.message);
          }
        },
        error: err => {
          console.error('Update failed', err);
          const { FirstName, LastName, NewPassword } = err.error.errors;
          if(FirstName || LastName || NewPassword) {
            if(FirstName) FirstName.forEach(msg => {
              this.showError(msg);
            });
            if(LastName) LastName.forEach(msg => {
              this.showError(msg);
            });
            if(NewPassword) NewPassword.forEach(msg => {
              this.showError(msg);
            });
          } else {
            this.showError(err.message);
          }
        }
      });
  }

  resetForm(form: NgForm) {
    form.resetForm(); // resets the fields and validations
    // repopulate the name fields, and make sure the model still has the email for the user
    setTimeout(() => this.loadUserData(this.authService.getLocalUserInfo()),50); // without a set timeout it renders with the fields empty
  }

  loadUserData(user: UserDto) {
    console.log('Resetting the user info');
    console.log(user);

    this.model.email = user?.email;
    this.model.firstName = user?.firstName;
    this.model.lastName = user?.lastName;
    console.log(this.model);
  }

  showMessage(msg: string) {
    this.messageService.add({
      severity: 'success', 
      summary: 'Update succeeded',
      detail: msg
    });
  }

  showError(err: string) {
    this.messageService.add({
      severity: 'error',
      summary: 'Update failed',
      detail: err
    });
  }
}
