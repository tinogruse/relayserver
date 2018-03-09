import {Directive, Input} from '@angular/core';
import {AbstractControl, NG_VALIDATORS, ValidationErrors, Validator} from '@angular/forms';

@Directive({
  selector: '[trsEquals]',
  providers: [
    { provide: NG_VALIDATORS, multi: true, useExisting: EqualsDirective },
  ],
})
export class EqualsDirective implements Validator {
  private _other: any;
  private _control: AbstractControl;

  @Input('trsEquals') set other(value: any) {
    this._other = value;

    if (this._control) {
      this._control.updateValueAndValidity();
    }
  }

  validate(control: AbstractControl): ValidationErrors | null {
    this._control = control;
    return !control.value || !this._other || control.value === this._other ? null : { equals: true };
  }
}
