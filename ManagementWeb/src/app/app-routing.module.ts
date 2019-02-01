import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
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
import {SetupComponent} from './components/setup/setup.component';
import {UsersComponent} from './components/users/users.component';
import {AuthenticationGuard} from './guards/authentication.guard';
import {FirstTimeSetupGuard} from './guards/first-time-setup.guard';

const routes: Routes = [
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
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true, paramsInheritanceStrategy: 'always' })],
  exports: [RouterModule],
})
export class AppRoutingModule {
}
