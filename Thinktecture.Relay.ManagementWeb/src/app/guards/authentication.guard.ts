import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {SecurityService} from '../services/security.service';

@Injectable()
export class AuthenticationGuard implements CanActivate {
  constructor(private readonly _security: SecurityService, private readonly _router: Router) {
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
    if (this._security.authenticated) {
      return true;
    }

    this._router.navigate(['login']);
    return false;
  }
}
