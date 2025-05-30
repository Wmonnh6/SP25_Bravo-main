import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { InvitationRequest } from '../models/invitationRequest';
import { ApplicationSubjectService } from './application-subject.service';
import { ApiResult } from '../models/apiResult';

@Injectable({
  providedIn: 'root'
})
export class InviteService {

  /** API endpoint url for invites */
  private apiUrl: string = "";

  constructor(
    readonly http: HttpClient,
    readonly applicationService: ApplicationSubjectService
  ) {
    // Set the url for this service's api endpoint at the backend
    applicationService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/user/invite-user`;
    });
  }

  // function to request the backend to create the invite
  createInvite(request: InvitationRequest) {
    return this.http.post<ApiResult<string>>(this.apiUrl, {
      email: request.email,
      isAdmin: request.isAdmin
    });
  }
}
