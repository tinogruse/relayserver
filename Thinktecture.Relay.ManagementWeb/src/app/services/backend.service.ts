import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MatDialog, MatDialogConfig} from '@angular/material';
import {Router} from '@angular/router';
import * as moment from 'moment';
import {Observable} from 'rxjs/Observable';
import {of} from 'rxjs/observable/of';
import {_throw} from 'rxjs/observable/throw';
import {catchError, map, mapTo, switchMap, tap} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SimpleDialogComponent} from '../components/dialogs/simple-dialog/simple-dialog.component';
import {Dashboard} from '../models/dashboard';
import {User} from '../models/user';

const backendUrl = `${environment.backendUrl}api/managementweb/`;

@Injectable()
export class BackendService {
  constructor(private readonly _httpClient: HttpClient, private readonly _matDialog: MatDialog, private readonly _router: Router) {
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

  createFirstTimeUser(userName: string, password: string): Observable<void> {
    return this._httpClient.post(`${backendUrl}user/firsttime`, { userName, password }).pipe(
      switchMap(() => this._matDialog.open(SimpleDialogComponent, { data: { content: 'User was created successfully. You can now log in.' } }).afterClosed()), // tslint:disable:max-line-length
      map(() => {
        this._router.navigate(['login'], { queryParams: { userName }, replaceUrl: true, skipLocationChange: true });
      }),
      catchError(response => {
        const config: MatDialogConfig = { data: { title: 'Creation failed' } };

        if (response instanceof HttpErrorResponse) {
          switch (response.status) {
            case 403: // Forbidden
              config.data.content = 'There is already an existing user.';
              this._router.navigate(['login'], { queryParams: { userName }, replaceUrl: true, skipLocationChange: true });
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
    return this._httpClient.get<Dashboard>(`${backendUrl}dashboard/info`).pipe(
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

  getUsers(): Observable<User[]> {
    return this._httpClient.get<User[]>(`${backendUrl}user/users`).pipe(
      tap(users => {
        users.forEach(user => {
          user.creationDate = moment(user.creationDate);

          if (user.lockedUntil) {
            user.lockedUntil = moment(user.lockedUntil);
          }
        });
      }),
    );
  }

  addUser(user: User): Observable<void> {
    return this._httpClient.post<void>(`${backendUrl}user/user`, user);
  }

  updateUser(user: User): Observable<void> {
    return this._httpClient.put<void>(`${backendUrl}user/user`, user);
  }

  deleteUser(user: User): Observable<void> {
    return this._httpClient.delete<void>(`${backendUrl}user/user`, { params: { id: user.id } });
  }
}
