import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Task } from '../models/task'; // Task model
import { ApiResult } from '../models/apiResult'; // API result model
import { ApplicationSubjectService } from './application-subject.service';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private apiUrl: string = "";

  constructor(
    private http: HttpClient, 
    private appService: ApplicationSubjectService
  ) { 
    appService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/Task`;
    });
  }

  // Fetch all tasks
  getAllTasks(): Observable<Task[]> {
    return this.http.get<ApiResult<Task[]>>(`${this.apiUrl}/getAllTasks`)
    .pipe(map(response => response.data));
  }

  addTask(task: Task): Observable<ApiResult<Task>> {
    console.log('Sending request for Task: ' + task);
    return this.http.post<ApiResult<Task>>(`${this.apiUrl}/addTask`, { 
        Name: task.name,
        isTimeOff: task.isTimeOff? true: false, // while the linter claims this is unnecessary, assigning just the value of the boolean from the model fails validation on the backend bc .Net isn't parsing that boolean correctly into the backend request object
        isActive: true
      });
  }

  // Delete a task by ID
  deleteTask(taskId: number): Observable<ApiResult<string>> {
    return this.http.delete<ApiResult<string>>(`${this.apiUrl}/deleteTask`, { body: { TaskId: taskId } });
  }

  // Update a task
  updateTask(task: Task): Observable<ApiResult<Task>> {
    console.log('Sending request to update Task: ' + task.id);
    return this.http.put<ApiResult<Task>>(`${this.apiUrl}/updateTask`, {
      Id: task.id,
      Name: task.name,
      isTimeOff: task.isTimeOff ? true : false, // Same handling as in addTask
      isActive: task.isActive
    });
  }
}
