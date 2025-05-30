import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ApplicationSubjectService } from './application-subject.service';
import { MyTimeEntry } from '../models/myTimeEntry';
import { ApiResult } from '../models/apiResult';
import { TimeEntryRequest } from '../models/timeEntryRequest';
import { AuthService } from './auth.service';
import { ClosedWeekRequest } from '../models/ClosedWeekRequest';

@Injectable({
  providedIn: 'root'
})
export class TimeEntryService {
  private apiUrl = "";
  readonly authToken: string = '';

  constructor(private http: HttpClient,
    private appService: ApplicationSubjectService,
    readonly authService: AuthService) {
    appService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/timeEntry`;
    });
    this.authToken = authService.getLocalToken();
  }

  // Fetch time entries for the logged-in user, paginated by weeks
  getTimeEntries(date: Date): Observable<ApiResult<MyTimeEntry[]>> {
    const url = `${this.apiUrl}/getUserTimeEntries?date=${date.toISOString()}`;
    return this.http.get<any>(url).pipe(
      tap((data) => {
        console.log('Fetched Time Entries Backend:', data);
      })
    );
  }

  // Fetch time entries for the Employee user (Admin only), paginated by weeks
  getEmployeeTimeEntries(date: Date, userId: number): Observable<ApiResult<MyTimeEntry[]>> {
    const url = `${this.apiUrl}/getEmployeeTimeEntries?userId=${userId}&date=${date.toISOString()}`;
    return this.http.get<any>(url).pipe(
      tap((data) => {
        console.log('Fetched Employee Time Entries Backend:', data);
      })
    );
  }

  //Send request to backend to add a new time entry
  addTimeEntry(newEntry: TimeEntryRequest) {
    return this.http.post<ApiResult<MyTimeEntry>>(this.apiUrl + "/addTimeEntry",
      {
        Hours: newEntry.Hours,
        Date: newEntry.Date,
        Comment: newEntry.Comment,
        UserId: newEntry.UserId,
        TaskId: newEntry.TaskId,
        // id is not passed in here because we don't have one and it should be generated on the backend
        Id: null
      },
      {
        headers: { 'Authorization': 'Bearer ' + this.authToken }
      });
  }

  // Send request to update a time entry
  updateTimeEntry(entry: TimeEntryRequest) {
    // if admin use the /admin endpoint
    let urlSuffix = this.authService.isAdmin() ? '/admin' : '';
    const updateUrl = this.apiUrl + "/updateTimeEntry" + urlSuffix;
    return this.http.put<ApiResult<MyTimeEntry>>(
      updateUrl,
      {
        Id: entry.Id,
        Hours: entry.Hours,
        Date: entry.Date,
        Comment: entry.Comment,
        UserId: entry.UserId,
        TaskId: entry.TaskId
      }
    );
  }

  // Send request to backend to add a new time entry for another employee
  addOthersTimeEntry(newEntry: TimeEntryRequest) {
    return this.http.post<ApiResult<MyTimeEntry>>(this.apiUrl + "/addOthersTimeEntry",
      {
        Hours: newEntry.Hours,
        Date: newEntry.Date,
        Comment: newEntry.Comment,
        UserId: newEntry.UserId,
        TaskId: newEntry.TaskId,
        // id is not passed in here because we don't have one and it should be generated on the backend
        Id: null
      },
      {
        headers: { 'Authorization': 'Bearer ' + this.authToken }
      }
    );
  }

  // Method to delete a time entry
  deleteTimeEntry(timeEntryId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/deleteTimeEntry`, {
      body: { TimeEntryId: timeEntryId }
    });
  }

  checkWeekStatus(weekDate: Date): Observable<ApiResult<boolean>> {
    const checkUrl = `${this.apiUrl.substring(0, this.apiUrl.length - 9)}closedWeek/checkWeekStatus?date=${weekDate.toISOString()}`;
    return this.http.get<ApiResult<boolean>>(checkUrl);
  }

  closeWeek(week: ClosedWeekRequest): Observable<ApiResult<Date>> {
    const checkUrl = `${this.apiUrl.substring(0, this.apiUrl.length - 9)}closedWeek/closeWeek`;
    return this.http.post<ApiResult<Date>>(checkUrl, {
      date: week.date
    });
  }

  openWeek(week: ClosedWeekRequest): Observable<any> {
    const checkUrl = `${this.apiUrl.substring(0, this.apiUrl.length - 9)}closedWeek/openWeek`;
    return this.http.delete<any>(checkUrl, {
      body: { date: week.date }
    });
  }

  // Method for admins to delete any time entry
  deleteAnyTimeEntry(timeEntryId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/deleteAnyTimeEntry`, {
      body: { TimeEntryId: timeEntryId }
    });
  }
}