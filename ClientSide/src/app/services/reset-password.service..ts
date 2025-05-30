import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiResult } from '../models/apiResult';
import { ResetPasswordRequest } from '../models/resetPasswordRequest';
import { ApplicationSubjectService } from './application-subject.service';

@Injectable({
  providedIn: 'root'
})
export class ResetPasswordService {

  /*** API url */
  private apiUrl: string = "";

  constructor(
    private http: HttpClient,
    private applicationService: ApplicationSubjectService,

  ) {
    applicationService.apiUrlSubject.subscribe(x => {
      this.apiUrl = `${x}/user/ResetPassword`;
    });
  }

  //Send request to backend to reset the password
  resetPassword(request: ResetPasswordRequest) {
    return this.http.post<ApiResult<string>>(this.apiUrl, request);
  }
}
