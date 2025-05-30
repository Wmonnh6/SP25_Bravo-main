import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApplicationSubjectService {

/** Web API URL */
  apiUrlSubject: BehaviorSubject<string> = new BehaviorSubject<string>("");

  constructor() { }
}
