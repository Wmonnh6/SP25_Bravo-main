import { Routes } from '@angular/router';
import { RegisterFormComponent } from './components/register-form/register-form.component';
import { InviteUserComponent } from './components/invite-user/invite-user.component';
import { PasswordResetComponent } from './components/password-reset/password-reset.component';
import { LoginComponent } from './components/login/login.component';
import { authGuard, authGuardAdmin } from './auth.guard';
import { MyTimeEntriesComponent } from './components/my-time-entries/my-time-entries.component';
import { TaskComponent } from './components/task/task.component';
import { SetNewPasswordComponent } from './components/set-new-password/set-new-password.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { CalendarViewComponent } from './components/calendar-view/calendar-view.component';
import { TimeOffChartComponent } from './components/time-off-chart/time-off-chart.component';
import { TimeOffManagementComponent } from './components/time-off-management/time-off-management.component';
import { MyTimeOffRequestsComponent } from './components/my-time-off-requests/my-time-off-requests.component';

export const routes: Routes = [
    { path: "", component: MyTimeEntriesComponent, pathMatch: "full", canActivate: [authGuard] },
    { path: "invite-user", component: InviteUserComponent, canActivate: [authGuardAdmin] },
    { path: "password-reset", component: PasswordResetComponent },
    { path: "user-profile", component: UserProfileComponent, canActivate: [authGuard] },
    { path: "register", component: RegisterFormComponent },
    { path: "login", component: LoginComponent },
    { path: "task", component: TaskComponent, canActivate: [authGuardAdmin] },
    { path: "set-new-password", component: SetNewPasswordComponent },
    { path: "calendar-view", component: CalendarViewComponent, canActivate: [authGuard] },
    { path: "time-off-chart", component: TimeOffChartComponent, canActivate: [authGuardAdmin]},
    { path: "time-off-management", component: TimeOffManagementComponent, canActivate: [authGuardAdmin]},
    { path: "my-time-off-requests", component: MyTimeOffRequestsComponent, canActivate: [authGuard]},
    { path: '**', redirectTo: 'login' }
];