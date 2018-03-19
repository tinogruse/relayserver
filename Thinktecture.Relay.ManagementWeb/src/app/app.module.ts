import {HTTP_INTERCEPTORS, HttpClient, HttpClientModule} from '@angular/common/http';
import {LOCALE_ID, NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatDialogModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatPaginatorModule,
  MatProgressSpinnerModule,
  MatSidenavModule,
  MatSortModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
} from '@angular/material';
import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {RouterModule} from '@angular/router';
import {TranslateLoader, TranslateModule, TranslateService} from '@ngx-translate/core';
import {TranslateHttpLoader} from '@ngx-translate/http-loader';
import {ChartsModule} from 'ng2-charts';

import {AppComponent} from './app.component';
import {ChartComponent} from './components/chart/chart.component';
import {DashboardChartComponent} from './components/dashboard-chart/dashboard-chart.component';
import {DashboardLogComponent} from './components/dashboard-log/dashboard-log.component';
import {DashboardComponent} from './components/dashboard/dashboard.component';
import {LinkChartComponent} from './components/link-chart/link-chart.component';
import {LinkInfoComponent} from './components/link-info/link-info.component';
import {LinkLogComponent} from './components/link-log/link-log.component';
import {LinkTraceLogComponent} from './components/link-trace-log/link-trace-log.component';
import {LinkTraceComponent} from './components/link-trace/link-trace.component';
import {LinkComponent} from './components/link/link.component';
import {LinksComponent} from './components/links/links.component';
import {LoginComponent} from './components/login/login.component';
import {MainComponent} from './components/main/main.component';
import {RequestLogComponent} from './components/request-log/request-log.component';
import {SetupComponent} from './components/setup/setup.component';
import {SimpleDialogComponent} from './components/simple-dialog/simple-dialog.component';
import {SpinnerOverlayComponent} from './components/spinner-overlay/spinner-overlay.component';
import {UsersComponent} from './components/users/users.component';
import {DialogCloseDirective} from './directives/dialog-close.directive';
import {EqualsDirective} from './directives/equals.directive';
import {UserNameAvailableDirective} from './directives/user-name-available.directive';
import {AuthenticationGuard} from './guards/authentication.guard';
import {FirstTimeSetupGuard} from './guards/first-time-setup.guard';
import {AuthorizationInterceptor} from './interceptors/authorization.interceptor';
import {BackendService} from './services/backend.service';
import {SecurityService} from './services/security.service';

export function localeIdFactory(translate: TranslateService) {
  const match = window.location.search.match(/[?&]localeid=(.{2,}?-.{2,}?)(?:&|$)/i);
  return match && match[1] || translate.getBrowserCultureLang();
}

export function createTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    MainComponent,
    SimpleDialogComponent,
    SetupComponent,
    SpinnerOverlayComponent,
    EqualsDirective,
    DashboardComponent,
    UsersComponent,
    DialogCloseDirective,
    LinksComponent,
    UserNameAvailableDirective,
    LinkComponent,
    ChartComponent,
    RequestLogComponent,
    DashboardChartComponent,
    DashboardLogComponent,
    LinkChartComponent,
    LinkInfoComponent,
    LinkLogComponent,
    LinkTraceComponent,
    LinkTraceLogComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,

    TranslateModule.forRoot({
      loader: { provide: TranslateLoader, useFactory: createTranslateLoader, deps: [HttpClient] },
    }),

    ChartsModule,

    RouterModule.forRoot([
      { path: '', pathMatch: 'full', redirectTo: 'main' },
      { path: 'setup', component: SetupComponent },
      { path: 'login', canActivate: [FirstTimeSetupGuard], component: LoginComponent },
      {
        path: 'main', canActivate: [AuthenticationGuard], component: MainComponent, children: [
          { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
          {
            path: 'dashboard', component: DashboardComponent, children: [
              { path: '', pathMatch: 'full', redirectTo: 'chart' },
              { path: 'chart', component: DashboardChartComponent },
              { path: 'log', component: DashboardLogComponent },
            ],
          },
          { path: 'users', component: UsersComponent },
          {
            path: 'links', children: [
              { path: '', pathMatch: 'full', component: LinksComponent },
              {
                path: ':id', component: LinkComponent, children: [
                  { path: '', pathMatch: 'full', redirectTo: 'info' },
                  { path: 'info', component: LinkInfoComponent },
                  { path: 'chart', component: LinkChartComponent },
                  { path: 'log', component: LinkLogComponent },
                  { path: 'trace', component: LinkTraceComponent },
                  { path: 'trace/:id', component: LinkTraceLogComponent },
                ],
              },
            ],
          },
        ],
      },
    ], { useHash: true, paramsInheritanceStrategy: 'always' }),

    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatSidenavModule,
    MatListModule,
    MatInputModule,
    MatCheckboxModule,
    MatDialogModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTableModule,
    MatTooltipModule,
    MatMenuModule,
    MatSortModule,
    MatPaginatorModule,
  ],
  providers: [
    { provide: LOCALE_ID, useFactory: localeIdFactory, deps: [TranslateService] },
    { provide: HTTP_INTERCEPTORS, multi: true, useClass: AuthorizationInterceptor },

    SecurityService,
    BackendService,

    FirstTimeSetupGuard,
    AuthenticationGuard,
  ],
  entryComponents: [SimpleDialogComponent],
  bootstrap: [AppComponent],
})
export class AppModule {
}
