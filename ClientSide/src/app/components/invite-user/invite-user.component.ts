// component imports
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { IftaLabelModule } from 'primeng/iftalabel';
import { CheckboxModule } from 'primeng/checkbox';
import { PanelModule} from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';

// service imports
import { InviteService } from '../../services/invite.service';
import { MessageService } from 'primeng/api';
import { InvitationRequest } from '../../models/invitationRequest';


@Component({
  selector: 'app-invite-user',
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, IftaLabelModule, CheckboxModule, PanelModule, ToastModule],
  templateUrl: './invite-user.component.html',
  styleUrl: './invite-user.component.scss'
})
export class InviteUserComponent {

  employeeEmail : string = "";
  isAdmin : boolean = false;

  status : string = "";

  constructor(
    readonly inviteService: InviteService,
    readonly messageService: MessageService
  ) {}

  // function to call the invite service
  createInvite() {
    const request = new InvitationRequest(this.employeeEmail, this.isAdmin);
    console.log(request);
    this.inviteService.createInvite(request)
      .subscribe({
        // on success
        next: resp => {
          console.log(resp);
          this.status = resp.message;
          this.employeeEmail = '';
          this.isAdmin = false;
          this.showSuccess(resp.message);
        },
        // on failure
        error: err => {
          // the backend won't pass the internal reasons for failure so this is a generic error message with reminder for the dev to check the backend logs
          this.status = "Something went wrong when creating the invitation. Check the backend logs";
          console.log(err);

          // if there is an Email property of the errors object then it comes from the validation of the request on the backend
          // else it comes from the ManagerResult messages from the backend
          const { Email } = err.error.errors;
          if(Email) Email.forEach(msg => this.showError(msg)); // this properly displays the array of errors coming from the Email validation on the backend
          else this.showError(err.message); // this properly displays the message from the ManagerResults from the backend
        }
      });
  }

  showSuccess(msg: string) {
    this.messageService.add({
      severity: 'success',
      summary: 'Successful Invite',
      detail: msg
    });
  }

  showError(err: string) {
    this.messageService.add({
      severity: 'error',
      summary: 'Invite Error',
      detail: err
    })
  }
}
