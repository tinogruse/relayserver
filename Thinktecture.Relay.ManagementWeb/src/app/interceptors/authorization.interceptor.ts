import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Router} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {tap} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SecurityService} from '../services/security.service';

@Injectable()
export class AuthorizationInterceptor implements HttpInterceptor {
  constructor(private readonly _security: SecurityService, private readonly _router: Router) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.startsWith(environment.backendUrl)) {
      return next.handle(req.clone({ setHeaders: this._security.authorizationHeader })).pipe(
        tap({
          error: error => {
            if (error instanceof HttpErrorResponse) {
              if (error.status === 401) {
                this._security.deauthenticate();
                this._router.navigate(['login']);
              }
            }
          },
        }),
      );
    }

    return next.handle(req);
  }
}
