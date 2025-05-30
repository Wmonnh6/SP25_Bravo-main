import { Injectable } from '@angular/core';
import { ApplicationSubjectService } from './application-subject.service';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom, tap } from 'rxjs';
import { AppConfigModel } from '../models/appConfigModel';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AppConfigService {

    constructor(
        private applicationService: ApplicationSubjectService,
        private http: HttpClient
    ) {

    }

    async loadAppConfig(): Promise<void> {
        try {
            await firstValueFrom(                
                this.http.get<AppConfigModel>(`/assets/app-config${environment.production ? '.prod' : ''}.json`)
                    .pipe(tap(x => {
                        this.applicationService.apiUrlSubject.next(x.ApiUrl);
                        console.log('App Config Loaded:', x);
                    }))
            );
        } catch (error) {
            console.error('Failed to load app config', error);
        }
    }
}
