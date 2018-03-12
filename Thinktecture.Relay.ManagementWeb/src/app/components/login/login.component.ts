import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {SecurityService} from '../../services/security.service';

@Component({
  selector: 'trs-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  userName: string;
  password: string;
  rememberMe: boolean;
  submitting: boolean;

  constructor(private readonly _security: SecurityService, private readonly _router: Router, private readonly _route: ActivatedRoute) {
  }

  ngOnInit() {
    const { userName } = this._route.snapshot.queryParams;
    if (userName) {
      this.userName = userName;
      this.rememberMe = true;
    }
  }

  submit() {
    this.submitting = true;

    this._security.authenticate(this.userName, this.password, this.rememberMe).subscribe(
      () => this._router.navigate(['main'], { replaceUrl: true }),
      () => this.submitting = false,
    );
  }
}
