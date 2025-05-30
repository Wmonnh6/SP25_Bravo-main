import { Directive } from '@angular/core';
import { AbstractControl, 
        NG_VALIDATORS, 
        ValidationErrors,
        Validator,
        ValidatorFn 
      } from '@angular/forms';

export const passwordMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const newPass = control.get('new-password');
  const confirmPass = control.get('confirm-password');

  if(newPass && confirmPass && newPass.value !== confirmPass.value) {
    control.get('confirm-password').setErrors({'incorrect':true});
    return { notMatch: true};
  } else {
    return null;
  }
}

@Directive({
  selector: '[match-password]',
  providers: [{provide: NG_VALIDATORS, useExisting:PasswordMatchDirective, multi:true}]
})
export class PasswordMatchDirective implements Validator {

  validate(control: AbstractControl): ValidationErrors | null {
    return passwordMatchValidator(control);
  }
}
