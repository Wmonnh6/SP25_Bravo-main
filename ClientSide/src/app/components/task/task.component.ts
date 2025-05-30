// component imports
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastModule } from 'primeng/toast';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { IftaLabelModule } from 'primeng/iftalabel';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';

// service imports
import { TaskService } from '../../services/task.service'; // Your TaskService
import { MessageService } from 'primeng/api'; // To show success/error messages

// model imports
import { Task } from '../../models/task';


@Component({
  selector: 'app-task',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    ToastModule,
    DialogModule,
    IftaLabelModule,
    InputTextModule,
    CheckboxModule
  ],
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss']
})
export class TaskComponent {

  tasks: Task[] = []; // Array to hold task data
  loading: boolean = false; // Loading state for UI
  status: string = ""; // To hold status messages

  // For the dialog
  displayDialog: boolean = false; // Controls visibility of the confirmation dialog
  taskToDelete: Task | null = null; // Holds the task data to be deleted
  // New task dialog
  newTaskDialog: boolean = false;
  newTask: Task = null;

  // Edit task dialog
  // Add just after your newTask property
  editTaskDialog: boolean = false;
  taskToEdit: Task = new Task(null, '', true, false);

  constructor(
    private taskService: TaskService, // Inject TaskService
    private messageService: MessageService // Inject MessageService for feedback
  ) { }

  // Lifecycle hook to fetch tasks when the component initializes
  ngOnInit() {
    this.fetchTasks();

    // initialize a new task object so we can bind the properties
    this.newTask = new Task(
      null,
      '',
      true,
      false
    );
  }

  // Function to fetch tasks from the backend
  fetchTasks() {
    this.loading = true;
    this.taskService.getAllTasks().subscribe({
      next: (tasks) => {
        this.tasks = tasks || []; // Assign the tasks to the component's tasks array
        this.loading = false;
        console.log("Tasks Length: ", this.tasks.length);
      },
      error: (err) => {
        console.log(err); // Log errors
        this.tasks = [];
        this.loading = false;
        this.showError('Failed to load tasks');
      }
    });
  }

  // Function to confirm deletion (opens the dialog)
  confirmDelete(task: Task) {
    this.taskToDelete = task; // Set the task to be deleted
    console.log("Task to delete: ", this.taskToDelete);
    this.displayDialog = true; // Show the confirmation dialog
  }

  // Function to cancel deletion
  cancelDelete() {
    this.displayDialog = false; // Hide the dialog
    this.taskToDelete = null; // Clear the task to be deleted
  }

  // Function to delete a task
  deleteTaskById(taskId: number) {
    console.log("Deleting task with ID: ", taskId);
    this.taskService.deleteTask(taskId).subscribe({
      next: () => {
        // Remove the task from the tasks array after deletion
        this.tasks = this.tasks.filter(task => task.id !== taskId);
        this.displayDialog = false;
        this.showSuccess('Task deleted successfully.');
      },
      error: (err) => {
        console.log("Error deleting task", err); // Log error
        this.showError('Failed to delete task');
        this.displayDialog = false;
      }
    });
  }

  // Function to handle task edit (for now, just a placeholder)
  editTask(taskId: number) {
    console.log('Edit task with ID:', taskId);

    const task = this.tasks.find(t => t.id === taskId);
    if (task) {
      // Create a copy of the task to avoid modifying the original until save
      this.taskToEdit = { ...task };
      this.editTaskDialog = true;
    } else {
      this.showError('Task not found');
    }
  }

  // Edit Task functions
  cancelEditTaskDialog() {
    this.editTaskDialog = false;
    this.taskToEdit = new Task(null, '', true, false);
  }

  updateTask() {
    if (!this.taskToEdit) return;

    this.taskService.updateTask(this.taskToEdit).subscribe({
      next: (response) => {
        // Update the task in the local array
        const index = this.tasks.findIndex(t => t.id === this.taskToEdit.id);
        if (index !== -1) {
          this.tasks[index] = response.data || this.taskToEdit;
        }
        this.showSuccess('Task updated successfully');
        this.editTaskDialog = false;
        this.taskToEdit = new Task(null, '', true, false);
      },
      error: (err) => {
        console.log('Error updating task', err);
        this.showError('Failed to update task');
      }
    });
  }

  // Add New Task functions
  showNewTaskDialog(form: NgForm) {
    form.resetForm();
    console.log("Resetting form and validations: " + form);
    this.newTaskDialog = true;
  }

  cancelNewTaskDialog(form: NgForm) {
    form.reset();
    console.log("Resetting form fields: " + form);
    this.newTaskDialog = false;
  }

  addNewTask(form: NgForm) {
    console.log("Sending request to add new task: " + this.newTask.name);
    this.taskService.addTask(this.newTask).subscribe({
      next: resp => {
        console.log('Succesfully created new task: ' + JSON.stringify(resp.data));

        this.showSuccess(resp.message);
        this.tasks.push(resp.data); // adds the newly created task to the current task list
        form.resetForm(); // reset the form and validations
        this.newTaskDialog = false; // close the dialog modal
      },
      error: err => {
        console.log('Error creating new task', err);

        const { Name, isTimeOff } = err.error.errors; // destructure the properties that have backend validation
        if (Name || isTimeOff) {
          if (Name) {
            Name.forEach(msg => this.showError(msg));
          }
          if (isTimeOff) {
            isTimeOff.forEach(msg => this.showError(msg));
          }
        } else {
          this.showError(err.message);
        }
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
