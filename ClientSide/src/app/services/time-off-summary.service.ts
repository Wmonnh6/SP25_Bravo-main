import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TimeOffSummary } from '../models/timeOffSummary'; // Import the model
import { ApiResult } from '../models/apiResult'; // API result model
import { ApplicationSubjectService } from './application-subject.service';

@Injectable({
  providedIn: 'root'
})
export class TimeOffSummaryService {

  private apiUrl: string = "";

  constructor(
    private http: HttpClient, 
    private appService: ApplicationSubjectService
  ) { 
    appService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/timeOffSummary`;
    });
  }

  // Fetch total time-off hours for all users
  getTimeOffSummary(month: Date): Observable<ApiResult<TimeOffSummary[]>> {
    return this.http.get<ApiResult<TimeOffSummary[]>>(`${this.apiUrl}/getTimeOffSummary?month=${month.toISOString()}`);
  }
}