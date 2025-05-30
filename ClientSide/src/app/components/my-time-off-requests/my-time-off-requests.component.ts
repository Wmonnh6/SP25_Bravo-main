import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { MyTimeEntry } from '../../models/myTimeEntry';
import { TimeOffRequestsService } from '../../services/time-off-requests.service';
import { DeleteTimeOffRequest } from '../../models/DeleteTimeOffRequest';


@Component({
  selector: 'app-my-time-off-requests',
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
  templateUrl: './my-time-off-requests.component.html',
  styleUrl: './my-time-off-requests.component.scss'
})
export class MyTimeOffRequestsComponent {
  timeEntries: MyTimeEntry[] = []; // Store the time off requests for the table
  currentDate: Date = null;
  loading: boolean = false;
  deleteConfirmDialog: boolean = false; //visibility control for the confirmation dialog
  request: DeleteTimeOffRequest = new DeleteTimeOffRequest(); //request model to send to backend when deleting

  ngOnInit(): void {
    this.currentDate = new Date();
    this.loadRequests();
  }

  constructor(
    private timeOffRequestsService: TimeOffRequestsService,
    private messageService: MessageService
  ) { }

  loadRequests() {
    this.loading = true;
    this.timeOffRequestsService.getUserTimeOffRequests().subscribe({
      next: (response) => {
        this.loading = false;
        if (response && response.success && response.data) {
          // Log the fetched time entries to check correctness
          console.log('Fetched Time Off Requests:', response.data);
          this.timeEntries = response.data;
        } else {
          this.timeEntries = [];
          console.debug('No time entries found or invalid response format:', response);
        }
      },
      error: error => {
        console.error('Error fetching time entries:', error);
      }
    })
  }

  timeHasPassed(requestDate: Date): boolean {
    const date = new Date(requestDate);
    return this.currentDate.getTime() > date.getTime();
  }

  confirmDeleteTimeOffRequest(requestId: number, requestedDate: Date) {
    this.deleteConfirmDialog = true;
    this.request.timeOffRequestId = requestId;
    this.request.requestedDate = requestedDate;
  }

  deleteTimeOffRequest() {
    this.timeOffRequestsService.deleteUserTimeOffRequest(this.request).subscribe({
      next: (response) => {
        if (response.success) {
          // Remove the deleted entry from the local array
          this.timeEntries = this.timeEntries.filter(entry => entry.id !== this.request.timeOffRequestId);
          this.showSuccess('Time off request deleted successfully');
          this.cancelDelete();
        }
        else {
          this.showError(response.message);
          this.cancelDelete();
        }
      },
      error: (error) => {
        console.error('Error deleting time off request:', error);
        this.showError('Failed to delete time off request');
        this.cancelDelete();
      }
    });
  }

  cancelDelete() {
    this.deleteConfirmDialog = false;
  }

  // Function to show success message
  showSuccess(msg: string) {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: msg
    });
  }

  // Function to show error message
  showError(msg: string) {
    this.messageService.add({
      severity: 'error',
      summary: 'Error',
      detail: msg
    });
  }
}
