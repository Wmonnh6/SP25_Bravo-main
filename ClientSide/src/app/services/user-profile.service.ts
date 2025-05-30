import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UpdateProfileRequest } from '../models/updateProfileRequest';
import { ApplicationSubjectService } from './application-subject.service';
import { ApiResult } from '../models/apiResult';
import { UserDto } from '../models/userDto';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {

  private apiUrl: string = "";
  readonly authToken: string = "";

  constructor(
    readonly http: HttpClient,
    readonly applicationService: ApplicationSubjectService,
    readonly authservice: AuthService
  ) { 
    applicationService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/user/update-user`;
    });
    this.authToken = authservice.getLocalToken();
  }

  updateUserProfile(request: UpdateProfileRequest): Observable<ApiResult<UserDto>> {
    return this.http.put<ApiResult<UserDto>>(this.apiUrl, {
      email: request.email,
      firstName: request.firstName,
      lastName: request.lastName,
      newPassword: request.newPass === ""? null : request.newPass
    },
    {
      headers: {'Authorization': 'Bearer ' + this.authToken }
    });
  }
}
