import {Component} from '@angular/core';
import {Router} from '@angular/router';
import {SecurityService} from '../../services/security.service';

@Component({
  selector: 'trs-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
})
export class MainComponent {
  constructor(private readonly _security: SecurityService, private readonly _router: Router) {
  }

  readonly menu = [
    { caption: 'Dashboard', icon: 'home', route: ['dashboard'] },
    { caption: 'Users', icon: 'supervisor_account', route: ['users'] },
    { caption: 'Links', icon: 'link', route: ['links'] },
  ];

  logout() {
    this._security.deauthenticate();
    this._router.navigate(['login']);
  }
}
