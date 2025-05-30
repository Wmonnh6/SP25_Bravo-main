import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { UserDto } from '../models/userDto';
import { Router } from '@angular/router';
import { LoginRequest } from '../models/loginRequest';
import { jwtDecode } from 'jwt-decode';
import { CreateAccountRequest } from '../models/createAccountRequest';
import { ApiResult } from '../models/apiResult';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  user$: BehaviorSubject<UserDto> = new BehaviorSubject<UserDto>(null);
  isLoggedIn$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  /** Token name */
  MTE_TOKEN: string = "MyTimeEntryToken";

  /*** API url */
  private apiUrl: string = "";

  constructor(
    private http: HttpClient,
    private applicationService: ApplicationSubjectService,
    private router: Router
  ) {
    applicationService.apiUrlSubject.subscribe(baseUrl => {
      this.apiUrl = `${baseUrl}/user`;
    });
  }

  userAccount(user: CreateAccountRequest) {
    return this.http.post<ApiResult<UserDto>>(this.apiUrl + "/register", user);
  }

  login(request: LoginRequest): Observable<ApiResult<UserDto>> {    
    return this.http.post<ApiResult<UserDto>>(this.apiUrl + "/login", request).pipe(
      map(res => {
        if (res.success) {
          const userInfo = this.decodeToken(res.data.token);
          localStorage.setItem(this.MTE_TOKEN, res.data.token)
          
          var user = this.mapTokenToUser(userInfo);
          
          this.user$.next(user);
          this.isLoggedIn$.next(true);
          
          return res; // Successfully logged in
        }
        return res; // Login failed
      })
    );
  }

  logout() {
    // the logic
    this.user$.next(null);
    this.isLoggedIn$.next(false);
    localStorage.removeItem(this.MTE_TOKEN);
    this.router.navigate(['/login']);
 }

  /** Read the token from the browser */
  getLocalToken(): string {
    return localStorage.getItem(this.MTE_TOKEN);
  }

  isAuthenticated(token: string): boolean {
    const user = this.decodeToken(token);
    return !!user && user.exp * 1000 > Date.now();
  }

  /** Get the user info from the JWT */
  getLocalUserInfo(): UserDto {
    const token = this.getLocalToken();
    const userInfo = this.decodeToken(token);

    if (this.isAuthenticated(token)) {
      var user = this.mapTokenToUser(userInfo);

      this.user$.next(user);
      this.isLoggedIn$.next(true);
  
      return user;
    }
    this.user$.next(null);
    this.isLoggedIn$.next(false);

    return null;
  }

  isAdmin(): boolean {
    return this.getLocalUserInfo().isAdmin;
  }

  updateToken(token: string) {
    localStorage.setItem(this.MTE_TOKEN, token)
    
    const userInfo = this.decodeToken(token);
    var user = this.mapTokenToUser(userInfo);    

    this.user$.next(user);
    this.isLoggedIn$.next(true);
  }

  /** { name, family_name, email, exp, isAdmin} */
  private decodeToken(token: string): any {
    if (!token) return null;
    try {
      return jwtDecode(token);
    } catch (error) {
      return null;
    }
  }

  private mapTokenToUser(token: any) {
    var user = new UserDto();
    user.firstName = token.name;
    user.lastName = token.family_name;
    user.email = token.email;
    user.isAdmin = token.isAdmin === "true";
    user.id = token.sub;

    return user;
  }
}
