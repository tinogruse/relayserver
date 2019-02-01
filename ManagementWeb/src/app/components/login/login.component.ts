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

  constructor(private readonly security: SecurityService, private readonly router: Router, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    const { userName } = this.route.snapshot.queryParams;
    if (userName) {
      this.userName = userName;
      this.rememberMe = true;
    }
  }

  submit() {
    this.submitting = true;

    this.security.authenticate(this.userName, this.password, this.rememberMe).subscribe(
      () => this.router.navigate(['main'], { replaceUrl: true }),
      () => this.submitting = false,
    );
  }
}
