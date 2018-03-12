import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {environment} from '../../environments/environment';
import {SecurityService} from '../services/security.service';

@Injectable()
export class AuthorizationInterceptor implements HttpInterceptor {
  constructor(private readonly _security: SecurityService) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.startsWith(environment.backendUrl)) {
      req = req.clone({ setHeaders: this._security.authorizationHeader });
    }

    return next.handle(req);
  }
}
