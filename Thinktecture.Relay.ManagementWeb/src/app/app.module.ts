import {HttpClient, HttpClientModule} from '@angular/common/http';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatDialogModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatProgressSpinnerModule,
  MatSidenavModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
} from '@angular/material';
import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {RouterModule} from '@angular/router';
import {TranslateLoader, TranslateModule} from '@ngx-translate/core';
import {TranslateHttpLoader} from '@ngx-translate/http-loader';
import {ChartsModule} from 'ng2-charts';

import {AppComponent} from './app.component';
import {DashboardComponent} from './components/dashboard/dashboard.component';
import {SimpleDialogComponent} from './components/dialogs/simple-dialog/simple-dialog.component';
import {LoginComponent} from './components/login/login.component';
import {MainComponent} from './components/main/main.component';
import {SetupComponent} from './components/setup/setup.component';
import {SpinnerOverlayComponent} from './components/spinner-overlay/spinner-overlay.component';
import {EqualsDirective} from './directives/equals.directive';
import {AuthenticationGuard} from './guards/authentication.guard';
import {FirstTimeSetupGuard} from './guards/first-time-setup.guard';
import {BackendService} from './services/backend.service';
import {SecurityService} from './services/security.service';

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
          { path: 'dashboard', component: DashboardComponent },
        ],
      },
    ], { useHash: true }),

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
  ],
  providers: [
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
