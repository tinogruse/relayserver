import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MatDialog, MatDialogConfig} from '@angular/material';
import {Router} from '@angular/router';
import {Observable} from 'rxjs/Observable';
import {of} from 'rxjs/observable/of';
import {_throw} from 'rxjs/observable/throw';
import {catchError, map, mapTo, switchMap, tap} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SimpleDialogComponent} from '../components/dialogs/simple-dialog/simple-dialog.component';
import {Dashboard} from '../models/dashboard';
import {SecurityService} from './security.service';
import moment = require('moment');

const backendUrl = `${environment.backendUrl}api/managementweb/`;

@Injectable()
export class BackendService {
  constructor(
    private readonly _httpClient: HttpClient,
    private readonly _security: SecurityService,
    private readonly _matDialog: MatDialog,
    private readonly _router: Router,
  ) {
  }

  ensureFirstTimeUser(): Observable<boolean> {
    return this._httpClient.get(`${backendUrl}setup/needsfirsttimesetup`).pipe(
      mapTo(true),
      catchError(() => {
        this._router.navigate(['setup']);
        return of(false);
      }),
    );
  }

  createFirstTimeUser(username: string, password: string): Observable<void> {
    return this._httpClient.post(`${backendUrl}user/firsttime`, { username, password }).pipe(
      switchMap(() => this._matDialog.open(SimpleDialogComponent, { data: { content: 'User was created successfully. You can now log in.' } }).afterClosed()), // tslint:disable:max-line-length
      map(() => {
        this._security.username = username;
        this._router.navigate(['login'], { replaceUrl: true });
      }),
      catchError(response => {
        const config: MatDialogConfig = { data: { title: 'Creation failed' } };

        if (response instanceof HttpErrorResponse) {
          switch (response.status) {
            case 403: // Forbidden
              config.data.content = 'There is already an existing user.';
              this._router.navigate(['login'], { replaceUrl: true });
              break;
            case 400: // Bad Request
              break;

            default:
              config.data.content = response.message;
              break;
          }
        }

        this._matDialog.open(SimpleDialogComponent, config);

        return _throw(response);
      }),
    );
  }

  dashboardInfo(): Observable<Dashboard> {
    return this._httpClient.get<Dashboard>(`${backendUrl}dashboard/info`, { headers: this._security.authorizationHeader }).pipe(
      tap(dashboard => {
        dashboard.logs.forEach(log => {
          log.onPremiseConnectorInDate = moment(log.onPremiseConnectorInDate);
          log.onPremiseConnectorOutDate = moment(log.onPremiseConnectorOutDate);

          if (log.onPremiseTargetInDate) {
            log.onPremiseTargetInDate = moment(log.onPremiseTargetInDate);
          }
          if (log.onPremiseTargetOutDate) {
            log.onPremiseTargetOutDate = moment(log.onPremiseTargetOutDate);
          }
        });
        dashboard.chart.forEach(chart => chart.key = moment(chart.key));
      }),
    );
  }
}
