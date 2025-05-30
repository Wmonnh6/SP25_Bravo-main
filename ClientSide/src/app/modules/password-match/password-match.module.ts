/*
  Found algorithm and code on https://www.csharp.com/article/angular-custom-directive-for-confirm-password-validation/
*/

import { NgModule } from '@angular/core';
import { PasswordMatchDirective } from '../../directives/password-match.directive';

@NgModule({
  declarations: [],
  imports: [
    PasswordMatchDirective
  ],
  exports: [
    PasswordMatchDirective
  ]
})
export class PasswordMatchModule { }
