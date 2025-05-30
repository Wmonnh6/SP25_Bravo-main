import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { TimeOffRequestManagementDTO } from '../../models/TimeOffRequestManagementDTO';
import { TimeOffManagementService } from '../../services/time-off-management.service';
import { MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { UserSelectionDto } from '../../models/UserSelectionDto';
import { UserService } from '../../services/user.service';
import { ApproveTimeOffStatusRequest } from '../../models/ApproveTimeOffStatusRequest';
import { RejectTimeOffStatusRequest } from '../../models/RejectTimeOffStatusRequest';

@Component({
  selector: 'app-time-off-management',
  imports: [
    ButtonModule,
    DialogModule,
    FormsModule,
    SelectModule,
    InputNumberModule,
    DatePickerModule,
    TextareaModule,
    ToastModule,
    TableModule,
    CommonModule
  ],
  templateUrl: './time-off-management.component.html',
  styleUrl: './time-off-management.component.scss'
})
export class TimeOffManagementComponent {

  loading = true;
  timeOffRequests: TimeOffRequestManagementDTO[] = [];
  currentEmployeeId?: number = null;
  employees: UserSelectionDto[] = [];
  requestStatus = ["All", "Pending", "Approved", "Rejected"]
  currentRequestStatus = "Pending";
  rangeDates: Date[] = null;
  currentRequest: any; // Holds request object for approval pending
  isDialogVisible: boolean = false;
  isRejectDialogVisible: boolean = false;
  rejectComment: string = '';

  constructor(
    private timeOffManagementService: TimeOffManagementService,
    private messageService: MessageService,
    private userService: UserService,
  ) {

    this.getAllTimeOffRequests();

    this.userService.getAllEmployees().subscribe({
      next: (res) => {
        console.log(res);
        if (res.success)
          this.employees = res.data;
        else
          this.showError(res.message);
      },
      error: (err) => {
        console.log(err);
        this.showError(err.message);
      }
    });
  }

   // Show the confirmation dialog
  confirmApproval(request: any) {
    this.currentRequest = request; // Store the request to approve
    this.isDialogVisible = true;  // Show the confirmation dialog
  }

  // Handle the cancel action for approval dialog
  onCancel() {
    this.isDialogVisible = false; // Close the dialog without approving
  }

   // Show the rejection confirmation dialog
  confirmRejection(request: any) {
    this.currentRequest = request; // Store the request to reject
    this.rejectComment = ''; // Clear the previous comment
    this.isRejectDialogVisible = true; // Show the rejection dialog
  }

  // Handle the cancel action for rejection
  onRejectCancel() {
    this.isRejectDialogVisible = false; // Close the rejection dialog without rejecting
  }

  getAllTimeOffRequests() {
    this.timeOffManagementService.getAllTimeOffRequests(
      {
        userId: this.currentEmployeeId,
        requestStatus: this.currentRequestStatus,
        startDate: this.rangeDates && this.rangeDates[0] ? this.rangeDates[0] : null,
        endDate: this.rangeDates && this.rangeDates[1] ? this.rangeDates[1] : null
      }).subscribe({
        next: (res) => {
          console.log(res);
          if (res.success) {
            this.timeOffRequests = res.data;
          } else {
            this.showError(res.message);
          }

          this.loading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.showError(err.message);
          console.log(err);
          this.loading = false;
        }
      });
  }
  // Function to change status of request to approved
  approve(request: any) {
    const updateRequest: ApproveTimeOffStatusRequest = {
      requestId: request.timeOffRequest.id, 
      status: 'Approved'
    };
    this.timeOffManagementService.approveTimeOffStatus(updateRequest)
    .subscribe({
      next: (res) => {
        if (res.success) {
          request.timeOffRequest.status = 'Approved'; // Update UI
          this.isDialogVisible = false;
          this.showSuccess("Time-off request approved successfully and User has been notified");
        } else {
          this.showError(res.message);
        }
      },
      error: (err) => {
        console.error(err);
        this.showError("Failed to approve time-off request.");
      }
    });
  }

  // Function to change status of request to rejected
  reject(request: any) {
    // If a rejection comment is provided, append it to the existing comment
    const updatedComment = request.comment + (this.rejectComment ? ` \n\n[RejectMessage] - ${this.rejectComment}` : '');

    const updateRequest: RejectTimeOffStatusRequest = {
      requestId: request.timeOffRequest.id,
      status: 'Rejected',
      comment: updatedComment // Update the comment with the rejection message
    };

    this.timeOffManagementService.rejectTimeOffRequest(updateRequest)
      .subscribe({
        next: (res) => {
          if (res.success) {
            request.timeOffRequest.status = 'Rejected'; // Update UI
            request.comment = updatedComment; // Update comment in UI
            this.isRejectDialogVisible = false;
            this.showSuccess("Time-off request rejected successfully and User has been notified");
          } else {
            this.showError(res.message);
          }
        },
        error: (err) => {
          console.error(err);
          this.showError("Failed to reject time-off request.");
        }
      });
  }

  onEmployeeChange($event: SelectChangeEvent) {
    console.log("Employee changed. Employee ID: ", $event.value);
    this.currentEmployeeId = $event.value;
    this.getAllTimeOffRequests();
  }

  onRequestStatusChange($event: SelectChangeEvent) {
    console.log("Status: ", $event.value);
    this.getAllTimeOffRequests();
  }

  onDateChange($event: any) {
    console.log("Date changed: ", $event);
    console.log(this.rangeDates);
    this.getAllTimeOffRequests();
  }

  onDateClear() {
    this.rangeDates = null;
    this.getAllTimeOffRequests();
  }

  // Function to show error message
  showError(msg: string) {
    this.messageService.add({
      severity: 'error',
      summary: 'Error',
      detail: msg
    });
  }
  // Function to show success message
  showSuccess(msg: string) {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: msg
    });
  }
}
