import {Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree} from '@angular/router';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {BackendService} from '../services/backend.service';

@Injectable({ providedIn: 'root' })
export class FirstTimeSetupGuard implements CanActivate {
  constructor(private readonly backend: BackendService, private readonly router: Router) {
  }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.backend.ensureFirstTimeUser().pipe(
      map(setup => setup ? this.router.createUrlTree(['setup']) : true),
    );
  }
}
