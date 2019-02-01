import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {Dashboard} from '../../models/dashboard';
import {BackendService} from '../../services/backend.service';

@Component({
  selector: 'trs-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  constructor(private readonly backend: BackendService) {
  }

  info$: Observable<Dashboard>;

  ngOnInit() {
    this.info$ = this.backend.dashboardInfo();
  }
}
