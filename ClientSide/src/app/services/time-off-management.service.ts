import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { Observable } from 'rxjs';
import { ApiResult } from '../models/apiResult';
import { TimeOffRequestManagementDTO } from '../models/TimeOffRequestManagementDTO';
import { GetTimeOffRequestsRequest } from '../models/getTimeOffRequestsRequest';
import { ApproveTimeOffStatusRequest } from '../models/ApproveTimeOffStatusRequest';
import { RejectTimeOffStatusRequest } from '../models/RejectTimeOffStatusRequest';

@Injectable({
  providedIn: 'root'
})
export class TimeOffManagementService {

  private apiUrl: string = "";

  constructor(
    private http: HttpClient,
    private appService: ApplicationSubjectService
  ) {
    appService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/TimeOffManagement`;
    });
  }

  getAllTimeOffRequests(request: GetTimeOffRequestsRequest): Observable<ApiResult<TimeOffRequestManagementDTO[]>> {
    return this.http.post<ApiResult<TimeOffRequestManagementDTO[]>>(`${this.apiUrl}/get-time-off-requests`, request);
  }

  approveTimeOffStatus(request: ApproveTimeOffStatusRequest): Observable<ApiResult<boolean>> {
    return this.http.post<ApiResult<boolean>>(`${this.apiUrl}/approve-time-off-status`, request);
  }

  rejectTimeOffRequest(request: RejectTimeOffStatusRequest): Observable<ApiResult<boolean>> {
    return this.http.post<ApiResult<boolean>>(`${this.apiUrl}/reject-time-off-request`, request);
  }

}
