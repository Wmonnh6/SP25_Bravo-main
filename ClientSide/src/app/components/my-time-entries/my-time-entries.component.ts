import { Component, ViewChild } from '@angular/core';
import { ToastModule } from 'primeng/toast';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';

//service imports
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';
import { MessageService } from 'primeng/api';
import { TimeEntryService } from '../../services/time-entry.service';

//form imports
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FormsModule, NgForm } from '@angular/forms';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';

//model import
import { Task } from '../../models/task';
import { MyTimeEntry } from '../../models/myTimeEntry';
import { TimeEntryRequest } from '../../models/timeEntryRequest';
import { UserService } from '../../services/user.service';
import { UserSelectionDto } from '../../models/UserSelectionDto';
import { ClosedWeekRequest } from '../../models/ClosedWeekRequest';

@Component({
    selector: 'app-my-time-entries',
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
    templateUrl: './my-time-entries.component.html',
    styleUrl: './my-time-entries.component.scss'
})
export class MyTimeEntriesComponent {

    timeEntries: MyTimeEntry[] = []; // Store the time entries
    totalRecords: number = 0; // Total records to be paginated
    rowsPerPage: number = 5; // Rows per page
    loading: boolean = false; // Loading indicator
    currentDate: Date = null; // Current Date
    dateRange: string = ''; // Date Range for page view
    deleteConfirmDialog: boolean = false;
    timeEntryToDeleteId: number = null;
    //The label of the lock week button
    closeActionLabel: string = '';
    //The lock status of the current week
    isClosed: boolean = false;
    //Visibility control for the confirm close dialog
    closeConfirmDialog: boolean = false;
    
    //Initialize new entry to bind values from form
    timeEntryRequest: TimeEntryRequest = new TimeEntryRequest(0, null, null, null, null, null);
    //access ngForm 
    @ViewChild('entryForm') entryForm!: NgForm;
    //Visibility control for dialog and error msg 
    displayDialog: boolean = false;
    submitClicked: boolean = false;
    //Varaibles for task selector drop down
    tasks: Task[] = []; //options
    selectLoading: boolean = false;

    // properties for the add/edit dialog
    dialogHeader: string = ''; // this property is to set the dialog header depending whether it's for adding or editing time entries
    submitLabel: string = ''; // this property is to set the dialog submit button depending whether it's for adding or editing a time entry
    timeEntryType: string = ''; // this property is for setting whether it's an add or edit time entry

    employees: UserSelectionDto[] = [];
    currentEmployeeId?: number = null;
    isAdmin = false;

    //Date picker upper boundary
    maxDate: Date;

    today = new Date();

    constructor(
        private timeEntryService: TimeEntryService,
        private authService: AuthService,
        private taskService: TaskService,
        private userService: UserService,
        private messageService: MessageService) {

    }

    ngOnInit(): void {
        this.authService.user$.subscribe(x => this.isAdmin = x?.isAdmin || false);
        //fetch tasks to populate Select drop down
        this.fetchTasks();

        // Fetch the logged-in user ID from AuthService
        const user = this.authService.user$.value;

        if (user?.id) {
            if (user.isAdmin) {
                this.userService.getAllEmployees().subscribe({
                    next: res => {
                        console.log(res);
                        if (res.success)
                            this.employees = res.data;
                        else
                            this.showError(res.message);
                    },
                    error: err => {
                        console.log(err);
                        this.showError(err.message);
                    }
                });
            }
        }
        this.currentDate = new Date();
        this.updateDateRange();

        if (this.isAdmin && this.currentEmployeeId) {
            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
        } else {
            this.loadTimeEntries(this.currentDate);
        }

        this.checkWeekStatus();
    }

    // Method to load time entries (will be triggered on pagination)
    loadTimeEntries(Date: Date): void {
        this.timeEntryService.getTimeEntries(Date).subscribe({
            next: (response) => {
                if (response && response.success && response.data) {
                    // Log the fetched time entries to ensure correct data is fetched
                    console.log('Fetched Time Entries:', response.data); // This should be the array of time entries
                    this.timeEntries = response.data; // Assign the time entries array to the table
                    this.totalRecords = response.data.length;
                } else {
                    this.timeEntries = [];
                    console.debug('No time entries found or invalid response format:', response);
                }
            }, error: error => {
                console.error('Error fetching time entries:', error);
            }
        })
    }

    // Function to fetch tasks from the backend
    fetchTasks() {
        this.selectLoading = true;
        this.taskService.getAllTasks().subscribe({
            next: (tasks) => {
                this.tasks = tasks || []; // Assign the tasks to the component's tasks array
                this.selectLoading = false;
            },
            error: (err) => {
                console.log(err); // Log errors
                this.tasks = [];
                this.selectLoading = false;
            }
        });
    }

    //Brings up the dialog to add an entry
    showNewEntryDialog() {
        if (this.isClosed){
            this.showError("The current week is closed for edits.");
        }
        else {
            this.timeEntryType = 'add';
            this.dialogHeader = 'Add New Time Entry';
            this.submitLabel = 'Add Entry';

            // Todo: when working on the update for admin add the logic to get the userid
            // of the selected employee and load it
            this.timeEntryRequest.UserId = this.authService.getLocalUserInfo().id;

            this.displayDialog = true;
        }
    }

    /*
        This function passes in the time entry as well as the task from the time entry
        because for some reason it is undefined if we try to destructure it or reference it from the entry.task
        (or entry.MyTimeEntryTask). We destructure user from the time entry here because
        for some reason it also is undefined when we try to reference entry.user.
        The markAsDirty calls are because even though we have 2-way binding on the model fields and
        are changing them here, the fields are still considered pristine and will therefore fail
        the required validation that was put in the html for each field.
    */
    showEditEntryDialog(entry: MyTimeEntry, id: number) {
        if (this.isClosed) {
            this.showError("The current week is closed for edits.");
        }
        else {
           const { user } = entry;
            // set the dialog header and labels
            this.timeEntryType = 'edit';
            this.dialogHeader = 'Edit Time Entry';
            this.submitLabel = 'Update Entry';
            // populate the data with the selected entry's info
            this.timeEntryRequest.Comment = entry.comment;
            this.timeEntryRequest.Date = new Date(entry.date);
            this.timeEntryRequest.Hours = entry.hours;
            this.timeEntryRequest.TaskId = entry.task.id;
            this.timeEntryRequest.UserId = entry.user.id;
            this.timeEntryRequest.Id = id;

            // "touch" each of the controls to bypass the pristine check for required
            this.entryForm.controls['selectedTask'].markAsDirty();
            this.entryForm.controls['selectedDate'].markAsDirty();
            this.entryForm.controls['hrsWorked'].markAsDirty();
            this.entryForm.controls['comment'].markAsDirty();
            // show the dialog
            this.displayDialog = true; 
        }
    }

    //Function to cancel adding a new entry and reset the form
    closeDialog(entryForm: NgForm) {
        this.displayDialog = false;
        entryForm.reset();
        this.submitClicked = false;
    }

    // Method to retrieve the next weeks entries
    nextWeek() {        
        this.currentDate.setDate(this.currentDate.getDate() + 7);
        this.updateDateRange();
        this.checkWeekStatus();

        if (this.isAdmin && this.currentEmployeeId) {
            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
        } else {
            this.loadTimeEntries(this.currentDate);
        }
    }

    // Method to retrieve the previous weeks entries
    previousWeek() {
        this.currentDate.setDate(this.currentDate.getDate() - 7);
        this.updateDateRange();
        this.checkWeekStatus();

        if (this.isAdmin && this.currentEmployeeId) {
            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
        } else {
            this.loadTimeEntries(this.currentDate);
        }
    }

    // Calculates and updates the current week's date range
    updateDateRange() {
        const startDate = this.currentDate;
        startDate.setDate(this.currentDate.getDate() - this.currentDate.getDay()); // Start of the week
        const endDate = new Date(startDate);
        endDate.setDate(startDate.getDate() + 6); // End of the week

        this.dateRange = `${startDate.toLocaleDateString()} - ${endDate.toLocaleDateString()}`;
    }

    getStartOfWeek(date: Date): Date {
        const startOfWeek = new Date(date);
        const day = startOfWeek.getDay();
        const diff = startOfWeek.getDate() - day;
        startOfWeek.setDate(diff);
        return startOfWeek;
    }
    
    getEndOfWeek(date: Date): Date {
        const endOfWeek = new Date(date);
        const day = endOfWeek.getDay();
        const diff = endOfWeek.getDate() - day + 6;
        endOfWeek.setDate(diff);
        return endOfWeek;
    }

    

    // this function handles the logic of whether it is
    // submitted as a new entry or an updated entry
    // Todo: this will need to be modified for when the admin
    // submits entries for another employee
    submitTimeEntry(form: NgForm) {
        if(this.timeEntryType === 'add') {
            this.addNewEntry(form);
        } else if (this.timeEntryType === 'edit') {
            this.editTimeEntry(form);
        } else {
            this.showError('There was an error opening the dialog');
        }
    }

    addNewEntry(newForm: NgForm) {
        this.submitClicked = true;
        console.log("Attempting to save new entry:", this.timeEntryRequest);
        if (newForm.valid) {
            if (this.currentEmployeeId && this.isAdmin) {
                // A different employee is selected from the drop-down
                this.timeEntryRequest.UserId = this.currentEmployeeId;  // Add the selected employee's UserId to the request
                
                // Submit request to the backend to add time entry for another employee
                this.timeEntryService.addOthersTimeEntry(this.timeEntryRequest).subscribe({
                    next: response => {
                        console.log("Added new time entry for " + this.employees.find(({ id }) => id === this.currentEmployeeId).name);
                        this.showSuccess(response.message);
                         // Get the current date range (start of week to end of week)
                        const startOfWeek = this.getStartOfWeek(new Date(this.currentDate));
                        const endOfWeek = this.getEndOfWeek(new Date(this.currentDate));
                    
                        const entryDate = new Date(response.data.date);
                        // Determine if new task should be pushed to the array based on the current viewed week
                        if(entryDate >= startOfWeek && entryDate <= endOfWeek){
                        this.timeEntries.push(response.data);
                        }
                        this.closeDialog(newForm);
                    },
                    error: error => {
                        console.log(error);
                        this.showError(error.message);
                    }
                });
            } else {

            //submit request to backend 
            this.timeEntryService.addTimeEntry(this.timeEntryRequest).subscribe({
                next: response => {
                    if (response.success) {
                        console.log("Added new time entry.");
                        this.showSuccess(response.message);
                        // Get the current date range (start of week to end of week)
                        const startOfWeek = this.getStartOfWeek(new Date(this.currentDate));
                        const endOfWeek = this.getEndOfWeek(new Date(this.currentDate));

                        const entryDate = new Date(response.data.date);
                        // Determine if new task should be pushed to the array based on the current viewed week
                        if (entryDate >= startOfWeek && entryDate <= endOfWeek) {
                            this.timeEntries.push(response.data);
                        }
                        this.closeDialog(newForm);
                    } else {
                        this.showError(response.message);
                    }
                },
                error: error => {
                    console.log(error);
                    this.showError(error.message);
                }
            });
        }
        } else {
            this.showError("Form is invalid!");
            console.log("Form is invalid!");
        }
    }
    // Method for admins to fetch employee time entries
    loadEmployeeTimeEntries(date: Date, userId: number): void {
        if (!userId) {
            this.showError("Please select an employee.");
            return;
        }

        this.timeEntryService.getEmployeeTimeEntries(date, userId).subscribe({
            next: (response) => {
                if (response && response.success && response.data) {
                    console.log('Fetched Employee Time Entries:', response.data);
                    this.timeEntries = response.data;
                    this.totalRecords = response.data.length;
                } else {
                    this.timeEntries = [];
                    console.debug('No time entries found or invalid response format:', response);
                }
            },
            error: (error) => {
                console.error('Error fetching employee time entries:', error);
                this.showError('Failed to fetch employee time entries.');
            }
        });
    }

    editTimeEntry(editForm: NgForm) {
        console.log("Attempting to update time entry:", this.timeEntryRequest);
        this.submitClicked = true;

        if (editForm.valid) {
            this.timeEntryService.updateTimeEntry(this.timeEntryRequest).subscribe({
                next: resp => {
                    console.log("Updating the time entry.");
                    if(resp.success) {
                        this.showSuccess(resp.message);
                        if (this.isAdmin && this.currentEmployeeId) {
                            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
                        } else {
                            this.loadTimeEntries(this.currentDate);
                        }

                        this.closeDialog(editForm);
                    } else {
                        this.showError(resp.message);
                    }
                },
                error: err => {
                    console.log(err);
                    this.showError(err.message);
                }
            });
        } else {
            this.showError("Form is invalid!");
            console.log("Form is invalid!");
        }
    }

    // Method to delete a time entry
    deleteTimeEntry() {
        if (!this.timeEntryToDeleteId) return;

        // Determine if we're deleting someone else's entry
        const isDeletingOthersEntry = this.currentEmployeeId && this.isAdmin;

        let deleteObservable;

        if (isDeletingOthersEntry) {
            // Use admin endpoint when admin is deleting someone else's entry
            deleteObservable = this.timeEntryService.deleteAnyTimeEntry(this.timeEntryToDeleteId);
        } else {
            // Use regular endpoint when user is deleting their own entry
            deleteObservable = this.timeEntryService.deleteTimeEntry(this.timeEntryToDeleteId);
        }

        deleteObservable.subscribe({
            next: (response) => {
                // Remove the deleted entry from the local array
                if (response.success) {
                    // Remove the deleted entry from the local array
                    this.timeEntries = this.timeEntries.filter(entry => entry.id !== this.timeEntryToDeleteId);
                    this.showSuccess('Time entry deleted successfully');
                    this.cancelDelete();
                }
                else {
                    this.showError(response.message);
                    this.cancelDelete();
                }
            },
            error: (error) => {
                console.error('Error deleting time entry:', error);
                this.showError('Failed to delete time entry');
                this.cancelDelete();
            }
        });
    }

    // Method to cancel delete operation
    cancelDelete() {
        this.deleteConfirmDialog = false;
        this.timeEntryToDeleteId = null;
    }

    confirmDeleteTimeEntry(timeEntryId: number) {
        if (this.isClosed) {
            this.showError("The current week is closed for edits.");
        }
        else {
           console.log(timeEntryId);
            this.timeEntryToDeleteId = timeEntryId;
            this.deleteConfirmDialog = true; 
        }
    }

    onEmployeeChange($event: SelectChangeEvent) {
        console.log("Employee changed. Employee ID: ", $event.value);
        this.currentEmployeeId = $event.value;

        if (this.currentEmployeeId) {
            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
        } else {
            this.loadTimeEntries(this.currentDate);
        }
    }

    getCurrentDate() {
        this.currentDate = new Date();
        this.updateDateRange();
        this.checkWeekStatus();
        if (this.currentEmployeeId) {
            this.loadEmployeeTimeEntries(this.currentDate, this.currentEmployeeId);
        } else {
            this.loadTimeEntries(this.currentDate);
        }
    }

    isCurrentWeek() : boolean {
        const startDate = this.currentDate;
        startDate.setDate(this.currentDate.getDate() - this.currentDate.getDay()); // Start of the week

        const endDate = new Date(startDate);
        endDate.setDate(startDate.getDate() + 6); // End of the week
        
        return this.today.getDate() >= startDate.getDate() && this.today.getDate() <= endDate.getDate();
    }

    //Method to check the current week status (closed or open)
    checkWeekStatus() {
        this.timeEntryService.checkWeekStatus(this.currentDate).subscribe({
            next: (response) => {
                //Set the status variable
                this.isClosed = response.data;
                console.log("Week is closed?:", this.isClosed);
            },
            error: (error) => {
                console.error('Error when checking the week status:', error);
            }
        });
    }

    // Method to bring up the confirmation dialog for closing/opening a week
    confirmCloseWeek() {
        this.closeConfirmDialog = true;
    }

    // Closes the dialog
    cancelClose() {
        this.closeConfirmDialog = false;
    }

    // Method that determines whether the button will open or close a week
    changeWeekStatus() {
        if (this.isClosed) {
            //week is locked, so the action taken will be to open it
            this.openWeek();
        }
        else {
            //week is open, so the action taken will be to close it
            this.confirmCloseWeek();
        }
    }

    // Opens the week currently being viewed
    openWeek(){
        //make a new request
        const week = new ClosedWeekRequest();
        week.date = this.currentDate;
        console.log("Opening week that contains this date:" + this.currentDate);

        //send request to backend
        this.timeEntryService.openWeek(week).subscribe({
            next: (response) => {
                if (response.success){
                  this.isClosed = false;
                    this.showSuccess("Week opened successfully!");
                    this.closeConfirmDialog = false;  
                }
                else {
                    this.showError(response.message);
                }
                
            },
            error: (error) => {
                console.error('Error opening the week:', error);
            }
        });
    }

    // Closes the week currently being viewed
    closeWeek() {
        //make a new request
        const week = new ClosedWeekRequest();
        week.date = this.currentDate;
        console.log("Closing week that contains this date:" + this.currentDate);

        //send request to backend
        this.timeEntryService.closeWeek(week).subscribe({
            next: (response) => {
                if (response.success) {
                   this.isClosed = true;
                    this.showSuccess("Week closed successfully!");
                    this.closeConfirmDialog = false;
                }
                else {
                    this.showError(response.message);
                }
                
            },
            error: (error) => {
                console.error('Error closing the week:', error);
            }
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

    // Function to show error message
    showError(msg: string) {
        this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: msg
        });
    }
}
