import {HttpClient, HttpErrorResponse, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MatDialog, MatDialogConfig} from '@angular/material';
import {Observable} from 'rxjs/Observable';
import {_throw} from 'rxjs/observable/throw';
import {catchError, map} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SimpleDialogComponent} from '../components/dialogs/simple-dialog/simple-dialog.component';

interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

@Injectable()
export class SecurityService {
  private _accessToken: string | null = null;

  constructor(private readonly _httpClient: HttpClient, private readonly _matDialog: MatDialog) {
    this._accessToken = localStorage.getItem('access_token');
  }

  get authorizationHeader(): { [header: string]: string } {
    return { 'Authorization': `bearer ${this._accessToken}` };
  }

  get authenticated(): boolean {
    return this._accessToken !== null;
  }

  get username(): string | null {
    return localStorage.getItem('username');
  }

  set username(value: string | null) {
    if (value === null) {
      localStorage.removeItem('username');
    } else {
      localStorage.setItem('username', value);
    }
  }

  authenticate(username: string, password: string, rememberMe: boolean): Observable<void> {
    const body = new HttpParams({ fromObject: { grant_type: 'password', username, password } });
    return this._httpClient.post<TokenResponse>(`${environment.backendUrl}token`, body).pipe(
      map(response => {
        this._accessToken = response.access_token;

        this.username = rememberMe ? username : null;
        if (rememberMe) {
          localStorage.setItem('access_token', response.access_token);
        } else {
          localStorage.removeItem('access_token');
        }
      }),

      catchError(response => {
        const config: MatDialogConfig = { data: { title: 'Login failed' } };

        config.data.content = (response instanceof HttpErrorResponse) && response.status === 400
          ? 'The login is not possible because the credentials are invalid.'
          : response;

        this._matDialog.open(SimpleDialogComponent, config);

        return _throw(response);
      }),
    );
  }
}
