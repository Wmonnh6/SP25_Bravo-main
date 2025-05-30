import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { Observable } from 'rxjs';
import { ApiResult } from '../models/apiResult';
import { MyTimeEntry } from '../models/myTimeEntry';
import { DeleteTimeOffRequest } from '../models/DeleteTimeOffRequest';

@Injectable({
  providedIn: 'root'
})
export class TimeOffRequestsService {

  private apiUrl: string = "";

  constructor(
    private http: HttpClient,
    private appService: ApplicationSubjectService
  ) {
    appService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/TimeOffRequests`;
    });
  }

  //Retrieve the current user's time off requests
  getUserTimeOffRequests(): Observable<ApiResult<MyTimeEntry[]>> {
    return this.http.get<ApiResult<MyTimeEntry[]>>(`${this.apiUrl}/getUserTimeOffRequests`);
  }

  //Delete a time off request
  deleteUserTimeOffRequest(request: DeleteTimeOffRequest): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/deleteUserTimeOffRequest`, {body: request});
  }

}