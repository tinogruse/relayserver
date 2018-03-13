import {Directive, Input} from '@angular/core';
import {AbstractControl, AsyncValidator, NG_ASYNC_VALIDATORS, ValidationErrors} from '@angular/forms';
import {Observable} from 'rxjs/Observable';
import {map} from 'rxjs/operators';
import {BackendService} from '../services/backend.service';

@Directive({
  selector: '[trsUserNameAvailable]',
  providers: [
    { provide: NG_ASYNC_VALIDATORS, multi: true, useExisting: UserNameAvailableDirective },
  ],
})
export class UserNameAvailableDirective implements AsyncValidator {
  constructor(private readonly _backend: BackendService) {
  }

  @Input('trsUserNameAvailable') scope: 'user' | 'link';

  validate(control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> {
    return this._backend.ensureUserName(control.value, this.scope).pipe(
      map(available => available ? null : { available: true }),
    );
  }
}
