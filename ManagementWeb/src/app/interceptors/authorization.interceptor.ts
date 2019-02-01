import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Router} from '@angular/router';
import {Observable} from 'rxjs';
import {tap} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SecurityService} from '../services/security.service';

@Injectable()
export class AuthorizationInterceptor implements HttpInterceptor {
  constructor(private readonly security: SecurityService, private readonly router: Router) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.startsWith(environment.backendUrl)) {
      return next.handle(req.clone({ setHeaders: this.security.authorizationHeader })).pipe(
        tap({
          error: error => {
            if (error instanceof HttpErrorResponse) {
              if (error.status === 401) {
                this.security.deauthenticate();
                this.router.navigate(['login']);
              }
            }
          },
        }),
      );
    }

    return next.handle(req);
  }
}
