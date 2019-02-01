import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MatDialog, MatDialogConfig} from '@angular/material';
import {Router} from '@angular/router';
import * as moment from 'moment';
import {Observable, of, throwError} from 'rxjs';
import {catchError, map, mapTo, switchMap, tap} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SimpleDialogComponent} from '../components/simple-dialog/simple-dialog.component';
import {ContentBytesChartDataItem, processContentBytesChartDataItem} from '../models/content-bytes-chart-data-item';
import {Dashboard} from '../models/dashboard';
import {Link, LinkCreate, LinkDetail} from '../models/link';
import {PageRequest} from '../models/page-request';
import {PageResult} from '../models/page-result';
import {processRequestLogEntry, RequestLogEntry} from '../models/request-log-entry';
import {User} from '../models/user';

const backendUrl = `${environment.backendUrl}api/managementweb/`;

export interface ErrorResult {
  errors?: string[];
}

@Injectable({ providedIn: 'root' })
export class BackendService {
  constructor(private readonly httpClient: HttpClient, private readonly matDialog: MatDialog, private readonly router: Router) {
  }

  ensureFirstTimeUser(): Observable<boolean> {
    return this.httpClient.get<{ setup: boolean }>(`${backendUrl}setup/needsfirsttimesetup`).pipe(
      map(result => result.setup),
    );
  }

  createFirstTimeUser(userName: string, password: string): Observable<void> {
    return this.httpClient.post(`${backendUrl}user/firsttime`, { userName, password }).pipe(
      switchMap(() => this.matDialog.open(SimpleDialogComponent, { data: { content: 'User was created successfully. You can now log in.' } }).afterClosed()), // tslint:disable:max-line-length
      map(() => {
        this.router.navigate(['login'], { queryParams: { userName }, replaceUrl: true, skipLocationChange: true });
      }),
      catchError(response => {
        const config: MatDialogConfig = { data: { title: 'Creation failed' } };

        if (response instanceof HttpErrorResponse) {
          switch (response.status) {
            case 403: // Forbidden
              config.data.content = 'There is already an existing user.';
              this.router.navigate(['login'], { queryParams: { userName }, replaceUrl: true, skipLocationChange: true });
              break;
            case 400: // Bad Request
              config.data.contents = response.error.message.split('\n');
              break;

            default:
              config.data.content = response.message;
              break;
          }
        }

        this.matDialog.open(SimpleDialogComponent, config);

        return throwError(response);
      }),
    );
  }

  ensureUserName(userName: string, scope: 'user' | 'link'): Observable<boolean> {
    return this.httpClient.get(`${backendUrl}${scope}/userNameAvailability`, { params: { userName } }).pipe(
      mapTo(true),
      catchError(() => of(false)),
    );
  }

  dashboardInfo(): Observable<Dashboard> {
    return this.httpClient.get<Dashboard>(`${backendUrl}dashboard/info`).pipe(
      tap(dashboard => {
        dashboard.logs.forEach(log => processRequestLogEntry(log));
        dashboard.chart.forEach(chart => processContentBytesChartDataItem(chart));
      }),
    );
  }

  getUsers(): Observable<User[]> {
    return this.httpClient.get<User[]>(`${backendUrl}user/users`).pipe(
      tap(users => users.forEach(user => user.lockedUntil = user.lockedUntil && moment(user.lockedUntil))),
    );
  }

  createUser(user: User): Observable<ErrorResult> {
    return this.httpClient.post<ErrorResult>(`${backendUrl}user/user`, user);
  }

  updateUser(user: User): Observable<ErrorResult> {
    return this.httpClient.put<ErrorResult>(`${backendUrl}user/user`, user);
  }

  deleteUser(user: User): Observable<ErrorResult> {
    return this.httpClient.delete<ErrorResult>(`${backendUrl}user/user`, { params: { id: user.id } });
  }

  getLinks(page: PageRequest): Observable<PageResult<Link>> {
    return this.httpClient.get<PageResult<Link>>(`${backendUrl}link/links`, { params: page as any }).pipe(
      tap(result => result.items.forEach(link => link.creationDate = moment(link.creationDate))),
    );
  }

  getLink(id: string): Observable<LinkDetail> {
    return this.httpClient.get<LinkDetail>(`${backendUrl}link/link`, { params: { id } }).pipe(
      tap(link => {
        link.creationDate = moment(link.creationDate);
        link.tokenRefreshWindow = link.tokenRefreshWindow && moment.duration(link.tokenRefreshWindow);
        link.heartbeatInterval = link.heartbeatInterval && moment.duration(link.heartbeatInterval);
        link.reconnectMinWaitTime = link.reconnectMinWaitTime && moment.duration(link.reconnectMinWaitTime);
        link.reconnectMaxWaitTime = link.reconnectMaxWaitTime && moment.duration(link.reconnectMaxWaitTime);
        link.absoluteConnectionLifetime = link.absoluteConnectionLifetime && moment.duration(link.absoluteConnectionLifetime);
        link.slidingConnectionLifetime = link.slidingConnectionLifetime && moment.duration(link.slidingConnectionLifetime);
      }),
    );
  }

  createLink(link: Link): Observable<LinkCreate & ErrorResult> {
    return this.httpClient.post<LinkCreate & ErrorResult>(`${backendUrl}link/link`, link);
  }

  deleteLink(link: Link): Observable<ErrorResult> {
    return this.httpClient.delete<ErrorResult>(`${backendUrl}link/link`, { params: { id: link.id } });
  }

  getChart(id: string): Observable<ContentBytesChartDataItem[]> {
    return this.httpClient.get<ContentBytesChartDataItem[]>(`${backendUrl}log/chartcontentbytes`, { params: { id } }).pipe(
      tap(result => result.forEach(chart => processContentBytesChartDataItem(chart))),
    );
  }

  getLog(id: string): Observable<RequestLogEntry[]> {
    return this.httpClient.get<RequestLogEntry[]>(`${backendUrl}log/recentlog`, { params: { id } }).pipe(
      tap(result => result.forEach(log => processRequestLogEntry(log))),
    );
  }
}
