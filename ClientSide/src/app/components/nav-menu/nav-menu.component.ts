import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { UserDto } from '../../models/userDto';
import { CommonModule } from '@angular/common';
import { MenubarModule } from 'primeng/menubar';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { Router } from '@angular/router';


@Component({
    selector: 'app-nav-menu',
    imports: [CommonModule, MenubarModule, ButtonModule],
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.scss']

})
export class NavMenuComponent {
    user: UserDto = null;
    items: MenuItem[];
    loginItem: any = null;

    constructor(
        private authService: AuthService,
        private router: Router
    ) {
        this.authService.user$.subscribe(user => {
            this.user = user;
            this.buildMenu();
        });
    }

    ngOnInit() {
    }

    buildMenu() {
        //user is logged in
        if (this.user != null) {
            this.user.firstName = this.user.firstName;
            this.user.lastName = this.user.lastName;
            this.user.isAdmin = this.user.isAdmin;

            this.loginItem = {
                //set the loginItem to be a logout button
                label: 'Logout',
                command: () => this.authService.logout(),
                icon: 'pi pi-sign-out'
            }

            this.items = [
                {
                    label: 'My Time Entry',
                    routerLink: ['/']
                },
                {
                    label: 'My Time Off Requests',
                    routerLink: ['/my-time-off-requests']
                },
                {
                    label: 'Calendar',
                    routerLink: ['/calendar-view']
                },
                {
                    label: 'Tasks',
                    routerLink: ['/task'],
                    visible: this.user.isAdmin
                },
                {
                    label: 'Time Off Chart',
                    routerLink: ['/time-off-chart'],
                    visible: this.user.isAdmin
                },
                {
                    label: 'Time Off Management',
                    routerLink: ['/time-off-management'],
                    visible: this.user.isAdmin
                },
                {
                    label: 'Invite',
                    routerLink: ['/invite-user'],
                    visible: this.user.isAdmin
                },
                {
                    label: 'My Profile',
                    routerLink: ['/user-profile'],
                }
            ]
        }
        //user isn't logged in
        else {
            this.loginItem = {
                //set the loginItem to be a login button
                label: 'Login',
                command: () => this.router.navigate(['/login']),
                icon: 'pi pi-sign-in'
            };

            this.items = [];
        }
    }
}
