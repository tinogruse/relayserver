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
  private _accessToken: string | null = localStorage.getItem('access_token');
  private _userName: string | null = localStorage.getItem('user_name');

  constructor(private readonly _httpClient: HttpClient, private readonly _matDialog: MatDialog) {
  }

  get authorizationHeader(): { [header: string]: string } {
    return { 'Authorization': `bearer ${this._accessToken}` };
  }

  get authenticated(): boolean {
    return this._accessToken !== null;
  }

  get userName(): string | null {
    return this._userName;
  }

  authenticate(userName: string, password: string, rememberMe: boolean): Observable<void> {
    const body = new HttpParams({ fromObject: { grant_type: 'password', username: userName, password } });
    return this._httpClient.post<TokenResponse>(`${environment.backendUrl}token`, body).pipe(
      map(response => {
        this._accessToken = response.access_token;
        this._userName = userName;

        if (rememberMe) {
          localStorage.setItem('access_token', response.access_token);
          localStorage.setItem('user_name', userName);
        } else {
          localStorage.removeItem('access_token');
          localStorage.removeItem('user_name');
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

  deauthenticate() {
    this._userName = this._accessToken = null;
    localStorage.clear();
  }
}
