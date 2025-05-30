import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { Observable } from 'rxjs';
import { ApiResult } from '../models/apiResult';
import { UserSelectionDto } from '../models/UserSelectionDto';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  /*** API url */
  private apiUrl: string = "https://localhost:7062/api/user"

  constructor(
    private http: HttpClient,
    private applicationService: ApplicationSubjectService
  ) {
    applicationService.apiUrlSubject.subscribe(x => {
      this.apiUrl = `${x}/user`;
    });
  }

  checkEmailExists(email: string): Observable<{ success: boolean; message: string }> {
    return this.http.get<{ success: boolean; message: string }>(`${this.apiUrl}/checkEmailExists?email=${email}`);
  }

  getAllEmployees(): Observable<ApiResult<UserSelectionDto[]>> {
    return this.http.get<ApiResult<UserSelectionDto[]>>(`${this.apiUrl}/get-all-employees`);
  }
}
