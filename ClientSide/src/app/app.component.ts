import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { FooterComponent } from "./components/footer/footer.component";
import { AuthService } from './services/auth.service';

@Component({
	selector: 'app-root',
	imports: [RouterOutlet, NavMenuComponent, FooterComponent],
	templateUrl: './app.component.html',
	styleUrl: './app.component.scss'
})
export class AppComponent {
	title = 'My Time Entry';

	constructor(authService: AuthService) {
		authService.getLocalUserInfo();
	}
}
