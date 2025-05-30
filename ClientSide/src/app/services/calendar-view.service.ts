import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { Observable } from 'rxjs';
import { ApiResult } from '../models/apiResult';
import { TimeOffRequest } from '../models/timeOffRequest';

@Injectable({
    providedIn: 'root'
})
export class CalendarViewService {

    private apiUrl: string = "";

    constructor(
        private http: HttpClient,
        private appService: ApplicationSubjectService
    ) {
        appService.apiUrlSubject.subscribe(baseUrl => {
            this.apiUrl = `${baseUrl}/calendar`;
        });
    }

    getAllEmployees(): Observable<ApiResult<TimeOffRequest[]>> {
        return this.http.get<ApiResult<TimeOffRequest[]>>(`${this.apiUrl}/get-all-time-off-requests`);
    }
}
