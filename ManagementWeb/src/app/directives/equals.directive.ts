import {Directive, Input} from '@angular/core';
import {AbstractControl, NG_VALIDATORS, ValidationErrors, Validator} from '@angular/forms';

@Directive({
  selector: '[trsEquals]',
  providers: [
    { provide: NG_VALIDATORS, multi: true, useExisting: EqualsDirective },
  ],
})
export class EqualsDirective implements Validator {
  private otherValue: any;
  private control: AbstractControl;

  @Input('trsEquals') set other(value: any) {
    this.otherValue = value;

    if (this.control) {
      this.control.updateValueAndValidity();
    }
  }

  validate(control: AbstractControl): ValidationErrors | null {
    this.control = control;
    return !control.value || !this.otherValue || control.value === this.otherValue ? null : { equals: true };
  }
}
