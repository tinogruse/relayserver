import {Directive, Input} from '@angular/core';
import {AbstractControl, AsyncValidator, NG_ASYNC_VALIDATORS, ValidationErrors} from '@angular/forms';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {BackendService} from '../services/backend.service';

@Directive({
  selector: '[trsUserNameAvailable]',
  providers: [
    { provide: NG_ASYNC_VALIDATORS, multi: true, useExisting: UserNameAvailableDirective },
  ],
})
export class UserNameAvailableDirective implements AsyncValidator {
  constructor(private readonly backend: BackendService) {
  }

  @Input('trsUserNameAvailable') scope: 'user' | 'link';

  validate(control: AbstractControl): Promise<ValidationErrors | null> | Observable<ValidationErrors | null> {
    return this.backend.ensureUserName(control.value, this.scope).pipe(
      map(available => available ? null : { available: true }),
    );
  }
}
