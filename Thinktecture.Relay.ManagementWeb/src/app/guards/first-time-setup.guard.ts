import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {BackendService} from '../services/backend.service';

@Injectable()
export class FirstTimeSetupGuard implements CanActivate {
  constructor(private readonly _backend: BackendService) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    return this._backend.ensureFirstTimeUser();
  }
}
