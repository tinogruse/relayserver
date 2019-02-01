import {HttpClient, HttpErrorResponse, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {MatDialog, MatDialogConfig} from '@angular/material';
import {Observable, throwError} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {environment} from '../../environments/environment';
import {SimpleDialogComponent} from '../components/simple-dialog/simple-dialog.component';

interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

@Injectable({ providedIn: 'root' })
export class SecurityService {
  private accessToken: string | null = localStorage.getItem('access_token');

  readonly userName: string | null = localStorage.getItem('user_name');

  constructor(private readonly httpClient: HttpClient, private readonly matDialog: MatDialog) {
  }

  get authorizationHeader(): { [header: string]: string } {
    return { 'Authorization': `bearer ${this.accessToken}` };
  }

  get authenticated(): boolean {
    return this.accessToken !== null;
  }

  authenticate(userName: string, password: string, rememberMe: boolean): Observable<void> {
    const body = new HttpParams({ fromObject: { grant_type: 'password', username: userName, password } });
    return this.httpClient.post<TokenResponse>(`${environment.backendUrl}token`, body).pipe(
      map(response => {
        this.accessToken = response.access_token;
        (this as any).userName = userName;

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

        this.matDialog.open(SimpleDialogComponent, config);

        return throwError(response);
      }),
    );
  }

  deauthenticate() {
    (this as any).userName = this.accessToken = null;
    localStorage.clear();
  }
}
