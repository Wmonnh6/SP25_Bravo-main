import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { providePrimeNG } from 'primeng/config';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import Material from '@primeng/themes/material';
import { AppConfigService } from './services/app-config.service';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Card, CardModule } from 'primeng/card';
import { AuthInterceptor } from './auth.interceptor';
import { DialogModule } from 'primeng/dialog';
import { DatePipe } from '@angular/common';

const loadConfig = (configService: AppConfigService) => {
  return () => configService.loadAppConfig();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([AuthInterceptor])),
    MessageService,
    ButtonModule,
    TableModule,
    InputTextModule,
    FormsModule,
    Card,
    CardModule,
    ReactiveFormsModule,
    DialogModule,
    DatePipe,

    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Material
      }
    }),
    {
      provide: APP_INITIALIZER,
      useFactory: loadConfig,
      multi: true,
      deps: [AppConfigService]
    }
  ]
};
