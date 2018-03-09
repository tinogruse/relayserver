import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {SecurityService} from '../../services/security.service';

@Component({
  selector: 'trs-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  username: string;
  password: string;
  rememberMe: boolean;
  submitting: boolean;

  constructor(private readonly _security: SecurityService, private readonly _router: Router) {
  }

  ngOnInit() {
    const username = this._security.username;
    if (username) {
      this.username = username;
      this.rememberMe = true;
    }
  }

  submit() {
    this.submitting = true;

    this._security.authenticate(this.username, this.password, this.rememberMe).subscribe(
      () => this._router.navigate(['main'], { replaceUrl: true }),
      () => this.submitting = false,
    );
  }
}
